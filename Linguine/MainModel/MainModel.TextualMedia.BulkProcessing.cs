using Infrastructure;
using Linguine.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    public partial class MainModel
    {
        internal void StartBulkProcessing(string textName)
        {
            using var context = LinguineFactory.CreateDbContext();
            TextualMedia? tm = TextualMediaManager?.GetByName(textName, context) ?? null;

            if (tm is null)
            {
                throw new Exception("missing textual media manager");
            }

            //StartBulkProcessing(tm);
        }

        internal void StopBulkProcessing(string textName)
        {
            throw new NotImplementedException();
        }

        //StatementManager.IndexOffEndOfLastStatement(tm);
       
    }
}
