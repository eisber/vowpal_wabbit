namespace VowpalWabbit.Prediction
{
    public class VowpalWabbitPredictorRegression : VowpalWabbitPredictor
    {
        public VowpalWabbitPredictorRegression(Model model) : base(model)
        {
        }

        public float Predict(Example ex)
        {
            return base.Predict(ex, 0);
        }
    }
}
