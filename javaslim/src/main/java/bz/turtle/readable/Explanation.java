package bz.turtle.readable;

import java.util.ArrayList;
import java.util.List;

public class Explanation {
  /** count of missing features */
  public Counter missingFeatures = new Counter();
  /** amount of features looked up */
  public Counter featuresLookedUp = new Counter();
  /** sum of predictions of all classess (unnormalized unclipped) */
  public Counter predictions = new Counter();

  public List<String> explanations = new ArrayList<>();

  public void add(String e) {
    explanations.add(e);
  }

  @Override
  public String toString() {
    StringBuilder sb = new StringBuilder();
    sb.append("--------------\n");
    sb.append(String.format("Features Looked Up: %f\n", featuresLookedUp.get()));
    sb.append(String.format("Missing Features: %f\n", missingFeatures.get()));
    sb.append(String.format("Predictions Sum: %f\n\n", predictions.get()));
    explanations.sort(String::compareTo);
    for (String e : explanations) {
      sb.append(e);
      sb.append("\n");
    }
    sb.append("--------------\n");
    return sb.toString();
  }
}
