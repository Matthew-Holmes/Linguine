using Infrastructure;
using System;

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
        private DefinitionVocalisationManager?      _definitionVocalisationManager;
        private ExternalDictionary?                 _dictionary;

        private bool _needToImportADictionary;
        private void LoadManagers()
        {
            _externalDictionaryManager          = new ExternalDictionaryManager(LinguineFactory);
            _textualMediaManager                = new TextualMediaManager(LinguineFactory);
            _textualMediaSessionManager         = new TextualMediaSessionManager(LinguineFactory);
            _variantsManager                    = new VariantsManager(LinguineFactory);
            _statementManager                   = new StatementManager(LinguineFactory);
            _parsedDictionaryDefinitionManager  = new ParsedDictionaryDefinitionManager(LinguineFactory);
            _definitionVocalisationManager      = new DefinitionVocalisationManager(LinguineFactory);

            _needToImportADictionary = _externalDictionaryManager.AvailableDictionaries().Count == 0;
            if (!_needToImportADictionary)
            {
                _dictionary = _externalDictionaryManager.GetFirstDictionary();
            }

            HasManagers = true;
        }

        private void LoadServices()
        {
            InitialiseDefinitionLearningService();
        }

        public ExternalDictionary? Dictionary
        {
            get
            {
                if (_needToImportADictionary) { return null; }
                if (_dictionary is null)
                {
                    throw new Exception("Attempting to read property before model loading complete");
                }
                return _dictionary;
            }
        }


        public DefinitionVocalisationManager DefinitionVocalisationManager
        {
            get
            {
                if (_definitionVocalisationManager is null)
                {
                    throw new Exception("Attempting to read property before model loading complete");
                }
                return _definitionVocalisationManager;
            }
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

        private ParsedDictionaryDefinitionManager ParsedDictionaryDefinitionManager
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
