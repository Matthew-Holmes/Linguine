using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class StatementDatabaseEntryManager : ManagerBase
    {
        // TODO  - we may want to be able to edit terms; or even merge or insert statements
        // add those methods as reqired

        public StatementDatabaseEntryManager(LinguineDataHandler db) : base(db)
        {
        }

        public List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> AttachDefinitions(List<StatementDatabaseEntry> statements)
        {
            List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> ret = new List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>>();

            foreach (StatementDatabaseEntry statement in statements) 
            {
                List<StatementDefinitionNode> defs = _db.StatementDefinitions
                    .Where(d => d.StatementDatabaseEntry == statement).ToList();
                ret.Add(Tuple.Create(statement, defs));
            }

            return ret;
        }

        public List<StatementDatabaseEntry> GetAllStatementsFor(TextualMedia tm)
        {
            return _db.Statements.Where(s => s.ParentKey == tm.DatabasePrimaryKey).ToList();
        }

        public List<StatementDatabaseEntry> FindChainFromContextCheckpoint(StatementDatabaseEntry lastInChain)
        {
            List<StatementDatabaseEntry> chain = new List<StatementDatabaseEntry> { lastInChain };

            while (chain.Last().ContextCheckpoint is not null)
            {
                chain.Add(lastInChain.Previous); // will throw if first statement doesn't have checkpoint
                // it should, so a throw is correct
            }

            chain.Reverse(); return chain;
        }

        public List<StatementDatabaseEntry> GetStatementsInsideRangeWithEndpoints(TextualMedia tm, int start, int stop)
        {
            return _db.Statements
                .Where(s => s.Parent == tm)
                .Where(s => s.FirstCharIndex >= start && s.LastCharIndex <= stop)
                .ToList();
        }

        public List<StatementDatabaseEntry> GetStatementsCoveringRangeWithEndpoints(TextualMedia tm, int start, int stop)
        {
            return _db.Statements
                .Where(s => s.Parent == tm)
                .Where(s => s.LastCharIndex >= start && s.FirstCharIndex <= stop)
                .ToList();
        }

        public List<StatementDatabaseEntry> PrependUpToContextCheckpoint(List<StatementDatabaseEntry> entries)
        {
            List<StatementDatabaseEntry> toPrepend = FindChainFromContextCheckpoint(entries.First());

            toPrepend.RemoveAt(toPrepend.Count - 1); // remove the overlap

            toPrepend.AddRange(entries);

            return toPrepend;
        }

        public int IndexOffEndOfLastStatement(TextualMedia tm)
        {
            return _db.Statements.Where(s => s.Parent == tm).Max(s => s.LastCharIndex) + 1;
        }

        public void RemoveAllFrom(StatementDatabaseEntry statement, int maxCollateral = 100)
        {
            List<StatementDatabaseEntry> toRemove = _db.Statements
                .Where(s => s.Parent == statement.Parent && s.FirstCharIndex >= statement.FirstCharIndex)
                .ToList();

            if (toRemove.Count > maxCollateral)
            {
                throw new Exception($"attempting to remove more than the allowed {maxCollateral} statements");
            }

            _db.Statements.RemoveRange(toRemove);
            _db.StatementDefinitions.RemoveRange(
                _db.StatementDefinitions.Where(
                    d => toRemove.Contains(d.StatementDatabaseEntry)));
        }

        public void AddContinuationOfChain(
            List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> statementsChain)
        {
            if (statementsChain.First().Item1.Previous is null)
            {
                throw new ArgumentException("should be calling AddStartOfChain if first statement has no previous");
            }
            AddInternal(statementsChain);
        }

        public void AddStartOfChain(List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> statementsChain)
        {
            if (statementsChain[0].Item1.Previous is not null)
            {
                throw new ArgumentException("not a start of a chain, should be calling Add");
            }
            AddInternal(statementsChain);
        }

        private void AddInternal(List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> statementsChain)
        {
            // do rigorous checking so that we know the database maintains integrity

            for (int i = 1; i < statementsChain.Count; i++)
            {
                if (statementsChain[i].Item1.Previous != statementsChain[i - 1].Item1)
                {
                    throw new ArgumentException("Statements must chain");
                }

                foreach (StatementDefinitionNode node in statementsChain[i].Item2)
                {
                    if (node.StatementDatabaseEntry != statementsChain[i].Item1)
                    {
                        throw new ArgumentException("Definitions must be for respective Statements");
                    }
                }
            }

            _db.Statements.AddRange(statementsChain.Select(s => s.Item1));
            _db.StatementDefinitions.AddRange(statementsChain.SelectMany(s => s.Item2)); // flattens list
        }
    }
}
