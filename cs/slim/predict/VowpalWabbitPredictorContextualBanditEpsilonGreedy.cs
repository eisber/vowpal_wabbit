using Microsoft.DecisionService.Exploration;

namespace VowpalWabbit.Prediction
{
    internal class VowpalWabbitPredictorContextualBanditEpsilonGreedy : VowpalWabbitPredictorContextualBandit
    {
        internal VowpalWabbitPredictorContextualBanditEpsilonGreedy(Model model) : base(model)
        {
        }

        protected override void Predict(MultilineExample example, out float[] scores, out float[] pdf)
        {
            scores = this.Predict(example);
            // model is trained against cost -> minimum is better
            int topAction = scores.MinIndex();

            pdf = ExplorationStrategies.GenerateEpsilonGreedy(this.model.Epsilon, topAction, scores.Length);
        }
    }
}
