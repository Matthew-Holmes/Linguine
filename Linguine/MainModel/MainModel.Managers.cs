using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    public partial class MainModel
    {
        private TextualMediaManager? _textualMediaManager;
        private ExternalDictionaryManager? _externalDictionaryManager;
        private VariantsManager? _variantsManager;
        private TextualMediaSessionManager? _textualMediaSessionManager;
        private StatementDatabaseEntryManager? _statementDatabaseEntryManager;

        private void LoadManagers()
        {
            _externalDictionaryManager = new ExternalDictionaryManager(Linguine);
            _textualMediaManager = new TextualMediaManager(Linguine);
            _textualMediaSessionManager = new TextualMediaSessionManager(Linguine);
            _variantsManager = new VariantsManager(Linguine);
            _statementDatabaseEntryManager = new StatementDatabaseEntryManager(Linguine);
        }

        public TextualMediaManager? TextualMediaManager
        {
            get
            {
                if (StartupComplete)
                {
                    return _textualMediaManager;
                }
                else
                {
                    return null;
                }
            }
        }

        public ExternalDictionaryManager? ExternalDictionaryManager
        {
            get
            {
                if (StartupComplete)
                {
                    return _externalDictionaryManager;
                }
                else
                {
                    return null;
                }
            }
        }
        public VariantsManager? VariantsManager
        {
            get
            {
                if (StartupComplete)
                {
                    return _variantsManager;
                }
                else
                {
                    return null;
                }
            }
        }

        private TextualMediaSessionManager? TextualMediaSessionManager
        {
            get
            {
                if (StartupComplete)
                {
                    return _textualMediaSessionManager;
                }
                else
                {
                    return null;
                }
            }
        }

        private StatementDatabaseEntryManager? StatementDatabaseEntryManager
        {
            get
            {
                if (StartupComplete)
                {
                    return _statementDatabaseEntryManager;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
