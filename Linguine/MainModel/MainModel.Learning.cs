using Infrastructure;
using Learning;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataClasses;
using Config;
using System.Threading.Tasks;
using System.Threading;

namespace Linguine
{
    public partial class MainModel
    {

        private int MaxContextExamples = 5;

        private Random _rng = new Random(Environment.TickCount);


        #region learner list for csv export

        public List<DictionaryDefinition> LearnerList { get; private set; } = new List<DictionaryDefinition>();

        public void AddLearnerListItem(DictionaryDefinition definition)
        {
            LearnerList.Add(definition);
        }

        public bool ExportLearnerListToCSV(string savePath)
        {

            using (StreamWriter writer = new StreamWriter(savePath))
            {
                // writer.WriteLine("Word,Definition"); // import better into Anki without this

                foreach (DictionaryDefinition def in LearnerList)
                {
                    writer.WriteLine($"{def.Word},{def.Definition}");
                }
            }

            LearnerList = new List<DictionaryDefinition>();

            return true;
        }

        #endregion

    }
}
