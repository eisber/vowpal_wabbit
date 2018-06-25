using Microsoft.DecisionService.Exploration;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Contextual bandit bag implementation.
    /// </summary>
    internal class VowpalWabbitPredictorContextualBanditBag : VowpalWabbitPredictorContextualBandit
    {
        internal VowpalWabbitPredictorContextualBanditBag(Model model) : base(model)
        {
        }

        protected override void Predict(MultilineExample example, out float[] scores, out float[] pdf)
        {
            scores = null;
            var topActions = new int[example.Examples.Count];

            for (ulong modelIdx = 0; modelIdx < (ulong)this.model.BagSize; modelIdx++)
            {
                scores = this.Predict(example, modelIdx);
                topActions[scores.MinIndex()]++;
            }

            pdf = ExplorationStrategies.GenerateBag(topActions);

            if (this.model.BagMinimumEpsilon > 0)
                ExplorationStrategies.EnforceMinimumProbability(this.model.BagMinimumEpsilon, true, pdf);

            // TODO: it's unclear which scores should be reported back. Maybe the ones from the first model as it sees all unchanged data? 
        }
    }
}
