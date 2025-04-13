using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Linguine
{
    partial class MainModel
    {
        public int? VocabTestWordCount => DefLearningService?.VocabTestWordCount ?? null;

        public Tuple<double[], double[]> GetPKnownByBinnedZipf()
        {
            while (!HasManagers)
            {
                Thread.Sleep(100);
                // TODO - pin down what we want to do for lazy loading!
            }

            return DefLearningService.GetPKnownByBinnedZipf();
        }

        public int DistintWordsTested
        {
            get
            {
                if (Dictionary is null)
                {
                    throw new Exception("trying to access dictionary before it is available");
                }
                TestRecordsManager? tr = new TestRecordsManager(Dictionary, _linguineDbContextFactory);
                return tr?.NumberDistinctDefinitionsTested() ?? 0;
            }
        }
    }
}
