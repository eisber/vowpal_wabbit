package bz.turtle.readable;

import bz.turtle.readable.input.Feature;
import bz.turtle.readable.input.Namespace;
import bz.turtle.readable.input.PredictionRequest;
import org.junit.Test;

import java.io.File;
import java.io.InputStream;
import java.nio.file.Paths;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotEquals;

public class ReadableModelTest {

  @Test
  public void predictBasic() throws Exception {
    File tdir = new File(this.getClass().getClassLoader().getResource("test").getFile());
    ReadableModel m = new ReadableModel(tdir);
    Explanation e = new Explanation();
    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace(
                        "f",
                        new Feature("a"),
                        new Feature("b"),
                        new Feature("c"),
                        new Feature("odd=-1"))),
                e)[0],
        -1,
        0.01);
    //    System.out.println(e.toString());
  }

  @Test
  public void predictGzip() throws Exception {
    ClassLoader cl = this.getClass().getClassLoader();
    File tdir = new File(cl.getResource("testgz").getFile());
    ReadableModel m = new ReadableModel(tdir);

    m.makeSureItWorks(
        Paths.get(tdir.toString(), "test.txt.gz").toFile(),
        Paths.get(tdir.toString(), "predictions.txt.gz").toFile(),
        false);

    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace(
                        "f",
                        new Feature("a"),
                        new Feature("b"),
                        new Feature("c"),
                        new Feature("odd=-1"))))[0],
        -1,
        0.01);
  }

  @Test(expected = UnsupportedOperationException.class)
  public void brokenInput() throws Exception {
    InputStream is =
        this.getClass().getClassLoader().getResourceAsStream("testgz/readable_model.txt.gz");
    ReadableModel m = new ReadableModel(is);
  }

  @Test
  public void predictBasicFile() throws Exception {
    File tdir = new File(this.getClass().getClassLoader().getResource("example.txt").getFile());
    ReadableModel m = new ReadableModel(tdir);
    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace(
                        "f",
                        new Feature("a"),
                        new Feature("b"),
                        new Feature("c"),
                        new Feature("odd=-1"))))[0],
        -1,
        0.01);
  }

  @Test
  public void predictQuadratic() throws Exception {
    File tdir = new File(this.getClass().getClassLoader().getResource("testq").getFile());
    ReadableModel m = new ReadableModel(tdir);
    // echo "1 |a x z |b x1 z1" | vw -t -i model.bin
    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace("a", new Feature("x"), new Feature("z")),
                    new Namespace("b", new Feature("x1"), new Feature("z1"))))[0],
        -0.0657,
        0.01);
  }

  @Test
  public void testReusableNamespace() throws Exception {
    ReadableModel m = new ReadableModel(this.getClass().getClassLoader().getResource("testq"));

    Namespace a = new Namespace("a", new Feature("x"));
    assertEquals(m.predict(new PredictionRequest(a))[0], -0.0843, 0.01);

    a.rename("b");
    assertEquals(m.predict(new PredictionRequest(a))[0], -0.0421, 0.01);

    a.rename("a");
    assertEquals(m.predict(new PredictionRequest(a))[0], -0.0843, 0.01);
  }

  @Test
  public void testErrorPileup() throws Exception {
    File tdir =
        new File(this.getClass().getClassLoader().getResource("test_error_pileup").getFile());
    ReadableModel m = new ReadableModel(tdir);

    //    tdir = new
    // File(this.getClass().getClassLoader().getResource("test_error_pileup2").getFile());
    //    m = new ReadableModel(tdir);
  }

  @Test
  public void predictQuadraticNumeric() throws Exception {
    File tdir = new File(this.getClass().getClassLoader().getResource("testqnum").getFile());
    ReadableModel m = new ReadableModel(tdir);
    // echo "1 |a numa:2 cat1 cat2 |b numb:4 cat3 cat4" | vw -t -i model.bin
    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace(
                        "a",
                        Feature.fromString("numa:2"),
                        Feature.fromString("cat1"),
                        Feature.fromString("cat2")),
                    new Namespace(
                        "b",
                        Feature.fromString("numb:4"),
                        Feature.fromString("cat3"),
                        Feature.fromString("cat4"))))[0],
        0.864206,
        0.001);
  }

  @Test
  public void testOaa() throws Exception {
    File tdir = new File(this.getClass().getClassLoader().getResource("oaa").getFile());
    ReadableModel m = new ReadableModel(tdir, true, false);

    tdir = new File(this.getClass().getClassLoader().getResource("oaa7").getFile());
    m = new ReadableModel(tdir, true, false);

    tdir = new File(this.getClass().getClassLoader().getResource("oaa10").getFile());
    m = new ReadableModel(tdir, true, false);
  }

  @Test
  public void hashAllVsStrings() throws Exception {
    File tdir = new File(this.getClass().getClassLoader().getResource("testhashnum").getFile());
    ReadableModel m = new ReadableModel(tdir);

    PredictionRequest predictionRequest =
        new PredictionRequest(
            new Namespace("a", new Feature("42"), new Feature("x")),
            new Namespace("b", new Feature(42), new Feature("y")));
    assertEquals(m.predict(predictionRequest)[0], 0.281, 0.01);

    ReadableModel mHashAll =
        new ReadableModel(
            new File(this.getClass().getClassLoader().getResource("testhashall").getFile()));
    assertEquals(mHashAll.predict(predictionRequest)[0], m.predict(predictionRequest)[0], 0.01);

    assertNotEquals(
        mHashAll.featureHashOf(100, new Feature("42")), m.featureHashOf(100, new Feature("42")));

    assertEquals(m.featureHashOf(100, new Feature("42")), m.featureHashOf(100, new Feature(42)));
  }

  @Test
  public void testClip() throws Exception {
    File tdir = new File(this.getClass().getClassLoader().getResource("testclip").getFile());
    ReadableModel m = new ReadableModel(tdir);
    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace(
                        "",
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"))))[0],
        2,
        0.01);

    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace(
                        "",
                        new Feature("neg"),
                        new Feature("neg"),
                        new Feature("neg"),
                        new Feature("neg"),
                        new Feature("neg"),
                        new Feature("neg"),
                        new Feature("neg"))))[0],
        -2,
        0.01);
  }

  @Test
  public void testLinkLogistic() throws Exception {
    File tdir =
        new File(this.getClass().getClassLoader().getResource("testlinklogistic").getFile());
    ReadableModel m = new ReadableModel(tdir);
    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace(
                        "",
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"))))[0],
        0.880797,
        0.0001);
  }

  @Test
  public void testLinkPoisson() throws Exception {
    File tdir = new File(this.getClass().getClassLoader().getResource("testlinkpoisson").getFile());
    ReadableModel m = new ReadableModel(tdir);
    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace(
                        "",
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"))))[0],
        7.38905,
        0.0001);
  }

  @Test
  public void testLinkGlf1() throws Exception {
    File tdir = new File(this.getClass().getClassLoader().getResource("testlinkglf1").getFile());
    ReadableModel m = new ReadableModel(tdir);
    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace(
                        "",
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"))))[0],
        0.76159,
        0.0001);
  }

  @Test
  public void testBrokenOptions() throws Exception {
    File tdir =
        new File(
            this.getClass().getClassLoader().getResource("test_break_options/model.txt").getFile());
    ReadableModel m = new ReadableModel(tdir);

    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace(
                        "",
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"))))[0],
        0.5,
        0.0001);
  }

  @Test
  public void testProbabilities() throws Exception {
    File tdir =
        new File(this.getClass().getClassLoader().getResource("testprobabilities").getFile());
    ReadableModel m = new ReadableModel(tdir, true, true);

    assertEquals(
        m.predict(
                new PredictionRequest(
                    new Namespace(
                        "",
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"),
                        new Feature("pos"))))[0],
        -0.113007,
        0.0001);
  }
}
