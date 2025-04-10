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

        public DefinitionResolverViewModel(Statement selectedStatement, int defIndex,
            UIComponents uiComponents, MainModel model, MainViewModel parent) : base(uiComponents, model, parent)
        {
            Title = "Resolve Definition";

            Context = StatementHelper.RunFromStatementDefIndex(selectedStatement, defIndex);
        }


        public Tuple<string, string, string> Context
        {
            get => _contexts;
            set
            {
                _contexts = value;
                OnPropertyChanged(nameof(Context));
            }
        }
    }
}
