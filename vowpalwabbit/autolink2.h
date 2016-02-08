#include "reductions.h"

// this will be defined in base_reduction.h
#define SETUP_VTABLE_PREDICTORLEARN(TREDUCTION) \
  virtual void learn(example& ec, TREDUCTION::Label& label) \
  { predict_or_learn<true>(ec, label);} \
  virtual void predict(example& ec, TREDUCTION::Label& label) \
  { predict_or_learn<false>(ec, label); } \
  template <bool is_learn> \
  void predict_or_learn(TREDUCTION::Label& label);

class AutoLink : public BaseReduction<Label, Label>
{
private:
  uint32_t _d; // degree of the polynomial
  uint32_t _stride_shift;

public:
  AutoLink(vw& all, ILearner<Label>* base_learner);

  virtual bool setup(vw& all);

  SETUP_VTABLE_PREDICTORLEARN(AutoLink);
};


