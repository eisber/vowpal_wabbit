using Microsoft.DecisionService.Exploration;

namespace VowpalWabbit.Prediction
{
    internal class VowpalWabbitPredictorContextualBanditSoftmax : VowpalWabbitPredictorContextualBandit
    {
        internal VowpalWabbitPredictorContextualBanditSoftmax(Model model) : base(model)
        {
        }

        protected override void Predict(MultilineExample example, out float[] scores, out float[] pdf)
        {
            // get scores
            scores = this.Predict(example);

            // generate exploration distribution
            pdf = ExplorationStrategies.GenerateSoftmax(this.model.Lambda, scores);
        }
    }
}
