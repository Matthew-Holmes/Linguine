using Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ObjectiveC;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    internal class StatementDatabaseEntryManager : ManagerBase
    {
        private static readonly Random rng = new Random();

        // TODO  - we may want to be able to edit terms; or even merge or insert statements
        // add those methods as reqired
        private object _lock = new();
        internal StatementDatabaseEntryManager(LinguineDbContextFactory dbf) : base(dbf)
        {
        }

        internal List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> AttachDefinitions(List<StatementDatabaseEntry> statements)
        {
            using var context = _dbf.CreateDbContext();
            List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> ret = new List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>>();

            foreach (StatementDatabaseEntry statement in statements) 
            {
                List<StatementDefinitionNode> defs = context.StatementDefinitions
                    .Where(d => d.StatementDatabaseEntry == statement)
                    .Include(n => n.DictionaryDefinition).ToList();
                ret.Add(Tuple.Create(statement, defs));
            }

            return ret;
        }

        internal List<StatementDatabaseEntry> GetAllStatementsEntriesFor(TextualMedia tm)
        {
            using var context = _dbf.CreateDbContext();
            context.Attach(tm);
            return context.Statements.Where(s => s.ParentKey == tm.DatabasePrimaryKey).Include(s => s.Previous).ToList();
        }

        internal List<StatementDatabaseEntry> GetNStatementsFor(DictionaryDefinition def, int n)
        {
            using var context = _dbf.CreateDbContext();
            context.Attach(def);

            List<StatementDefinitionNode> nodes = context.StatementDefinitions
                .Where(n => n.DictionaryDefinition == def)
                .Include(n => n.StatementDatabaseEntry)
                .Include(n => n.StatementDatabaseEntry.Parent)
                .ToList();

            if (nodes.Count == 0)
            {
                return new List<StatementDatabaseEntry>();
            }

            List<StatementDefinitionNode> sample = SelectionSampler.Sample(nodes, n, rng);

            return sample.Select(n => n.StatementDatabaseEntry).ToList();

        }

        internal List<StatementDatabaseEntry> FindChainFromContextCheckpoint(StatementDatabaseEntry lastInChain)
        {
            using var context = _dbf.CreateDbContext();

            context.Attach(lastInChain);

            List<StatementDatabaseEntry> chain = new List<StatementDatabaseEntry> { lastInChain };

            while (chain.Last().ContextCheckpoint is null)
            {
                context.Entry(chain.Last()).Reference(e => e.Previous).Load();
                StatementDatabaseEntry previous = chain.Last().Previous;
                chain.Add(chain.Last().Previous); // will throw if first statement doesn't have checkpoint
                // it should, so a throw is correct
            }

            chain.Reverse(); return chain;
        }

        internal List<StatementDatabaseEntry> GetStatementsInsideRangeWithEndpoints(TextualMedia tm, int start, int stop)
        {
            using var context = _dbf.CreateDbContext();
            context.Attach(tm);
            return context.Statements
                .Where(s => s.Parent == tm)
                .Where(s => s.FirstCharIndex >= start && s.LastCharIndex <= stop)
                .Include(s => s.Previous)
                .ToList();
        }

        internal List<StatementDatabaseEntry> GetStatementsCoveringRangeWithEndpoints(TextualMedia tm, int start, int stop)
        {
            using var context = _dbf.CreateDbContext();
            context.Attach(tm);
            return context.Statements
                .Where(s => s.Parent == tm)
                .Where(s => s.LastCharIndex >= start && s.FirstCharIndex <= stop)
                .Include(s => s.Previous)
                .ToList();
        }

        internal List<StatementDatabaseEntry> PrependUpToContextCheckpoint(List<StatementDatabaseEntry> entries)
        {
            List<StatementDatabaseEntry> toPrepend = FindChainFromContextCheckpoint(entries.First());

            toPrepend.RemoveAt(toPrepend.Count - 1); // remove the overlap

            toPrepend.AddRange(entries);

            return toPrepend;
        }

        //internal void RemoveAllFrom(StatementDatabaseEntry statement, int maxCollateral = 100)
        //{
        //    List<StatementDatabaseEntry> toRemove = _db.Statements
        //        .Where(s => s.Parent == statement.Parent && s.FirstCharIndex >= statement.FirstCharIndex)
        //        .ToList();

        //    if (toRemove.Count > maxCollateral)
        //    {
        //        throw new Exception($"attempting to remove more than the allowed {maxCollateral} statements");
        //    }

        //    _db.Statements.RemoveRange(toRemove);
        //    _db.StatementDefinitions.RemoveRange(
        //        _db.StatementDefinitions.Where(
        //            d => toRemove.Contains(d.StatementDatabaseEntry)));
        //}

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
            lock (_lock)
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
                // now update the database

                using var context = _dbf.CreateDbContext();

                // Sharp edge - have to attach some properties back to the tracking graph
                // otherwise this will throw System.InvalidOperationException
                StatementDatabaseEntry first = statementsChain[0].Item1;
                context.Attach(first.Parent);
                if (first.Previous is not null)
                {
                    context.Attach(first.Previous);
                }


                context.Statements.AddRange(statementsChain.Select(s => s.Item1));
                context.SaveChanges();
                context.ChangeTracker.Clear();

                // sharper edge - since the nodes can reference the same definitions
                // EF will try to load that twice sometimes and throw an Exception
                // so we go the careful (but slow!) route
                foreach (Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>> pair in statementsChain)
                {
                    foreach (StatementDefinitionNode node in pair.Item2)
                    {
                        if (node.DictionaryDefinition is not null)
                        {
                            context.Attach(node.DictionaryDefinition);
                        }
                        context.Attach(node.StatementDatabaseEntry);

                        context.StatementDefinitions.Add(node);
                        context.SaveChanges();
                        context.ChangeTracker.Clear();
                    }
                }
            }
        }
    }
}
