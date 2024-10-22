using Microsoft.EntityFrameworkCore;
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
    internal class StatementDatabaseEntryManager : ManagerBase
    {
        // TODO  - we may want to be able to edit terms; or even merge or insert statements
        // add those methods as reqired

        internal StatementDatabaseEntryManager(String conn) : base(conn)
        {
        }

        internal List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> AttachDefinitions(List<StatementDatabaseEntry> statements, LinguineContext lg)
        {
            List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> ret = new List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>>();

            foreach (StatementDatabaseEntry statement in statements) 
            {
                List<StatementDefinitionNode> defs = lg.StatementDefinitions
                    .Where(d => d.StatementDatabaseEntry == statement)
                    .Include(n => n.DictionaryDefinition).ToList();
                ret.Add(Tuple.Create(statement, defs));
            }

            return ret;
        }

        internal List<StatementDatabaseEntry> GetAllStatementsEntriesFor(TextualMedia tm, LinguineContext lg)
        {
            return lg.Statements.Where(s => s.ParentKey == tm.DatabasePrimaryKey).Include(s => s.Previous).ToList();
        }

        internal List<StatementDatabaseEntry> FindChainFromContextCheckpoint(StatementDatabaseEntry lastInChain, LinguineContext lg)
        {
            List<StatementDatabaseEntry> chain = new List<StatementDatabaseEntry> { lastInChain };

            StatementDatabaseEntry current = chain.Last();

            while (current.ContextCheckpoint is null)
            {
                lg.Entry(current).Reference(e => e.Previous).Load();

                if (current.Previous == null)
                {
                    throw new InvalidOperationException("First statement does not have a checkpoint.");
                }

                current = current.Previous;
                chain.Add(current);
            }

            chain.Reverse(); return chain;
        }

        internal List<StatementDatabaseEntry> GetStatementsInsideRangeWithEndpoints(TextualMedia tm, int start, int stop)
        {
            using LinguineContext lg = Linguine();
            return lg.Statements
                .Where(s => s.Parent == tm)
                .Where(s => s.FirstCharIndex >= start && s.LastCharIndex <= stop)
                .Include(s => s.Previous)
                .ToList();
        }

        internal List<StatementDatabaseEntry> GetStatementsCoveringRangeWithEndpoints(
            TextualMedia tm, int start, int stop, LinguineContext lg)
        {
            return lg.Statements
                .Where(s => s.Parent == tm)
                .Where(s => s.LastCharIndex >= start && s.FirstCharIndex <= stop)
                .Include(s => s.Previous)
                .ToList();
        }

        internal List<StatementDatabaseEntry> PrependUpToContextCheckpoint(List<StatementDatabaseEntry> entries, LinguineContext lg)
        {
            List<StatementDatabaseEntry> toPrepend = FindChainFromContextCheckpoint(entries.First(), lg);

            toPrepend.RemoveAt(toPrepend.Count - 1); // remove the overlap

            toPrepend.AddRange(entries);

            return toPrepend;
        }

        internal void RemoveAllFrom(StatementDatabaseEntry statement, int maxCollateral = 100)
        {
            using LinguineContext lg = Linguine();
            List<StatementDatabaseEntry> toRemove = lg.Statements
                .Where(s => s.Parent == statement.Parent && s.FirstCharIndex >= statement.FirstCharIndex)
                .ToList();

            if (toRemove.Count > maxCollateral)
            {
                throw new Exception($"attempting to remove more than the allowed {maxCollateral} statements");
            }

            lg.Statements.RemoveRange(toRemove);
            lg.StatementDefinitions.RemoveRange(
                lg.StatementDefinitions.Where(
                    d => toRemove.Contains(d.StatementDatabaseEntry)));

            lg.SaveChanges();
        }

        internal void AddContinuationOfChain(
            List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> statementsChain)
        {
            if (statementsChain.First().Item1.Previous is null)
            {
                throw new ArgumentException("should be calling AddStartOfChain if first statement has no previous");
            }
            AddInternal(statementsChain);
        }

        internal void AddStartOfChain(List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> statementsChain)
        {
            if (statementsChain[0].Item1.Previous is not null)
            {
                throw new ArgumentException("not a start of a chain, should be calling Add");
            }
            AddInternal(statementsChain);
        }

        internal void AddInternal(List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> statementsChain)
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
            using LinguineContext lg = Linguine();
            lg.Statements.AddRange(statementsChain.Select(s => s.Item1));
            lg.StatementDefinitions.AddRange(statementsChain.SelectMany(s => s.Item2)); // flattens list
            lg.SaveChanges();
        }
    }
}
