using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VW;
using VW.Interfaces;
using VW.Labels;
using VW.Serializer.Attributes;

namespace cs_unittest
{
    [TestClass]
    public class TestNull
    {
        [TestMethod]
        public void TestNull1()
        {
            using (var vw = new VowpalWabbit<Context, ADF>("--cb_adf --rank_all --interact ab"))
            {
                var ctx = new Context()
                {
                    ID = 25,
                    ActionDependentFeatures = new [] { 
                        new ADF {
                            ADFID = 23,
                            Label = new ContextualBanditLabel() {
                                Action = 1,
                                Cost= 1,
                                Probability = 0.2f
                            }
                        }
                    }.ToList()
                };

                vw.Learn(ctx);
                vw.Predict(ctx);
            }
        }
    }

    public class ADF : IExample
    {
        [Feature]
        public int ADFID { get; set; }

        [Feature(FeatureGroup = 'b', AddAnchor = true)]
        public float[] Vector {get ;set;}

        public ILabel Label { get; set; }
    }

    public class Context : SharedExample, IActionDependentFeatureExample<ADF>
    {
        [Feature]
        public int ID { get; set; }

        [Feature(FeatureGroup = 'a', AddAnchor = true)]
        public float[] Vector { get; set; }

        public IReadOnlyList<ADF> ActionDependentFeatures { get; set;}
    }
}
