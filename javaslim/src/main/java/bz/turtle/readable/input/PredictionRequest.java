package bz.turtle.readable.input;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * Document to predict on Example
 *
 * <pre>
 *      model.predict(
 *                 new PredictionRequest(
 *                     new Namespace(
 *                         "a",
 *                         new Feature("x"),
 *                         new Feature("y",0.5),
 *                         new Feature("z",0.3))));
 *
 * </pre>
 */
public class PredictionRequest implements Serializable {
  public List<Namespace> namespaces;

  /** request output to be with normalized probabilities */
  public boolean probabilities = false;

  public PredictionRequest() {
    namespaces = new ArrayList<>();
  }

  public PredictionRequest(Namespace... nss) {
    this.namespaces = Arrays.asList(nss);
  }
}
