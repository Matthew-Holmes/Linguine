namespace DataClasses
{
    public class Statement
    {
        private TextualMedia _parent;
        private String _statementText;
        private List<String> _context;
        private TextDecomposition _injectiveDecomposition;
        private TextDecomposition _rootedDecomposition;
        private int _firstCharIndex;
        private int _lastCharIndex;

        public int FirstCharIndex { get => _firstCharIndex; }
        public int LastCharIndex { get => _lastCharIndex; }

        public TextualMedia Parent { get => _parent; }
        public String StatementText { get => _statementText; }
        public List<String> StatementContext { get => _context; }

        public TextDecomposition InjectiveDecomposition { get => _injectiveDecomposition; }
        public TextDecomposition RootedDecomposition { get => _rootedDecomposition; }

        public Statement(TextualMedia parent,
                         int firstCharIndex, int lastCharIndex,
                         String statementText,
                         List<String> context,
                         TextDecomposition injectiveDecomposition,
                         TextDecomposition rootedDecomposition)
        {
            _parent = parent;
            _statementText = statementText;
            _context = context;
            _injectiveDecomposition = injectiveDecomposition;
            _rootedDecomposition = rootedDecomposition;
            _firstCharIndex = firstCharIndex;
            _lastCharIndex = lastCharIndex;
        }
    }

    // This is the format used to communicate between the main model and
    // learning extraction module
    public class ProtoStatement
    {
        private String _statementText;
        private List<String> _context;
        private TextDecomposition _injectiveDecomposition;
        private TextDecomposition _rootedDecomposition;

        public String StatementText { get => _statementText; }
        public List<String> StatementContext { get => _context; }

        public TextDecomposition InjectiveDecomposition { get => _injectiveDecomposition; }
        public TextDecomposition RootedDecomposition { get => _rootedDecomposition; }

        public ProtoStatement(
            String text, List<String> context, 
            TextDecomposition injective, TextDecomposition rooted)
        {
            _statementText = text;
            _context = context;
            _injectiveDecomposition = injective;
            _rootedDecomposition = rooted;
        }

     }
}
