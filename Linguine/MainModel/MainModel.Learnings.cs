using Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    public partial class MainModel
    {
        // TODO - this should be a lot more fleshed out
        // but for now we'll just a basic add and export functionality

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


    }
}
