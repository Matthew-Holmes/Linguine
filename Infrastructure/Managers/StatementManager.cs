﻿using Linguine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class StatementManager : ManagerBase
    {
        private StatementDatabaseEntryManager _databaseManager;

        public StatementManager(LinguineDataHandler db) : base(db)
        {
            _databaseManager = new StatementDatabaseEntryManager(db);
        }

        public List<Statement> GetAllStatementsFor(TextualMedia tm)
        {
            List<StatementDatabaseEntry> entries = _databaseManager.GetAllStatementsEntriesFor(tm);

            return StatementFactory.FromDatabaseEntries(_databaseManager.AttachDefinitions(entries));
        }

        public List<Statement> GetStatementsCoveringRange(TextualMedia tm, int start, int stop)
        {
            List<StatementDatabaseEntry> found = _databaseManager.GetStatementsCoveringRangeWithEndpoints(tm, start, stop);

            int oldCount = found.Count;
            found = _databaseManager.PrependUpToContextCheckpoint(found);
            int bookMark = found.Count - oldCount;

            var raw = _databaseManager.AttachDefinitions(found);

            return StatementFactory.FromDatabaseEntries(raw).Skip(bookMark).ToList();
        }

        public int IndexOffEndOfLastStatement(TextualMedia tm)
        {
            var statements = _db.Statements.Where(s => s.Parent == tm);
            if (!statements.Any())
            {
                return -1;
            }
            return statements.Max(s => s.LastCharIndex) + 1;
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

        public void AddMoreStatements(List<Statement> statements)
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

            StatementDatabaseEntry previousEntry = _db.Statements.Where(
                s => s.LastCharIndex == endOfChain - 1).FirstOrDefault() ?? throw new Exception();

            _databaseManager.AddContinuationOfChain(
                StatementDatabaseEntryFactory.FromStatements(statements, previous, previousEntry));
        }

        public Statement? GetLastStatement(TextualMedia tm)
        {
            // don't use this in other methods here to avoid calling intensive methods again
            int lastIndex = IndexOffEndOfLastStatement(tm);

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

    }
}