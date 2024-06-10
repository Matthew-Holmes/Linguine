using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    public partial class MainModel
    {
        public List<Statement>? DoProcessingStep(String textualMediaName)
        {
            TextualMedia? tm = TextualMediaManager.GetByName(textualMediaName);

            if (tm is null) { return null; } // couldn't find a media matching the name

            int end = StatementManager.IndexOffEndOfLastStatement(tm);

            if (end == tm.Text.Length) { return null; } // already processed
                                                        // TODO - what if there is junk at the end of the text?

            // Send a chunk to the statement engine TODO
            // Then the context engine TODO
            // Then text decomposition
            // Then definition resolution



            // Save in database, then return latest, if database connection string changed, throw away the progress


            throw new NotImplementedException();
        }
    }
}
