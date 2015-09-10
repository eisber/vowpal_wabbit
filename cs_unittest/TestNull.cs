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
                    Vector = null,
                    ActionDependentFeatures = new [] { 
                        new ADF {
                            ADFID = "23",
                            Label = new ContextualBanditLabel() {
                                Action = 1,
                                Cost= 1,
                                Probability = 0.2f
                            }
                        }
                    }.ToList()
                };

                vw.Learn(ctx);
                var result = vw.Predict(ctx);
                Assert.AreEqual(1, result.Length);
            }
        }

        [TestMethod]
        public void TestNull2()
        {
                using (var vw = new VowpalWabbit<Context, ADF>("--cb_adf --rank_all --interact ab"))
                {
                    var ctx = new Context()
                    {
                        ID = 25,
                        Vector = null,
                    ActionDependentFeatures = new[] { 
                        new ADF {
                            ADFID = "23",
                            Label = new ContextualBanditLabel() {
                                Action = 1,
                                Cost= 1,
                                Probability = 0.2f
                            }
                        }
                    }.ToList()
                    };

                    vw.Learn(ctx);
            }
        }

        [TestMethod]
        public void TestNull3()
        {
            using (var vw = new VowpalWabbit<Context, ADF>("--cb_adf --rank_all --interact ac"))
            {
                var ctx = new Context()
                {
                    ID = 25,
                    Vector = new float [] { 3 },
                    VectorC = new float[] { 2, 2, 3 },
                    ActionDependentFeatures = new[] { 
                        new ADF {
                            ADFID = "23",
                            Label = new ContextualBanditLabel() {
                                Action = 1,
                                Cost= 1,
                                Probability = 0.2f
                            },
                }
                    }.ToList()
                }; 

                vw.Learn(ctx);
            
                ctx.Vector = null; 
                vw.Learn(ctx);

                ctx.Vector = new float[] { 2 };
                ctx.VectorC = null;
                vw.Learn(ctx);

                ctx.Vector = null;
                vw.Learn(ctx);
            }
        }

        [TestMethod]
        public void TestNull4()
        {
            using (var vw = new VowpalWabbit<Context, ADF>("--cb_adf --rank_all --interact ab"))
            {
                var ctx = new Context()
                {
                    ID = 25,
                    Vector = null,
                    ActionDependentFeatures = new[] { 
                    new ADF {
                        ADFID = null,
                        Label = new ContextualBanditLabel() {
                            Action = 1,
                            Cost= 1,
                            Probability = 0.2f
                        }
                    }
                }.ToList()
                };

                vw.Learn(ctx);
                var result = vw.Predict(ctx);
                Assert.AreEqual(1, result.Length);

                ctx.ID = null;
                //ctx.ActionDependentFeatures[0].ADFID = "foo";

                vw.Learn(ctx);
                result = vw.Predict(ctx);
                Assert.AreEqual(1, result.Length);
            }
        }

        [TestMethod]
        public void TestNull5()
        {
            using (var vw = new VowpalWabbit<Context, ADF>("--cb_adf --rank_all --interact ab"))
            {
                var ctx = new Context()
                {
                    ID = 25,
                    ActionDependentFeatures = new[] { 
                        new ADF {
                            ADFID = "123",
                            Label = new ContextualBanditLabel() {
                                Action = 1,
                                Cost= 1,
                                Probability = 0.2f
                            },
                        },
                        new ADF(),
                        new ADF(),
                        new ADF { ADFID = "4"}
                    }.ToList()
                };

                vw.Learn(ctx);
                var result = vw.Predict(ctx);
                Assert.AreEqual(4, result.Length);

                ctx.ActionDependentFeatures[0].ADFID = null;
                ctx.ActionDependentFeatures[3].ADFID = null;

                result = vw.Predict(ctx);
                Assert.AreEqual(4, result.Length);
            }
        }
    }

    public class ADF : IExample
    {
        [Feature]
        public string ADFID { get; set; }

        [Feature(FeatureGroup = 'b', AddAnchor = true)]
        public float[] Vector {get ;set;}

        public ILabel Label { get; set; }
    }

    public class Context : SharedExample, IActionDependentFeatureExample<ADF>
    {
        [Feature]
        public int? ID { get; set; }

        [Feature(FeatureGroup = 'a', AddAnchor = true)]
        public float[] Vector { get; set; }

        [Feature(FeatureGroup = 'c')]
        public float[] VectorC { get; set; }

        public IReadOnlyList<ADF> ActionDependentFeatures { get; set;}
    }
}
