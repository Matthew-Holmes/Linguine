using Infrastructure;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public class Statement
    {
        private TextualMedia _parent;
        private String _statementText;
        private List<String> _context;
        private TextDecomposition _injectiveDecomposition;
        private TextDecomposition _rootedDecomposition;

        public TextualMedia Parent { get => _parent; }
        public String StatementText { get => _statementText; }
        public List<String> StatementContext { get => _context; }

        public TextDecomposition InjectiveDecomposition { get => _injectiveDecomposition; }
        public TextDecomposition RootedDecomposition { get => _rootedDecomposition; }

    }
}
