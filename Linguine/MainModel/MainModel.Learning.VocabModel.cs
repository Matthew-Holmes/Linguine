using Infrastructure;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Linguine
{
    partial class MainModel
    {
        public int? VocabTestWordCount => SM.Services.DefLearning.VocabTestWordCount;

        public Tuple<double[], double[]> GetPKnownByBinnedZipf()
        {
            // TODO - extract the method for waiting for init??

            if (SM.ManagerState == DataManagersState.NoDatabaseYet)
            {
                throw new Exception("no database!");
            }

            while (SM.ManagerState == DataManagersState.Initialising)
            {
                Thread.Sleep(100);
                // TODO - pin down what we want to do for lazy loading!
            }

            if (SM.ManagerState != DataManagersState.Initialised)
            {
                throw new Exception("unexpected data manager state");
            }

            // TODO - where should def learning service go, engines??

            return SM.Services.DefLearning.GetPKnownByBinnedZipf();
        }

        public int DistinctWordsTested
        {
            get
            {
                return SM.Managers!.TestRecords.UniqueDefinitionsTested();
            }
        }
    }
}
