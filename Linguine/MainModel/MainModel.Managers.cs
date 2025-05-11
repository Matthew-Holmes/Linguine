using Infrastructure;
using System;

namespace Linguine
{
    public partial class MainModel
    {
        public bool HasManagers { get; private set; } = false;

        private TextualMediaManager?                _textualMediaManager;
        private DictionaryDefinitionManager?        _dictionaryDefinitionManager;
        private VariantsManager?                    _variantsManager;
        private TextualMediaSessionManager?         _textualMediaSessionManager;
        private StatementManager?                   _statementManager;
        private ParsedDictionaryDefinitionManager?  _parsedDictionaryDefinitionManager;
        private DefinitionVocalisationManager?      _definitionVocalisationManager;

        private bool _needToImportADictionary;
        private void LoadManagers()
        {
            _dictionaryDefinitionManager        = new DictionaryDefinitionManager(ReadonlyLinguineFactory);
            _textualMediaManager                = new TextualMediaManager(ReadonlyLinguineFactory);
            _textualMediaSessionManager         = new TextualMediaSessionManager(ReadonlyLinguineFactory);
            _variantsManager                    = new VariantsManager(ReadonlyLinguineFactory);
            _statementManager                   = new StatementManager(ReadonlyLinguineFactory);
            _parsedDictionaryDefinitionManager  = new ParsedDictionaryDefinitionManager(ReadonlyLinguineFactory);
            _definitionVocalisationManager      = new DefinitionVocalisationManager(ReadonlyLinguineFactory);

            _needToImportADictionary = !_dictionaryDefinitionManager.AnyDefinitions();

            // TODO - some sort of flag in state enum for when need to load a dictionary??

            HasManagers = true;
        }

        private void LoadServices()
        {
            StartDefinitionLearningService();
        }

        public DictionaryDefinitionManager? DictionaryDefinitionManager
        {
            get
            {
                if (_dictionaryDefinitionManager is null)
                {
                    throw new Exception("Attempting to read property before model loading complete");
                }
                return _dictionaryDefinitionManager;
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
