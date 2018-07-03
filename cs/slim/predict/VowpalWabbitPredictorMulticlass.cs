using System;

namespace VowpalWabbit.Prediction
{
    public class VowpalWabbitPredictorMulticlass : VowpalWabbitPredictor
    {
        public VowpalWabbitPredictorMulticlass(Model model) : base(model)
        {
            if (!model.IsCsoaaLdf)
                throw new NotSupportedException("Model must be --csoaa_ldf");
        }

        public float[] Predict(MultilineExample example)
        {
            return base.Predict(example);
        }
    }
}