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
        private TextualMediaManager?            _textualMediaManager;
        private ExternalDictionaryManager?      _externalDictionaryManager;
        private VariantsManager?                _variantsManager;
        private TextualMediaSessionManager?     _textualMediaSessionManager;
        private StatementDatabaseEntryManager?  _statementDatabaseEntryManager;

        private void LoadManagers()
        {
            _externalDictionaryManager      = new ExternalDictionaryManager(Linguine);
            _textualMediaManager            = new TextualMediaManager(Linguine);
            _textualMediaSessionManager     = new TextualMediaSessionManager(Linguine);
            _variantsManager                = new VariantsManager(Linguine);
            _statementDatabaseEntryManager  = new StatementDatabaseEntryManager(Linguine);
        }

        public TextualMediaManager TextualMediaManager
        {
            get
            {
                if (_textualMediaManager is null)
                {
                    throw new Exception("Attempting to read property before model loading complete");
                }
                return _textualMediaManager;
            }
        }

        public ExternalDictionaryManager ExternalDictionaryManager
        {
            get
            {
                if (_externalDictionaryManager is null)
                {
                    throw new Exception("Attempting to read property before model loading complete");
                }
                return _externalDictionaryManager;
            }
        }
        public VariantsManager VariantsManager
        {
            get
            {
                if (_variantsManager is null)
                {
                    throw new Exception("Attempting to read property before model loading complete");
                }
                return _variantsManager;
            }
        }

        private TextualMediaSessionManager TextualMediaSessionManager
        {
            get
            {
                if (_textualMediaSessionManager is null)
                {
                    throw new Exception("Attempting to read property before model loading complete");
                }
                return _textualMediaSessionManager;
            }
        }

        private StatementDatabaseEntryManager StatementDatabaseEntryManager
        {
            get
            {
                if (_statementDatabaseEntryManager is null)
                {
                    throw new Exception("Attempting to read property before model loading complete");
                }
                return _statementDatabaseEntryManager;
            }
        }
    }
}
