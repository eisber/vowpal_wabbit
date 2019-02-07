package bz.turtle.readable.input;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * vowpal wabbit namespace check out https://github.com/JohnLangford/vowpal_wabbit/wiki/Input-format
 * for more info
 *
 * <p>We compute the hash value only once do not reuse between models because the hash is dependent
 * on the model seed
 */
public class Namespace implements Serializable {
  public String namespace;
  public List<FeatureInterface> features;

  public transient int computedHashValue;
  public transient boolean hashIsComputed = false;

  public Namespace() {
    this("");
  }

  public Namespace(String ns) {
    this.namespace = ns;
    features = new ArrayList<>();
  }

  @Override
  public String toString() {
    return String.format("{%s: %s}", namespace, features.toString());
  }

  public Namespace(String ns, FeatureInterface... features) {
    this.namespace = ns;
    this.features = Arrays.asList(features);
  }

  /**
   * change the namespace name, which also resets the computed hash, and all feature's computed hash
   *
   * @param name - the new namespace name
   */
  public void rename(String name) {
    this.namespace = name;
    computedHashValue = 0;
    hashIsComputed = false;
    features.forEach(f -> f.resetIsHashComputed());
  }
}
