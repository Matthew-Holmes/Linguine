using DataClasses;
using Google.Api;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
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

        private Statement            _statement;
        private String               _statementTranslationText;

        public DefinitionResolverViewModel(Statement selectedStatement, int defIndex,
            UIComponents uiComponents, MainModel model, MainViewModel parent) : base(uiComponents, model, parent)
        {
            Title = "Resolve Definition";

            Context = StatementHelper.RunFromStatementDefIndex(selectedStatement, defIndex);

            _statement = selectedStatement;

            Task.Run(() => GoAndGetStatementTranslation());
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

        #endregion
   
        private async Task GoAndGetStatementTranslation()
        {
            StatementTranslation = await _model.GetStatementTranslationText(_statement);
        }
    
    }
}
