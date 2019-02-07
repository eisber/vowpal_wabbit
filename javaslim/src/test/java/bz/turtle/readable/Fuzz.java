package bz.turtle.readable;

import org.junit.Test;

import java.io.*;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Comparator;
import java.util.Random;

public class Fuzz {

  public static final String alphabet = "abcdefghijklmnopqrstuvwxyz";

  public void runOrExit(String cmd) throws Exception {
    Process p = Runtime.getRuntime().exec(cmd);

    BufferedReader stdInput = new BufferedReader(new InputStreamReader(p.getInputStream()));

    BufferedReader stdError = new BufferedReader(new InputStreamReader(p.getErrorStream()));

    System.out.println(cmd);
    String s = null;
    while ((s = stdInput.readLine()) != null) {
      System.out.println(s);
    }

    while ((s = stdError.readLine()) != null) {
      System.out.println(s);
    }

    p.waitFor();

    if (p.exitValue() != 0) {
      throw new Exception("expected exit code 0, got " + p.exitValue());
    }
  }

  public void runVW(TestSet ts, String optionsTrain, String optionsTest) throws Exception {
    runOrExit(
        String.format(
            "vw -d %s --min_prediction -10000 --max_prediction 10000 --readable_model %s -f %s %s",
            ts.data, ts.model, ts.modelBin, optionsTrain));
    runOrExit(
        String.format("vw -d %s -t -i %s -r %s %s", ts.data, ts.modelBin, ts.pred, optionsTest));
    ReadableModel m = new ReadableModel(ts.model);
    m.makeSureItWorks(ts.data, ts.pred, false);
  }

  @Test
  public void testOaa() throws Exception {
    if (vwfound()) {
      TestSet ts = new TestSet(10, 1000);
      runVW(ts, "--oaa 10", "");
      ts.tearDown();
    }
  }

  @Test
  public void testBits() throws Exception {
    if (vwfound()) {
      TestSet ts = new TestSet(0, 1000);
      runVW(ts, "-b 8", "");
      ts.tearDown();
    }
  }

  @Test
  public void testBasic() throws Exception {
    if (vwfound()) {
      TestSet ts = new TestSet(0, 1000);
      runVW(ts, "", "");
      ts.tearDown();
    }
  }

  @Test
  public void testQuadratic() throws Exception {
    if (vwfound()) {
      TestSet ts = new TestSet(3, 1000);

      runVW(ts, "--oaa 3 -q ab", "");
      runVW(ts, "--oaa 3 -q ab -q cd -q ac -q bc", "");
      ts.tearDown();
    }
  }

  @Test(expected = UnsupportedOperationException.class)
  public void testCubic() throws Exception {
    if (vwfound()) {
      TestSet ts = new TestSet(0, 10);
      try {
        runVW(ts, "--cubic abc", "");
      } finally {
        ts.tearDown();
      }
    }
  }

  @Test(expected = UnsupportedOperationException.class)
  public void testNgram() throws Exception {
    if (vwfound()) {
      TestSet ts = new TestSet(0, 10);
      try {
        runVW(ts, "--ngram 1", "");
      } finally {
        ts.tearDown();
      }
    }
  }

  @Test(expected = UnsupportedOperationException.class)
  public void testSkip() throws Exception {
    if (vwfound()) {
      TestSet ts = new TestSet(0, 10);
      try {
        runVW(ts, "--ngram 2 --skips 1", "");
      } finally {
        ts.tearDown();
      }
    }
  }

  public static class TestSet {
    File tempDir, data, pred, model, modelBin;
    Random r = new Random();

    public String createExample(int klass) {
      StringBuilder sb = new StringBuilder();
      sb.append(klass);
      sb.append(" ");
      for (int nsA = 0; nsA < 5; nsA++) {
        String ns = Character.toString(alphabet.charAt(nsA));
        sb.append("|");
        sb.append(ns);
        sb.append(" ");
        for (int i = 1; i < 10; i++) {
          sb.append("n");
          sb.append((char) (r.nextInt(26) + 'a'));
          sb.append(":");
          sb.append(i);
          sb.append(" ");
        }

        for (int i = 0; i < 10; i++) {
          sb.append("c");
          sb.append((char) (r.nextInt(26) + 'a'));
          sb.append("=");
          sb.append(i);
          sb.append(" ");
        }
      }
      sb.append("\n");
      return sb.toString();
    }

    public TestSet(int nClassess, int nExamples) throws Exception {
      this.tempDir = Files.createTempDirectory("foobar").toFile();
      this.data = Paths.get(this.tempDir.toString(), "test.txt").toFile();
      this.pred = Paths.get(this.tempDir.toString(), "predictions.txt").toFile();
      System.out.println(this.tempDir);
      this.model = Paths.get(this.tempDir.toString(), "readable_model.txt").toFile();
      this.modelBin = Paths.get(this.tempDir.toString(), "model.bin").toFile();
      FileWriter fw = new FileWriter(this.data);
      BufferedWriter writer = new BufferedWriter(fw);

      for (int i = 0; i < nExamples; i++) {
        if (nClassess == 0) {
          writer.write(createExample(1));
          writer.write(createExample(-1));
        } else {
          for (int klass = 1; klass < nClassess; klass++) {
            writer.write(createExample(klass));
          }
        }
      }
      writer.close();
      fw.close();
    }

    public void cleanup(File path) throws IOException {
      Path pathToBeDeleted = Paths.get(path.toURI());

      Files.walk(pathToBeDeleted)
          .sorted(Comparator.reverseOrder())
          .map(Path::toFile)
          .forEach(File::delete);
    }

    public void tearDown() throws Exception {
      cleanup(this.tempDir);
    }
  }

  public boolean vwfound() {
    return new File("/usr/local/bin/vw").exists() || new File("/usr/bin/vw").exists();
  }
}
