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
        public bool HasManagers { get; private set; } = false;

        private TextualMediaManager?                _textualMediaManager;
        private ExternalDictionaryManager?          _externalDictionaryManager;
        private VariantsManager?                    _variantsManager;
        private TextualMediaSessionManager?         _textualMediaSessionManager;
        private StatementManager?                   _statementManager;
        private ParsedDictionaryDefinitionManager?  _parsedDictionaryDefinitionManager;

        private void LoadManagers()
        {
            _externalDictionaryManager          = new ExternalDictionaryManager(_linguineConnectionString);
            _textualMediaManager                = new TextualMediaManager(_linguineConnectionString);
            _textualMediaSessionManager         = new TextualMediaSessionManager(_linguineConnectionString);
            _variantsManager                    = new VariantsManager(_linguineConnectionString);
            _statementManager                   = new StatementManager(_linguineConnectionString);
            _parsedDictionaryDefinitionManager  = new ParsedDictionaryDefinitionManager(_linguineConnectionString);

            HasManagers = true;
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

        private StatementManager StatementManager
        {
            get
            {
                if (_statementManager is null)
                {
                    throw new Exception("Attempting to read property before model loading complete");
                }
                return _statementManager;
            }
        }

        public ParsedDictionaryDefinitionManager ParsedDictionaryDefinitionManager
        {
            get
            {
                if (_parsedDictionaryDefinitionManager is null)
                {
                    throw new Exception("Attempting to read property before model loading complete");
                }
                return _parsedDictionaryDefinitionManager;
            }
        }
    }
}
