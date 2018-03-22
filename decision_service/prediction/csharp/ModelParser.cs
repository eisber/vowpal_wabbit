using Newtonsoft.Json;

namespace Microsoft.DecisionService.Prediction
{
    public class ModelJson
    {
        public int bits { get; set; }

        public ulong[] indices { get; set; }

        public ulong[] weights { get; set; }

        public string[] interactions { get; set; }
    }

    public static class ModelParser
    {
        public static Model Parse(string json)
        {
            var modelJson = JsonConvert.DeserializeObject<ModelJson>(json);

            var weights = new float[1 << modelJson.bits];
            for (int i = 0; i < modelJson.indices.Length; i++)
                weights[modelJson.indices[i]] = modelJson.weights[i];

            return new Model(weights);
        }
    }
}