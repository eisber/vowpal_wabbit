package bz.turtle.readable.input;

/**
 * We compute the hash value only once do not reuse between namespaces because the hash is dependent
 * on the namespace hash
 */
public class Feature implements FeatureInterface {
  private int nameInt;
  private float value = 1f;

  private boolean isStringNameComputed = false;
  /** used so we dont recompute the hash value of the feature */
  public transient int computedHashValue;
  /** used so we dont recompute the hash value of the feature */
  public transient boolean hashIsComputed = false;

  private String name;
  private boolean hasIntegerName;

  public boolean hasIntegerName() {
    return hasIntegerName;
  }

  public Feature() {}

  public Feature(String name) {
    this(name, 1);
  }

  public Feature(int name) {
    this(name, 1);
  }

  public Feature(String name, float value) {
    this.setName(name);
    this.setValue(value);
  }

  public Feature(int name, float v) {
    this.name = null;
    this.isStringNameComputed = false;
    if (name < 0) {
      rename("" + name);
    } else {
      this.setNameInt(name);
    }
    this.setValue(v);
  }

  private static boolean isPositiveInteger(String s) {
    return isPositiveInteger(s, 10);
  }

  private static boolean isPositiveInteger(String s, int radix) {
    if (s.isEmpty()) return false;
    for (int i = 0; i < s.length(); i++) {
      if (i == 0 && s.charAt(i) == '-') {
        return false;
      }
      if (Character.digit(s.charAt(i), radix) < 0) return false;
    }
    return true;
  }

  public static Feature fromString(String featureString) {
    String[] parts = featureString.split(":");
    String name = parts[0];
    float value = 1f;
    if (parts.length > 1) {
      value = Float.parseFloat(parts[1]);
    }
    return new Feature(name, value);
  }

  @Override
  public String toString() {
    return String.format("%s[%d]:%f", getStringName(), this.getIntegerName(), getValue());
  }

  /**
   * change the feature name, which also resets the computed hash
   *
   * @param name - the new feature name
   */
  public void rename(int name) {
    if (name < 0) {
      rename("" + name);
    } else {
      this.setNameInt(name);
      this.isStringNameComputed = false;
      resetIsHashComputed();
    }
  }

  public void rename(String name) {
    this.setName(name);
    resetIsHashComputed();
  }

  /**
   * --hash strings vs --hash all, in case the feature value is integer, there is no need to convert
   * it to string
   */
  public int getIntegerName() {
    return nameInt;
  }

  private void setNameInt(int nameInt) {
    this.nameInt = nameInt;
    this.hasIntegerName = true;
  }

  /** feature value */
  public float getValue() {
    return value;
  }

  public void setValue(float value) {
    this.value = value;
  }

  /** @return the string name, recomputed from nameInt if needed @see getIntegerName */
  public String getStringName() {
    if (!isStringNameComputed) {
      this.name = "" + this.nameInt;
      this.isStringNameComputed = true;
    }
    return name;
  }

  private void setName(String name) {
    if (isPositiveInteger(name)) {
      try {
        setNameInt(Integer.parseInt(name));
      } catch (Exception e) {
      }
    }

    this.isStringNameComputed = true;
    this.name = name;
  }

  public void setComputedHash(int n) {
    this.computedHashValue = n;
    this.hashIsComputed = true;
  }

  public int getComputedHash() {
    return computedHashValue;
  }

  public boolean isHashComputed() {
    return hashIsComputed;
  }

  public void resetIsHashComputed() {
    hashIsComputed = false;
  }
}
