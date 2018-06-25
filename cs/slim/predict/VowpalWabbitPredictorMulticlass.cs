namespace VowpalWabbit.Prediction
{
    public class VowpalWabbitPredictorMulticlass : VowpalWabbitPredictor
    {
        internal VowpalWabbitPredictorMulticlass(Model model) : base(model)
        {
        }

        public float[] Predict(MultilineExample example)
        {
            return base.Predict(example);
        }
    }
}
