using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.DecisionService.Prediction
{
    public class ContextualBanditExample
    {
        public Example Shared { get; set; }

        public List<Example> Actions { get; } = new List<Example>();

        public float[] Predict(Model model)
        {
            // merge shared features with actions & predict each action
            return this.Actions.Select(a => model.Predict(a.Merge(this.Shared))).ToArray();
        }

        public string ToJson()
        {
            var json = new StringBuilder();
            json.Append('{');
            if (this.Shared != null)
                json.Append("\"shared\":").Append(this.Shared.ToJson()).Append(',');

            json.Append("\"_multi\":[")
                .Append(string.Join(",", this.Actions.Select(a => a.ToJson())))
                .Append(']');

            json.Append('}');

            return json.ToString();
        }
    }
}