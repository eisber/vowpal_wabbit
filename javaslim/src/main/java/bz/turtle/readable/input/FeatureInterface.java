package bz.turtle.readable.input;

public interface FeatureInterface {
  boolean hasIntegerName();

  int getIntegerName();

  String getStringName();

  float getValue();

  void rename(String name);

  void rename(int name);

  void setComputedHash(int h);

  int getComputedHash();

  void resetIsHashComputed();

  boolean isHashComputed();
}
