using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VW.API;

namespace csharp45
{
    class test
    {
        void x()
        {
            using (var ds = new DecisionService())
            {
                using (var ranking = new ListInt())
                using (var examples = new Examples())
                using (var writer = new ExamplesWriterAuditSerialize())
                {
                    //writer.start_examples(examples);

                    //writer.end_examples();

                    ds.ChooseRanking(examples, ranking);
                }
            }
        }
    }
}
