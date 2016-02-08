#include "autolink2.h"

const int autoconstant = 524267083;

AutoLink::AutoLink(ILearner<Label>* base_learner)
  : BaseReduction(base_learner), _d(0), _stride_shift(0)
{ }

bool AutoLink::setup(vw& all)
{
  if (missing_option<size_t, true>(all, "autolink", "create link function with polynomial d"))
    return false;

  _d = (uint32_t)all.vm["autolink"].as<size_t>();
  _stride_shift = all.reg.stride_shift;

  return true;
}

template <bool is_learn>
void AutoLink::predict_or_learn(example& ec, Label& label);
{ base_predict(ec, label);
  float base_pred = label.scalar;

  // NOTE: restores total_sum_feat_sq even if exception is thrown
  // Alternate name: ChangeTracker, Restorable,...?
  Writable<float> total_sum_feat_sq(ec.total_sum_feat_sq);

  // add features of label
  features_resource fr = ec.push_back(autolink_namespace);
  // ec.indices.push_back(autolink_namespace);
  // features& fs = ec.feature_space[autolink_namespace];
  for (size_t i = 0; i < _d; i++)
    if (base_pred != 0.)
    {
      fs.push_back(base_pred, autoconstant + (i << _stride_shift));
      base_pred *= label.scalar;
    }
  total_sum_feat_sq += fs.sum_feat_sq;

  // no virtual dispatch!
  base_learn_or_predict<is_learn>(ec, label);

  // replaced by Writable<float>::~Writable()
  // ec.total_sum_feat_sq -= fs.sum_feat_sq;

  // NOTE: frees memory even if exception is thrown
  // replaced by features_resource::~features_resource();
  // fs.erase();
  // ec.indices.pop();
}

