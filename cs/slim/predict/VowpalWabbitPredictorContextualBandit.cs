using Microsoft.DecisionService.Exploration;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Base class for contextual bandit predictors.
    /// </summary>
    public abstract class VowpalWabbitPredictorContextualBandit : VowpalWabbitPredictor
    {
        internal VowpalWabbitPredictorContextualBandit(Model model) : base(model)
        {
        }
        protected abstract void Predict(MultilineExample example, out float[] scores, out float[] pdf);

        /// <summary>
        /// Produces a ranking for the given example and the seed (=eventId).
        /// </summary>
        /// <param name="eventId">Seed for randomization.</param>
        /// <param name="example">The current context.</param>
        /// <returns>A ranking.</returns>
        public Ranking Predict(string eventId, MultilineExample example)
        {
            this.Predict(example, out float[] scores, out float[] pdf);

            return new Ranking
            {
                Indicies = Sampling.Sample(eventId, pdf, scores, out bool _),
                Probabilities = pdf
            };
        }
    }
}
