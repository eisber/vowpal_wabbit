package bz.turtle.readable;

import bz.turtle.readable.input.Feature;
import bz.turtle.readable.input.FeatureInterface;
import bz.turtle.readable.input.Namespace;
import bz.turtle.readable.input.PredictionRequest;

import java.io.*;
import java.net.URL;
import java.nio.file.Paths;
import java.util.*;
import java.util.function.BiConsumer;
import java.util.function.DoubleUnaryOperator;
import java.util.zip.GZIPInputStream;

/**
 * Reades Vowpal Wabbit --readable_model file and creates a weights array containing the weight per
 * bucket then using same hashing of vw finds the correct bucket for the input features and computes
 * the inner product.
 *
 * <p>check out this <a
 * href="https://gist.github.com/luoq/b4c374b5cbabe3ae76ffacdac22750af">gist</a> and <a
 * href="https://github.com/JohnLangford/vowpal_wabbit/wiki/Feature-Hashing-and-Extraction">Feature-Hashing-and-Extraction</a>
 * for more information
 *
 * <p>Example:
 *
 * <pre>
 * execute: echo "1 |ns a b c:4" | vw --readable_model /tmp/readable_model.txt
 *
 *
 *
 * ReadableModel m = new ReadableModel(new File("/tmp/readable_model.txt"));
 * float[] p = m.predict(new PredictionRequest(new Namespace("ns", new Feature("a"), new Feature("c",3)));
 * System.out.println(Arrays.toString(p));
 *
 * </pre>
 *
 * ReadableModel is thread safe, so make sure to reuse it between threads
 */
public class ReadableModel {
  private static final int intercept = 11650396;
  private final int FNV_prime = 16777619;

  private boolean hasIntercept = true;
  /**
   * This is the actual model of size 2**bits if you build something with vw -b 18 it will be of
   * size 262144
   */
  private float[] weights;

  private int bits;

  private int oaa = 1;
  private int mask = 0;
  private int multiClassBits = 0;
  private int seed = 0;
  private boolean hashAll = false;

  private float minLabel = 0;
  private float maxLabel = 0;

  private int ngram = 0;
  private int skip = 0;
  // -q ab
  // -q ac
  private Map<Character, Set<Character>> quadratic = new HashMap<>();
  private boolean quadraticAnyToAny = false;

  private DoubleUnaryOperator identity = DoubleUnaryOperator.identity();
  private DoubleUnaryOperator logistic = (o) -> (1. / (1. + Math.exp(-o)));
  private DoubleUnaryOperator glf1 = (o) -> (2. / (1. + Math.exp(-o)) - 1.);
  private DoubleUnaryOperator poisson = (o) -> Math.exp(o);

  private DoubleUnaryOperator link = this.identity;

  // XXX: incomplete
  private void extractOptions(String o, BiConsumer<String, String> cb) {
    o = o.trim();
    if (o.isEmpty()) return;
    String[] op = o.split("\\s+");
    for (int i = 0; i < op.length; i++) {
      if (op[i].contains("=")) {
        String[] splitted = op[i].split("=");
        cb.accept(splitted[0], splitted[1]);
      } else {
        cb.accept(op[i], op[i + 1]);
        i++; // skip 2 at a time
      }
    }
  }

  private String getSecondValue(String s) {
    String[] splitted = s.split(":");
    if (splitted.length == 1) {
      return "";
    }
    return splitted[1].trim();
  }

  private int intOrZero(String s) {
    if (s.equals("")) {
      return 0;
    }
    return Integer.parseInt(s);
  }

  private InputStream getReaderForExt(File f) throws IOException {
    if (f.toString().endsWith(".gz")) {
      FileInputStream fin = new FileInputStream(f);
      InputStream gzipStream = new GZIPInputStream(fin);
      return gzipStream;
    } else {
      return new FileInputStream(f);
    }
  }

  private File findFileWithExt(File root, String name) {
    File x = Paths.get(root.toString(), name + ".gz").toFile();
    if (x.exists()) {
      return x;
    }
    return Paths.get(root.toString(), name).toFile();
  }
  /**
   * Loads the model from a file output from vw --readable_file. The contents of the file looks like
   * this:
   *
   * <pre>
   *   Version 8.6.1
   *   Id
   *   Min label:0
   *   Max label:3
   *   bits:18
   *   lda:0
   *   1 ngram:2
   *   0 skip:
   *   options: --hash_seed 0 --link identity
   *   Checksum: 3984224786
   *   :0
   *   116060:0.532933
   *   155256:0.192113
   *   213375:0.390151
   *   218329:0.158008
   *   250698:0.192113
   *   259670:0.343652
   * </pre>
   *
   * <b>155256:0.192113</b> is hash bucket:weight, we use the same hashing algorithm as vw to find
   * the features in the model
   *
   * @param file the vw --readable_model file.txt, also supports .gz and will automatically
   *     decompress
   * @throws IOException if there is a problem with the reading
   * @throws UnsupportedOperationException if the model was built with options we dont support yet
   */
  public void loadReadableModel(File file) throws IOException, UnsupportedOperationException {
    InputStream is = getReaderForExt(file);
    loadReadableModel(is);
  }

  public void loadReadableModel(InputStream is) throws IOException, UnsupportedOperationException {
    BufferedReader br = new BufferedReader(new InputStreamReader(is));
    bits = 0;
    boolean inHeader = true;
    multiClassBits = 0;
    // TODO: more robust parsing
    try {
      String line;
      while ((line = br.readLine()) != null) {
        if (inHeader) {
          if (line.equals(":0")) {
            inHeader = false;
          }
          if (line.contains("bits:")) {
            bits = Integer.parseInt(getSecondValue(line));
            weights = new float[(1 << bits)];
          }
          if (line.contains("Min label")) {
            minLabel = Float.parseFloat(getSecondValue(line));
          }
          if (line.contains("Max label")) {
            maxLabel = Float.parseFloat(getSecondValue(line));
          }
          if (line.contains("ngram")) {
            ngram = intOrZero(getSecondValue(line));
            if (ngram != 0) {
              throw new UnsupportedOperationException("ngrams are not supported yet");
            }
          }
          if (line.contains("skip")) {
            skip = intOrZero(getSecondValue(line));
            if (skip != 0) {
              throw new UnsupportedOperationException("skip is not supported yet");
            }
          }

          if (line.contains("options")) {
            extractOptions(
                line.split(":", 2)[1],
                (key, value) -> {
                  if (key.equals("--oaa")) {
                    oaa = Integer.parseInt(value);

                    multiClassBits = 0;
                    int ml = oaa - 1;
                    while (ml > 0) {
                      multiClassBits++;
                      ml >>= 1;
                    }
                  }
                  if (key.equals("--cubic")) {
                    throw new UnsupportedOperationException("we do not support --cubic yet");
                  }
                  if (key.equals("--link")) {
                    switch (value) {
                      case "logistic":
                        this.link = this.logistic;
                        break;
                      case "identity":
                        this.link = this.identity;
                        break;
                      case "poisson":
                        this.link = this.poisson;
                        break;
                      case "glf1":
                        this.link = this.glf1;
                        break;
                      default:
                        throw new UnsupportedOperationException(
                            "only --link identity, logistic, glf1, or poisson are supported "
                                + value);
                    }
                  }

                  if (key.equals("--hash_seed")) {
                    seed = Integer.parseInt(value);
                  }
                  if (key.equals("--hash")) {
                    if (value.equals("all")) {
                      hashAll = true;
                    }
                  }
                  if (key.equals("--quadratic")) {
                    if (value.equals("::")) {
                      // TODO: the way we do permutation differs from the way we do permutations
                      // TODO: the results will differ
                      /*

                      from vw
                      echo '1 |aa x:1 y:2 ' | vw -f x.bin -a -q :: 2>&1 | grep Constant | tr "\t" "\n" | sort
                      aa^x*aa^x:113732:1:0@0
                      aa^x*aa^y:189809:2:0@0
                      aa^x:63954:1:0@0
                      aa^y*aa^y:125762:4:0@0
                      aa^y:237799:2:0@0

                      and we generate
                      aa^x:63954:1:0.000000
                      aa^y:237799:1:0.000000
                      aa^x*aa^x:113732:1:0.000000
                      aa^x*aa^y:189809:1:0.000000
                      aa^y*aa^x:176759:1:0.000000
                      aa^y*aa^y:125762:1:0.000000
                       */
                      quadraticAnyToAny = true;
                    } else {
                      quadratic
                          .computeIfAbsent(value.charAt(0), k -> new HashSet<>())
                          .add(value.charAt(1));
                    }
                  }
                  // TODO: --cubic
                  // TODO: ngrams, skips
                  // TODO: lda
                });
          }
        } else {
          String[] v = line.split(":");
          int bucket = Integer.parseInt(v[0]);
          float w = Float.parseFloat(v[1]);
          weights[bucket] = w;
        }
      }
    } finally {
      br.close();
    }

    mask = (1 << bits) - 1;
    if (weights == null) {
      throw new UnsupportedOperationException("failed to load the model, did not see 'bits:' line");
    }
  }

  /**
   * Takes a file or directory and loads the model.
   *
   * <p>If you pass a directory as input it will look for 3 files
   *
   * <ul>
   *   <li>readable_model.txt
   *   <li>test.txt
   *   <li>predictions.txt
   * </ul>
   *
   * If test.txt and predictions.txt exists it will automatically run makeSureItWorks() (or
   * test.txt.gz, predictions.txt.gz readable_model.txt.gz)
   *
   * <p>If you pass a file it will just load the model
   *
   * @param root file or directory to read from
   * @param hasIntercept model was built without --nocache option
   * @param probabilities if file is directory and predictions.txt exist, test there with normalized
   *     probabilities
   * @throws IOException if reading fails
   * @throws UnsupportedOperationException if the model was built with options we dont support yet
   */
  public ReadableModel(File root, boolean hasIntercept, boolean probabilities)
      throws IOException, UnsupportedOperationException {

    this.hasIntercept = hasIntercept;
    if (root.isDirectory()) {
      File model = findFileWithExt(root, "readable_model.txt");
      File test = findFileWithExt(root, "test.txt");
      File predictions = findFileWithExt(root, "predictions.txt");
      loadReadableModel(model);

      if (test.exists() && predictions.exists()) {
        makeSureItWorks(test, predictions, probabilities);
      }
    } else {
      loadReadableModel(root);
    }
  }

  public ReadableModel(InputStream is) throws IOException, UnsupportedOperationException {
    this(is, true);
  }

  public ReadableModel(InputStream is, boolean hasIntercept)
      throws IOException, UnsupportedOperationException {

    this.hasIntercept = hasIntercept;
    loadReadableModel(is);
  }

  public ReadableModel(URL root, boolean hasIntercept)
      throws IOException, UnsupportedOperationException {
    this(new File(root.getFile()), hasIntercept);
  }

  public ReadableModel(File root, boolean hasIntercept)
      throws IOException, UnsupportedOperationException {
    this(root, hasIntercept, false);
  }

  public ReadableModel(URL root) throws IOException, UnsupportedOperationException {
    this(new File(root.getFile()), true, false);
  }

  public ReadableModel(File root) throws IOException, UnsupportedOperationException {
    this(root, true, false);
  }

  /**
   * read the test file and pred file and try to do the same predictions
   *
   * @param testInputStream input stream with one example per line
   * @param predictionsInputStream input stream with output from vw -t -i model --predictions -r
   * @param probabilities predictions.txt contains probabilities
   * @throws IOException if file reading fails
   * @throws IllegalStateException if predictions mismatch
   */
  public void makeSureItWorks(
      InputStream testInputStream, InputStream predictionsInputStream, boolean probabilities)
      throws IOException, IllegalStateException {
    BufferedReader brTest = new BufferedReader(new InputStreamReader(testInputStream));
    BufferedReader brPred = new BufferedReader(new InputStreamReader(predictionsInputStream));

    int lineNum = 0;
    try {
      String testLine;
      String predLine;

      while ((testLine = brTest.readLine()) != null && ((predLine = brPred.readLine()) != null)) {
        String[] test = testLine.split("\\s+");
        PredictionRequest predictionRequest = new PredictionRequest();
        predictionRequest.probabilities = probabilities;
        boolean hasNamespace = false;
        for (int i = 0; i < test.length; i++) {
          // label |ns f:value f f f \ns
          if (test[i].startsWith("|")) {
            hasNamespace = true;
            String ns = test[i].replaceFirst("\\|", "");
            predictionRequest.namespaces.add(new Namespace(ns));
          } else if (hasNamespace) {

            float weight = 1;
            String feature = test[i];
            if (test[i].contains(":")) {
              String[] s = test[i].split(":");
              feature = s[0];
              weight = Float.parseFloat(s[1]);
            }

            predictionRequest
                .namespaces
                .get(predictionRequest.namespaces.size() - 1)
                .features
                .add(new Feature(feature, weight));
          }
        }
        float[] ourPrediction = predict(predictionRequest);

        // ran with --probabilities for -oaa
        if (predLine.contains(":")) {
          String[] perKlass = predLine.split(" ");
          for (int i = 0; i < perKlass.length; i++) {
            String[] kv = perKlass[i].split(":");
            int index = Integer.parseInt(kv[0]) - 1;
            float pred = Float.parseFloat(kv[1]);

            if (Math.abs(pred - ourPrediction[index]) > 0.01) {
              throw new IllegalStateException(
                  String.format(
                      "line: %d index %d, prediction: %f, ourPrediction: %f \noaa %s,\npred line: %s\ntest line: %s",
                      lineNum,
                      index,
                      pred,
                      ourPrediction[index],
                      Arrays.toString(ourPrediction),
                      predLine,
                      testLine));
            }
          }
        } else {
          float pred = Float.parseFloat(predLine);

          if (Math.abs(pred - ourPrediction[0]) > 0.01) {
            throw new IllegalStateException(
                String.format(
                    "line: %d index %d, prediction: %f, ourPrediction: %f \noaa %s,\npred line: %s\ntest line: %s",
                    lineNum,
                    0,
                    pred,
                    ourPrediction[0],
                    Arrays.toString(ourPrediction),
                    predLine,
                    testLine));
          }
        }
        lineNum++;
      }
    } finally {
      brPred.close();
      brTest.close();
    }
  }

  /**
   * read the test file and pred file and try to do the same predictions
   *
   * @param testFile file with one example per line
   * @param predFile file output from vw -t -i model --predictions -r
   * @param probabilities predictions.txt contains probabilities
   * @throws IOException if file reading fails
   * @throws IllegalStateException if predictions mismatch
   */
  public void makeSureItWorks(File testFile, File predFile, boolean probabilities)
      throws IOException, IllegalStateException {
    /* to make sure we predict the same values as VW */

    InputStream isTest = getReaderForExt(testFile);
    InputStream isPred = getReaderForExt(predFile);
    makeSureItWorks(isTest, isPred, probabilities);
  }

  private int getBucket(int featureHash, int klass) {
    return ((featureHash << multiClassBits) | klass) & mask;
  }

  /**
   * @param mmNamespaceHash the namespace hash VWMurmur.hash(namespace, seed) where seed is usually
   *     0 unless you pass --hash_seed to vw
   * @param feature the feature to compute hash of
   * @return the hash of the feature according to vw
   *     <p>check out
   *     https://github.com/JohnLangford/vowpal_wabbit/blob/579c34d2d2fd151b419bea54d9921fc7f3f55bbc/vowpalwabbit/parse_primitives.cc#L48
   */
  public int featureHashOf(int mmNamespaceHash, FeatureInterface feature) {
    if (hashAll) {
      return VWMurmur.hash(feature.getStringName(), mmNamespaceHash);
    } else {
      if (feature.hasIntegerName()) return feature.getIntegerName() + mmNamespaceHash;
      return VWMurmur.hash(feature.getStringName(), mmNamespaceHash);
    }
  }

  /**
   * really usefull if you want to score a list of items and dont want to be in the mercy of escape
   * analysis
   *
   * @return float array with enough elements to hold one prediction per class
   */
  public float[] getReusableFloatArray() {
    return new float[oaa];
  }

  /**
   * @param input PredictionRequest to evaluate
   * @return prediction per class
   */
  public float[] predict(PredictionRequest input) {
    return predict(input, null);
  }

  /**
   * @param input PredictionRequest to evaluate
   * @param explain Explanation if you want to get some debug information about the prediction query
   * @return prediction per class
   */
  public float[] predict(PredictionRequest input, Explanation explain) {
    float[] out = getReusableFloatArray();
    predict(out, input, explain);
    return out;
  }

  private void interact(
      float[] result,
      Namespace ans,
      FeatureInterface a,
      Namespace bns,
      FeatureInterface b,
      Explanation explain) {
    int fnv = ((a.getComputedHash() * FNV_prime) ^ b.getComputedHash());
    for (int klass = 0; klass < oaa; klass++) {
      int bucket = getBucket(fnv, klass);
      if (explain != null) {
        explain.add(
            String.format(
                "%s^%s*%s^%s:%d:%d:%f",
                ans.namespace,
                a.getStringName(),
                bns.namespace,
                b.getStringName(),
                bucket,
                klass + 1,
                weights[bucket]));
        if (weights[bucket] == 0) {
          explain.missingFeatures.add(1);
        }
        explain.featuresLookedUp.add(1);
      }

      result[klass] += a.getValue() * b.getValue() * weights[bucket];
    }
  }
  /**
   * @param result place to put result in (@see getReusableFloatArray)
   * @param input PredictionRequest to evaluate
   * @param explain Explanation if you want to get some debug information about the prediction query
   */
  public void predict(float[] result, PredictionRequest input, Explanation explain) {
    for (int klass = 0; klass < oaa; klass++) result[klass] = 0;

    // TODO: ngrams skips
    // TODO: --cubic hash calculation

    input.namespaces.forEach(
        n -> {
          if (!n.hashIsComputed) {
            int namespaceHash = n.namespace.length() == 0 ? 0 : VWMurmur.hash(n.namespace, seed);
            n.computedHashValue = namespaceHash;
            n.hashIsComputed = true;
          }

          n.features.forEach(
              f -> {
                if (!f.isHashComputed()) {
                  int featureHash = featureHashOf(n.computedHashValue, f);
                  f.setComputedHash(featureHash);
                }
                for (int klass = 0; klass < oaa; klass++) {
                  int bucket = getBucket(f.getComputedHash(), klass);
                  if (explain != null) {
                    explain.add(
                        String.format(
                            "%s^%s:%d:%d:%f",
                            n.namespace, f.getStringName(), bucket, klass + 1, weights[bucket]));
                    if (weights[bucket] == 0) {
                      explain.missingFeatures.add(1);
                    }
                    explain.featuresLookedUp.add(1);
                  }
                  result[klass] += f.getValue() * weights[bucket];
                }
              });
        });

    if (quadratic.size() > 0 || quadraticAnyToAny) {

      // foreach namespace nsA
      //    foreach interacting namespaces nsB
      //       foreach nsA.features a
      //         foreach nsB.feature b
      //            bucket = ((a.computedHashValue * FNV_prime) ^ b.computedHashValue);

      if (quadraticAnyToAny) {
        input.namespaces.forEach(
            ans ->
                input.namespaces.forEach(
                    bns -> {
                      ans.features.forEach(
                          a -> {
                            bns.features.forEach(
                                b -> {
                                  interact(result, ans, a, bns, b, explain);
                                });
                          });
                    }));
      } else {
        input.namespaces.forEach(
            ans -> {
              Set<Character> interactStartingWith = quadratic.get(ans.namespace.charAt(0));
              if (interactStartingWith == null) return;
              interactStartingWith.forEach(
                  inter -> {
                    // instead of building a hash Map<Character, List<Namespace>>
                    // it should be better to just scan the list and see if anything matches
                    input.namespaces.forEach(
                        bns -> {
                          if (bns.namespace.charAt(0) == inter) {
                            ans.features.forEach(
                                a -> {
                                  bns.features.forEach(
                                      b -> {
                                        interact(result, ans, a, bns, b, explain);
                                      });
                                });
                          }
                        });
                  });
            });
      }
    }

    if (hasIntercept) {
      for (int klass = 0; klass < oaa; klass++) {
        int bucket = getBucket(intercept, klass);
        if (explain != null) {
          explain.add(String.format("%s:%d:%d:%f", "Constant", bucket, klass + 1, weights[bucket]));
          if (weights[bucket] == 0) {
            explain.missingFeatures.add(1);
          }
          explain.featuresLookedUp.add(1);
        }

        result[klass] += weights[bucket];
      }
    }

    if (explain != null) {
      // uncliped unnormalized pred historuy
      for (int klass = 0; klass < oaa; klass++) {
        explain.predictions.add(result[klass]);
      }
    }

    if (input.probabilities) {
      this.clip(result);
      this.linkWith(result, this.logistic);
      if (this.oaa > 1) this.normalize(result);
    } else {
      this.clip(result);
      this.linkWith(result, link);
    }
  }

  protected void clip(float[] raw_out) {
    for (int klass = 0; klass < this.oaa; klass++) {
      raw_out[klass] = clip(raw_out[klass]);
    }
  }

  protected float clip(float raw_out) {
    return Math.max(Math.min(raw_out, this.maxLabel), this.minLabel);
  }

  protected void linkWith(float[] out, DoubleUnaryOperator f) {
    for (int klass = 0; klass < this.oaa; klass++) {
      out[klass] = (float) f.applyAsDouble(out[klass]);
    }
  }

  protected void normalize(float[] out) {
    float sum = 0;
    for (float o : out) sum += o;
    for (int klass = 0; klass < this.oaa; klass++) {
      out[klass] = out[klass] / sum;
    }
  }
}
