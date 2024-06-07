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
        /*
        List<Statement>? GetStatementsCoveringRange(String textualMediaName, int start, int stop)
        {
            TextualMedia? tm = TextualMediaManager?.GetByName(textualMediaName) ?? null;

            if (tm is null) { return null; } // couldn't find a media matching the name

            List<StatementDatabaseEntry>? found = StatementDatabaseEntryManager?.GetStatementsCoveringRangeWithEndpoints(
                tm, start, stop) ?? null;

            if (found is null) { return null; }

            int oldCount = found.Count;
            found = statManager.PrependUpToContextCheckpoint(found);
            int bookMark = found.Count - oldCount;

            var raw = statManager.AttachDefinitions(found);

            return StatementFactory.FromDatabaseEntries(raw).Skip(bookMark).ToList();
        }

        public List<Statement>? DoProcessingStep(String textualMediaName)
        {
            if (TextualMediaManager is null || StatementDatabaseEntryManager is null) { return null; }

            TextualMediaManager             tmManager = TextualMediaManager;
            StatementDatabaseEntryManager statManager = StatementDatabaseEntryManager;

            TextualMedia? tm = tmManager.GetByName(textualMediaName);

            if (tm is null) { return null; } // couldn't find a media matching the name

            int end = StatementDatabaseEntryManager.IndexOffEndOfLastStatement(tm);

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
        */
    }
}
