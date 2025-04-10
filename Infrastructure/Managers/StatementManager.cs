using DataClasses;
using Infrastructure.AntiCorruptionLayer;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure
{
    public class StatementManager : ManagerBase
    {
        private StatementDatabaseEntryManager _databaseManager;

        public StatementManager(LinguineDbContextFactory dbf) : base(dbf)
        {
            _databaseManager = new StatementDatabaseEntryManager(dbf);
        }

        public List<Statement> GetAllStatementsFor(TextualMedia tm)
        {
            List<StatementDatabaseEntry> entries = _databaseManager.GetAllStatementsEntriesFor(tm);

            return StatementFactory.FromDatabaseEntries(_databaseManager.AttachDefinitions(entries));
        }

        public void AddTranslation(StatementTranslation st)
        {
            int? statementKey = st.Underlying.ID;

            if (statementKey is null)
            {
                throw new Exception("only call this when the linked statment has a database reference");
            }

            int skey = (int)statementKey;

            using var context = _dbf.CreateDbContext();

            TranslatedStatementDatabaseEntry toAdd = new TranslatedStatementDatabaseEntry
            {
                StatementKey = skey,
                TranslatedLanguage = st.TranslatedLanguage,
                Translation = st.Translation
            };

            context.Add(toAdd);
            context.SaveChanges();

        }

        public List<StatementTranslation> GetTranslations(Statement statement, LearnerLevel lvl, LanguageCode lc)
        {

            if (statement.ID is null)
            {
                throw new Exception("only call this method for statements retrieved from the database");
            }

            int id = (int)statement.ID;

            using var context = _dbf.CreateDbContext();
            
            // go through the anti-corruption layer

            List<TranslatedStatementDatabaseEntry> entries = context.TranslatedStatements
                .Where(e => e.StatementKey == id)
                .Where(e => e.TranslatedLanguage == lc)
                .ToList();

            // return steps back out

            return entries.Select(e => new StatementTranslation
            {
                DatabasePrimaryKey = e.DatabasePrimaryKey,
                Underlying = statement,
                TranslatedLanguage = lc,
                Translation = e.Translation
            }).ToList();
        }

        public List<Statement> GetNStatementsFor(DictionaryDefinition def, int n)
        {
            List<StatementDatabaseEntry> entries = _databaseManager.GetNStatementsFor(def, n);

            List<Statement> ret = new List<Statement>();

            using var context = _dbf.CreateDbContext();

            foreach (StatementDatabaseEntry entry in entries)
            {
                List<StatementDefinitionNode> nodes = _databaseManager.AttachDefinition(entry, context).Item2;
                
                ret.Add(StatementFactory.FromDatabaseEntry(entry, nodes));
            }

            return ret;
        }

        public List<int> StatementStartIndices(TextualMedia tm)
        {
            using var context = _dbf.CreateDbContext();
            context.Attach(tm);
            return context.Statements.Where(s => s.Parent == tm).Select(s => s.FirstCharIndex).ToList();
        }

        public List<Statement> GetStatementsCoveringRange(TextualMedia tm, int start, int stop)
        {
            List<StatementDatabaseEntry> found = _databaseManager.GetStatementsCoveringRangeWithEndpoints(tm, start, stop);

            if (found.Count == 0) { return new List<Statement>(); }

            int oldCount = found.Count;
            found = _databaseManager.PrependUpToContextCheckpoint(found);
            int bookMark = found.Count - oldCount;

            var raw = _databaseManager.AttachDefinitions(found);

            return StatementFactory.FromDatabaseEntries(raw).Skip(bookMark).ToList();
        }

        public int IndexOffEndOfLastStatement(TextualMedia tm, LinguineDbContext context)
        {
            context.Attach(tm);
            var statements = context.Statements.Where(s => s.Parent == tm);
            if (!statements.Any())
            {
                return -1;
            }
            return statements.Max(s => s.LastCharIndex) + 1;
        }

        public int IndexOffEndOfLastStatement(TextualMedia tm)
        {
            using var context = _dbf.CreateDbContext();
            return IndexOffEndOfLastStatement(tm, context);
        }

        public void AddInitialStatements(List<Statement> statements)
        {
            TextualMedia parent = VerifyChainAndGetParent(statements);

            if (GetAllStatementsFor(parent).Count() > 0)
            {
                throw new Exception("Already have statements for this text!");
            }

            _databaseManager.AddStartOfChain(StatementDatabaseEntryFactory.FromStatements(statements, null, null));
        }

        public void AddStatements(List<Statement> statements)
        {
            TextualMedia parent = VerifyChainAndGetParent(statements);

            if (GetAllStatementsFor(parent).Count() == 0)
            {
                AddInitialStatements(statements);
                return;
            }

            int endOfChain = IndexOffEndOfLastStatement(parent);

            Statement previous = GetStatementsCoveringRange(
                parent, endOfChain - 1, endOfChain - 1).LastOrDefault() ?? throw new Exception();

            var context = _dbf.CreateDbContext();
            StatementDatabaseEntry previousEntry = context.Statements
                .Where(s => s.Parent == parent)
                .Where(s => s.LastCharIndex == endOfChain - 1)
                .FirstOrDefault() ?? throw new Exception();

            context.Dispose();

            _databaseManager.AddContinuationOfChain(
                StatementDatabaseEntryFactory.FromStatements(statements, previous, previousEntry));
        }

        public Statement? GetLastStatement(TextualMedia tm)
        {
            // don't use this in other methods here to avoid calling intensive methods again
            int lastIndex = IndexOffEndOfLastStatement(tm);

            if (lastIndex == -1) { return null; } // no statements

            return GetStatementsCoveringRange(tm, lastIndex - 1, lastIndex - 1).LastOrDefault() ?? null;
        }

        private static TextualMedia VerifyChainAndGetParent(List<Statement> statements)
        {
            for (int i = 0; i < statements.Count - 1; i++)
            {
                if (!(statements[i].FirstCharIndex <= statements[i].LastCharIndex &&
                      statements[i].LastCharIndex <= statements[i + 1].FirstCharIndex))
                {
                    throw new Exception("statements not ordered");
                }
            }
            if (!(statements[statements.Count - 1].FirstCharIndex <= statements[statements.Count - 1].LastCharIndex))
            {
                throw new Exception("statements not ordered");
            }

            if (statements.Select(s => s.Parent).Distinct().Count() > 1)
            {
                throw new Exception("statements for different parent texts!");
            }

            return statements.Select(s => s.Parent).Distinct().FirstOrDefault() ?? throw new Exception();
        }

        public static HashSet<DictionaryDefinition> GetAllUniqueDefinitions(List<Statement> statements)
        {
            HashSet<DictionaryDefinition> ret = new HashSet<DictionaryDefinition>();

            foreach (Statement statement in statements)
            {
                TextDecomposition flattenedRooted = statement.RootedDecomposition.Flattened();
                if (flattenedRooted.Decomposition is null) 
                {
                    // whole statement's decomposition was a leaf
                    if (statement.RootedDecomposition.Definition is not null)
                    {
                        ret.Add(statement.RootedDecomposition.Definition);
                    }
                    continue;
                }

                foreach (TextDecomposition leaf in flattenedRooted.Decomposition)
                {
                    if (leaf is null || leaf.Definition is null) { continue; }
                    ret.Add(leaf.Definition);
                }
            }

            return ret;
        }

    }
}
