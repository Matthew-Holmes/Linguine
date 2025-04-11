using DataClasses;
using Google.Api;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInputInterfaces;

namespace Linguine.Tabs
{
    class DefinitionResolverViewModel : TabViewModelBase
    {
        private Tuple<string, string, string>? _contexts;
        private Tuple<string, string, string, string>? _bestTranslatedAsText;

        private int                  _defIndex;
        private String               _wordText;
        private Statement            _statement;
        private String               _statementTranslationText;

        public DefinitionResolverViewModel(Statement selectedStatement, int defIndex,
            UIComponents uiComponents, MainModel model, MainViewModel parent) : base(uiComponents, model, parent)
        {
            Title = "Resolve Definition";

            Context = StatementHelper.RunFromStatementDefIndex(selectedStatement, defIndex);

            _statement = selectedStatement;
            _defIndex  = defIndex;
            _wordText = selectedStatement.InjectiveDecomposition.Units[defIndex].Trim();

            Task.Run(() => GoAndGetDetailsForUser());
        }

        #region UI properties

        public Tuple<string, string, string> Context
        {
            get => _contexts;
            set
            {
                _contexts = value;
                OnPropertyChanged(nameof(Context));
            }
        }

        public String StatementTranslation
        {
            get => _statementTranslationText;
            set
            {
                _statementTranslationText = value;
                OnPropertyChanged(nameof(StatementTranslation));
            }
        }

        public Tuple<string, string, string, string> BestTranslatedAsTexts
        {
            get => _bestTranslatedAsText;
            set
            {
                _bestTranslatedAsText = value;
                OnPropertyChanged(nameof(BestTranslatedAsTexts));
            }
        }

        #endregion
   
        private async Task GoAndGetDetailsForUser()
        {
            StatementTranslation = await _model.GetStatementTranslationText(_statement);
            
            string bestWordTranslation  = await _model.GetBestWordTranslation(_statement, _defIndex);
            bestWordTranslation = bestWordTranslation.Trim();

            ForBestTranslation fixedParts = TextFactory.BestTranslatedAsString(_model.Native);

            BestTranslatedAsTexts = Tuple.Create(fixedParts.inThisStatementIThink, _wordText, fixedParts.isBestTranslatedAs, bestWordTranslation);
        }
    
    }
}
