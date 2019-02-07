package bz.turtle.readable;

public class Counter {
  public float sum = 0;
  public int count = 0;

  @Override
  public String toString() {
    return String.format("%f/%d=%f", sum, count, get());
  }

  public void add(float f) {
    sum += f;
    count++;
  }

  /** @return sum/count */
  public float get() {
    if (count == 0) return 0;
    return sum / count;
  }

  public void clear() {
    sum = 0;
    count = 0;
  }
}
