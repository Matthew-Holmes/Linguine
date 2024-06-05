using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_LearningExtraction
{
    // since have tests for StatementDatabaseEntryFactory will reuse those
    // but check that this class can undo operations from before
    [TestClass]
    public class StatementFactoryTests
    {
        [TestMethod]
        public void FromDatabaseEntries_NoContext_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "no decomposition";
            //0123456789012345
            TextDecomposition injective = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted = new TextDecomposition("no decomposition", null);

            Statement statement = new Statement(parentText, 0, 15, "no decomposition", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);
        }

        

        [TestMethod]
        public void FromDatabaseEntries_NoContext_CopiesRequiredData()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "no decomposition";

            TextDecomposition injective = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted = new TextDecomposition("no decomposition", null);

            Statement statement = new Statement(parentText, 0, 15, "no decomposition", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);

            Assert.IsTrue(ret.Count == 1);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 1);
            Assert.AreEqual(reret[0].Parent, parentText);
            Assert.AreEqual(reret[0].FirstCharIndex, 0);
            Assert.AreEqual(reret[0].LastCharIndex, 15);

        }

        

        [TestMethod]
        public void FromDatabaseEntries_NoContext_EmptyPropertiesAreEmpty()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "no decomposition";

            TextDecomposition injective = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted = new TextDecomposition("no decomposition", null);

            Statement statement = new Statement(parentText, 0, 15, "no decomposition", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.IsNull(reret[0].RootedDecomposition.Definition);
            Assert.IsNull(reret[0].InjectiveDecomposition.Definition);
            Assert.AreEqual(reret[0].StatementContext.Count, 0);
        }

        

        public void FromDatabaseEntries_WithContext_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "no decomposition";

            TextDecomposition injective = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted = new TextDecomposition("no decomposition", null);

            Statement statement = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);
        }

        [TestMethod]
        public void FromDatabaseEntries_WithContextCheckpoint_GetsContext()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "no decomposition";

            TextDecomposition injective = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted = new TextDecomposition("no decomposition", null);

            Statement statement = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 1);
            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");
        }


        [TestMethod]
        public void FromDatabaseEntries_WithContext_EmptyPropertiesAreEmpty()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "no decomposition";

            TextDecomposition injective = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted = new TextDecomposition("no decomposition", null);

            Statement statement = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 1);
            Assert.IsNull(reret[0].RootedDecomposition.Definition);
            Assert.IsNull(reret[0].InjectiveDecomposition.Definition);
        }
        

        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionWithoutContextChanging_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------00000000001111111111222222222233333333334444444444
            //-----------------01234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are two statements";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are two statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are two statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are two statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);



            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement1, statement2 }, null);
            var reret = StatementFactory.FromDatabaseEntries(ret);
        }

        

        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionWithoutContextChanging_ContextsReturn()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------00000000001111111111222222222233333333334444444444
            //-----------------01234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are two statements";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are two statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are two statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are two statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement1, statement2 }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 2);

            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");
            Assert.AreEqual(reret[1].StatementContext.Count, 2);
            Assert.AreEqual(reret[1].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[1].StatementContext[1], "used to test a factory method involving text broken into units");
        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionWithoutContextChanging_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2, statement3, statement4, statement5 }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);
        }

        
        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionWithoutContextChanging_CheckpointsComeBack()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2, statement3, statement4, statement5 }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 5);
            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[1].StatementContext.Count, 2);
            Assert.AreEqual(reret[1].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[1].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[2].StatementContext.Count, 2);
            Assert.AreEqual(reret[2].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[2].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[3].StatementContext.Count, 2);
            Assert.AreEqual(reret[3].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[3].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[4].StatementContext.Count, 2);
            Assert.AreEqual(reret[4].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[4].StatementContext[1], "used to test a factory method involving text broken into units");
        }


        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextChangesAllGone_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, and now something unrelated";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("and now something unrelated", null);
            TextDecomposition rooted2 = new TextDecomposition("and now something unrelated", null);

            Statement statement2 = new Statement(
                parentText, 18, 44,
                "and now something unrelated",
                new List<string>
                {
                },
                injective2, rooted2);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2 }, null);
            var reret = StatementFactory.FromDatabaseEntries(ret);
        }


        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextChangesAllGone_CorrectContexts()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, and now something unrelated";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("and now something unrelated", null);
            TextDecomposition rooted2 = new TextDecomposition("and now something unrelated", null);

            Statement statement2 = new Statement(
                parentText, 18, 44,
                "and now something unrelated",
                new List<string>
                {
                },
                injective2, rooted2);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2 }, null);

            var entry1 = ret[0].Item1;

            Assert.IsNotNull(entry1.ContextCheckpoint);
            Assert.AreEqual(entry1.ContextCheckpoint.Count, 2);
            Assert.AreEqual(entry1.ContextCheckpoint[0], "in testing code");
            Assert.AreEqual(entry1.ContextCheckpoint[1], "used to test a factory method involving text broken into units");

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 2);

            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[1].StatementContext.Count, 0);
        }

   
        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextChangesLastOneGone_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, and now something unrelated";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("and now something unrelated", null);
            TextDecomposition rooted2 = new TextDecomposition("and now something unrelated", null);

            Statement statement2 = new Statement(
                parentText, 18, 44,
                "and now something unrelated",
                new List<string>
                {
                    "in testing code",
                },
                injective2, rooted2);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2 }, null);
            var reret = StatementFactory.FromDatabaseEntries(ret);
        }

        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextChangesLastOneGone_CorrectContexts()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, and now something unrelated";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("and now something unrelated", null);
            TextDecomposition rooted2 = new TextDecomposition("and now something unrelated", null);

            Statement statement2 = new Statement(
                parentText, 18, 44,
                "and now something unrelated",
                new List<string>
                {
                    "in testing code",
                },
                injective2, rooted2);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2 }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 2);

            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[1].StatementContext.Count, 1);
            Assert.AreEqual(reret[1].StatementContext[0], "in testing code");

        }

        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextChangesFirstOneGone_CorrectContexts()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, and now something unrelated";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("and now something unrelated", null);
            TextDecomposition rooted2 = new TextDecomposition("and now something unrelated", null);

            Statement statement2 = new Statement(
                parentText, 18, 44,
                "and now something unrelated",
                new List<string>
                {
                    "used to test a factory method involving text broken into units",
                },
                injective2, rooted2);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2 }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            var entry2 = ret[1].Item1;

            Assert.AreEqual(reret.Count, 2);

            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[1].StatementContext.Count, 1);
            Assert.AreEqual(reret[1].StatementContext[0], "used to test a factory method involving text broken into units");
        }


        /*
        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChangesDestructivelyA_CorrectContexts()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "in testing code",
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                {
                },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2, statement3, statement4, statement5 }, null);

            var entry1 = ret[0].Item1;
            var entry2 = ret[1].Item1;
            var entry3 = ret[2].Item1;
            var entry4 = ret[3].Item1;
            var entry5 = ret[4].Item1;

            Assert.IsNotNull(entry1.ContextCheckpoint);
            Assert.AreEqual(entry1.ContextCheckpoint.Count, 2);
            Assert.AreEqual(entry1.ContextCheckpoint[0], "in testing code");
            Assert.AreEqual(entry1.ContextCheckpoint[1], "used to test a factory method involving text broken into units");

            Assert.IsNull(entry2.ContextCheckpoint);
            Assert.IsNull(entry3.ContextCheckpoint);
            Assert.IsNull(entry4.ContextCheckpoint);
            Assert.IsNull(entry5.ContextCheckpoint);
        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChangesDestructivelyA_CorrectRemovals()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "in testing code",
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                {
                },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2, statement3, statement4, statement5 }, null);

            var entry1 = ret[0].Item1;
            var entry2 = ret[1].Item1;
            var entry3 = ret[2].Item1;
            var entry4 = ret[3].Item1;
            var entry5 = ret[4].Item1;

            var removals1 = entry1.ContextDeltaRemovalsDescendingIndex;
            var removals2 = entry2.ContextDeltaRemovalsDescendingIndex;
            var removals3 = entry3.ContextDeltaRemovalsDescendingIndex;
            var removals4 = entry4.ContextDeltaRemovalsDescendingIndex;
            var removals5 = entry5.ContextDeltaRemovalsDescendingIndex;

            Assert.AreEqual(removals1.Count, 0);
            Assert.AreEqual(removals2.Count, 0);

            Assert.AreEqual(removals3.Count, 1);
            Assert.AreEqual(removals3[0], 1);

            Assert.AreEqual(removals4.Count, 0);

            Assert.AreEqual(removals5.Count, 1);
            Assert.AreEqual(removals5[0], 0);
        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChangesDestructivelyB_CorrectRemovals()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "used to test a factory method involving text broken into units"
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                {
                },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2, statement3, statement4, statement5 }, null);

            var entry1 = ret[0].Item1;
            var entry2 = ret[1].Item1;
            var entry3 = ret[2].Item1;
            var entry4 = ret[3].Item1;
            var entry5 = ret[4].Item1;

            var removals1 = entry1.ContextDeltaRemovalsDescendingIndex;
            var removals2 = entry2.ContextDeltaRemovalsDescendingIndex;
            var removals3 = entry3.ContextDeltaRemovalsDescendingIndex;
            var removals4 = entry4.ContextDeltaRemovalsDescendingIndex;
            var removals5 = entry5.ContextDeltaRemovalsDescendingIndex;

            Assert.AreEqual(removals1.Count, 0);
            Assert.AreEqual(removals2.Count, 0);
            Assert.AreEqual(removals3.Count, 0);

            Assert.AreEqual(removals4.Count, 1);
            Assert.AreEqual(removals4[0], 0);

            Assert.AreEqual(removals5.Count, 1);
            Assert.AreEqual(removals5[0], 0);
        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextInsertions_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units",
                        "now trailing off"
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units",
                        "now trailing off"
                    },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2, statement3, statement4, statement5 }, null);

            var entry1 = ret[0].Item1;
            var entry2 = ret[1].Item1;
            var entry3 = ret[2].Item1;
            var entry4 = ret[3].Item1;
            var entry5 = ret[4].Item1;

            var removals1 = entry1.ContextDeltaInsertionsDescendingIndex;
            var removals2 = entry2.ContextDeltaInsertionsDescendingIndex;
            var removals3 = entry3.ContextDeltaInsertionsDescendingIndex;
            var removals4 = entry4.ContextDeltaInsertionsDescendingIndex;
            var removals5 = entry5.ContextDeltaInsertionsDescendingIndex;
        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextInsertions_CorrectCheckpoints()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units",
                        "now trailing off"
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units",
                        "now trailing off"
                    },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2, statement3, statement4, statement5 }, null);

            var entry1 = ret[0].Item1;
            var entry2 = ret[1].Item1;
            var entry3 = ret[2].Item1;
            var entry4 = ret[3].Item1;
            var entry5 = ret[4].Item1;

            var insertions1 = entry1.ContextDeltaInsertionsDescendingIndex;
            var insertions2 = entry2.ContextDeltaInsertionsDescendingIndex;
            var insertions3 = entry3.ContextDeltaInsertionsDescendingIndex;
            var insertions4 = entry4.ContextDeltaInsertionsDescendingIndex;
            var insertions5 = entry5.ContextDeltaInsertionsDescendingIndex;

            Assert.IsNotNull(entry1.ContextCheckpoint);
            Assert.AreEqual(entry1.ContextCheckpoint.Count, 1);
            Assert.AreEqual(entry1.ContextCheckpoint[0], "in testing code");

            Assert.IsNull(entry2.ContextCheckpoint);
            Assert.IsNull(entry3.ContextCheckpoint);
            Assert.IsNull(entry4.ContextCheckpoint);
            Assert.IsNull(entry5.ContextCheckpoint);
        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextInsertions_CorrectInsertions()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units",
                        "now trailing off"
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units",
                        "now trailing off"
                    },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2, statement3, statement4, statement5 }, null);

            var entry1 = ret[0].Item1;
            var entry2 = ret[1].Item1;
            var entry3 = ret[2].Item1;
            var entry4 = ret[3].Item1;
            var entry5 = ret[4].Item1;

            var insertions1 = entry1.ContextDeltaInsertionsDescendingIndex;
            var insertions2 = entry2.ContextDeltaInsertionsDescendingIndex;
            var insertions3 = entry3.ContextDeltaInsertionsDescendingIndex;
            var insertions4 = entry4.ContextDeltaInsertionsDescendingIndex;
            var insertions5 = entry5.ContextDeltaInsertionsDescendingIndex;

            Assert.AreEqual(insertions1.Count, 0);

            Assert.AreEqual(insertions2.Count, 1);
            Assert.AreEqual(insertions2[0].Item1, 1);
            Assert.AreEqual(insertions2[0].Item2, "used to test a factory method involving text broken into units");

            Assert.AreEqual(insertions3.Count, 0);

            Assert.AreEqual(insertions4.Count, 1);
            Assert.AreEqual(insertions4[0].Item1, 2);
            Assert.AreEqual(insertions4[0].Item2, "now trailing off");

            Assert.AreEqual(insertions5.Count, 0);

        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextIntraInsertions_CorrectInsertions()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                        "intra inserted",
                        "used to test a factory method involving text broken into units"
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "in testing code",
                        "intra inserted",
                        "another intra inserted",
                        "used to test a factory method involving text broken into units",
                        "now trailing off"
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                    {
                        "in testing code",
                        "intra inserted",
                        "another intra inserted",
                        "used to test a factory method involving text broken into units",
                        "now trailing off"
                    },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2, statement3, statement4, statement5 }, null);

            var entry1 = ret[0].Item1;
            var entry2 = ret[1].Item1;
            var entry3 = ret[2].Item1;
            var entry4 = ret[3].Item1;
            var entry5 = ret[4].Item1;

            var insertions1 = entry1.ContextDeltaInsertionsDescendingIndex;
            var insertions2 = entry2.ContextDeltaInsertionsDescendingIndex;
            var insertions3 = entry3.ContextDeltaInsertionsDescendingIndex;
            var insertions4 = entry4.ContextDeltaInsertionsDescendingIndex;
            var insertions5 = entry5.ContextDeltaInsertionsDescendingIndex;

            Assert.AreEqual(insertions1.Count, 0);

            Assert.AreEqual(insertions2.Count, 1);
            Assert.AreEqual(insertions2[0].Item1, 1);
            Assert.AreEqual(insertions2[0].Item2, "used to test a factory method involving text broken into units");

            Assert.AreEqual(insertions3.Count, 1);
            Assert.AreEqual(insertions3[0].Item1, 1);
            Assert.AreEqual(insertions3[0].Item2, "intra inserted");


            Assert.AreEqual(insertions4.Count, 2);
            Assert.AreEqual(insertions4[0].Item1, 3);
            Assert.AreEqual(insertions4[0].Item2, "now trailing off");
            Assert.AreEqual(insertions4[1].Item1, 2);
            Assert.AreEqual(insertions4[1].Item2, "another intra inserted");

            Assert.AreEqual(insertions5.Count, 0);

        }

        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextSwapsLastOne_CorrectDeltas()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, and now something unrelated";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("and now something unrelated", null);
            TextDecomposition rooted2 = new TextDecomposition("and now something unrelated", null);

            Statement statement2 = new Statement(
                parentText, 18, 44,
                "and now something unrelated",
                new List<string>
                {
                    "in testing code",
                    "testing context changing"
                },
                injective2, rooted2);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2 }, null);

            var entry2 = ret[1].Item1;

            Assert.AreEqual(entry2.ContextDeltaRemovalsDescendingIndex.Count, 1);
            Assert.AreEqual(entry2.ContextDeltaRemovalsDescendingIndex[0], 1);

            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex.Count, 1);
            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[0].Item1, 1);
            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[0].Item2, "testing context changing");

        }

        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextSwapsFirstOne_CorrectDeltas()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, and now something unrelated";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("and now something unrelated", null);
            TextDecomposition rooted2 = new TextDecomposition("and now something unrelated", null);

            Statement statement2 = new Statement(
                parentText, 18, 44,
                "and now something unrelated",
                new List<string>
                {
                    "outer changed",
                    "used to test a factory method involving text broken into units"
                },
                injective2, rooted2);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2 }, null);

            var entry2 = ret[1].Item1;

            Assert.AreEqual(entry2.ContextDeltaRemovalsDescendingIndex.Count, 1);
            Assert.AreEqual(entry2.ContextDeltaRemovalsDescendingIndex[0], 0);

            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex.Count, 1);
            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[0].Item1, 0);
            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[0].Item2, "outer changed");

        }

        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextSwapsMiddleOne_CorrectDeltas()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, and now something unrelated";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units",
                        "more context"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("and now something unrelated", null);
            TextDecomposition rooted2 = new TextDecomposition("and now something unrelated", null);

            Statement statement2 = new Statement(
                parentText, 18, 44,
                "and now something unrelated",
                new List<string>
                {
                    "in testing code",
                    "specifically the context updating",
                    "more context"

                },
                injective2, rooted2);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2 }, null);

            var entry2 = ret[1].Item1;

            Assert.AreEqual(entry2.ContextDeltaRemovalsDescendingIndex.Count, 1);
            Assert.AreEqual(entry2.ContextDeltaRemovalsDescendingIndex[0], 1);

            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex.Count, 1);
            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[0].Item1, 1);
            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[0].Item2, "specifically the context updating");

        }

        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextSwapsAll_CorrectDeltas()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, and now something unrelated";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units",
                        "more context"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("and now something unrelated", null);
            TextDecomposition rooted2 = new TextDecomposition("and now something unrelated", null);

            Statement statement2 = new Statement(
                parentText, 18, 44,
                "and now something unrelated",
                new List<string>
                {
                    "in more testing code",
                    "specifically the context updating",
                    "even more context"

                },
                injective2, rooted2);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2 }, null);

            var entry2 = ret[1].Item1;

            Assert.AreEqual(entry2.ContextDeltaRemovalsDescendingIndex.Count, 3);
            Assert.AreEqual(entry2.ContextDeltaRemovalsDescendingIndex[0], 2);
            Assert.AreEqual(entry2.ContextDeltaRemovalsDescendingIndex[1], 1);
            Assert.AreEqual(entry2.ContextDeltaRemovalsDescendingIndex[2], 0);

            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex.Count, 3);
            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[0].Item1, 0);
            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[0].Item2, "even more context");

            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[1].Item1, 0);
            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[1].Item2, "specifically the context updating");

            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[2].Item1, 0);
            Assert.AreEqual(entry2.ContextDeltaInsertionsDescendingIndex[2].Item2, "in more testing code");



        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChanges_CorrectDeltas()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                        "now testing changing context",
                        "trailing off",
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "inside a test statement",
                        "testing changing context",
                        "trailing off",
                        "near the end",
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                {
                },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1, statement2, statement3, statement4, statement5 }, null);

            var entry1 = ret[0].Item1;
            var entry2 = ret[1].Item1;
            var entry3 = ret[2].Item1;
            var entry4 = ret[3].Item1;
            var entry5 = ret[4].Item1;

            var insertions1 = entry1.ContextDeltaInsertionsDescendingIndex;
            var insertions2 = entry2.ContextDeltaInsertionsDescendingIndex;
            var insertions3 = entry3.ContextDeltaInsertionsDescendingIndex;
            var insertions4 = entry4.ContextDeltaInsertionsDescendingIndex;
            var insertions5 = entry5.ContextDeltaInsertionsDescendingIndex;

            var removals1 = entry1.ContextDeltaRemovalsDescendingIndex;
            var removals2 = entry2.ContextDeltaRemovalsDescendingIndex;
            var removals3 = entry3.ContextDeltaRemovalsDescendingIndex;
            var removals4 = entry4.ContextDeltaRemovalsDescendingIndex;
            var removals5 = entry5.ContextDeltaRemovalsDescendingIndex;

            Assert.AreEqual(insertions1.Count, 0);
            Assert.AreEqual(removals1.Count, 0);

            Assert.AreEqual(insertions2.Count, 0);
            Assert.AreEqual(removals2.Count, 0);

            Assert.AreEqual(removals3.Count, 1);
            Assert.AreEqual(removals3[0], 1);
            Assert.AreEqual(insertions3.Count, 2);
            Assert.AreEqual(insertions3[0].Item1, 1);
            Assert.AreEqual(insertions3[0].Item2, "trailing off");
            Assert.AreEqual(insertions3[1].Item1, 1);
            Assert.AreEqual(insertions3[1].Item2, "now testing changing context");


            Assert.AreEqual(removals4.Count, 2);
            Assert.AreEqual(removals4[0], 1);
            Assert.AreEqual(removals4[1], 0);
            Assert.AreEqual(insertions4.Count, 3);
            Assert.AreEqual(insertions4[0].Item1, 1);
            Assert.AreEqual(insertions4[0].Item2, "near the end");
            Assert.AreEqual(insertions4[1].Item1, 0);
            Assert.AreEqual(insertions4[1].Item2, "testing changing context");
            Assert.AreEqual(insertions4[2].Item1, 0);
            Assert.AreEqual(insertions4[2].Item2, "inside a test statement");

            Assert.AreEqual(insertions5.Count, 0);
            Assert.AreEqual(removals5.Count, 4);
            Assert.AreEqual(removals5[0], 3);
            Assert.AreEqual(removals5[1], 2);
            Assert.AreEqual(removals5[2], 1);
            Assert.AreEqual(removals5[3], 0);
        }


        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChanges_WithPrevious_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                        "now testing changing context",
                        "trailing off",
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "inside a test statement",
                        "testing changing context",
                        "trailing off",
                        "near the end",
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                {
                },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement2, statement3, statement4, statement5 }, statement1);
        }


        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChanges_WithPrevious_NoCheckpoints()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                        "now testing changing context",
                        "trailing off",
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "inside a test statement",
                        "testing changing context",
                        "trailing off",
                        "near the end",
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                {
                },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement2, statement3, statement4, statement5 }, statement1);

            var entry1 = ret[0].Item1;
            var entry2 = ret[1].Item1;
            var entry3 = ret[2].Item1;
            var entry4 = ret[3].Item1;

            Assert.IsNull(entry1.ContextCheckpoint);
            Assert.IsNull(entry2.ContextCheckpoint);
            Assert.IsNull(entry3.ContextCheckpoint);
            Assert.IsNull(entry4.ContextCheckpoint);
        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChanges_WithPrevious_CorrectDeltas()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------000000000011111111112222222222333333333344444444445555555555666666666677777777778888888888
            //-----------------012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            parentText.Text = "no decomposition, but now there are multiple statements, such as this, this one, and this";

            TextDecomposition injective1 = new TextDecomposition("no decomposition", null);
            TextDecomposition rooted1 = new TextDecomposition("no decomposition", null);

            Statement statement1 = new Statement(
                parentText, 0, 15,
                "no decomposition",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective1, rooted1);

            TextDecomposition injective2 = new TextDecomposition("but now there are multiple statements", null);
            TextDecomposition rooted2 = new TextDecomposition("but now there are multiple statements", null);

            Statement statement2 = new Statement(
                parentText, 18, 49,
                "but now there are multiple statements",
                new List<string>
                    {
                        "in testing code",
                        "used to test a factory method involving text broken into units"
                    },
                injective2, rooted2);

            TextDecomposition injective3 = new TextDecomposition("such as this", null);
            TextDecomposition rooted3 = new TextDecomposition("such as this", null);

            Statement statement3 = new Statement(
                parentText, 57, 68,
                "such as this",
                new List<string>
                    {
                        "in testing code",
                        "now testing changing context",
                        "trailing off",
                    },
                injective3, rooted3);

            TextDecomposition injective4 = new TextDecomposition("this one", null);
            TextDecomposition rooted4 = new TextDecomposition("this one", null);

            Statement statement4 = new Statement(
                parentText, 71, 78,
                "this one",
                new List<string>
                    {
                        "inside a test statement",
                        "testing changing context",
                        "trailing off",
                        "near the end",
                    },
                injective4, rooted4);

            TextDecomposition injective5 = new TextDecomposition("and this", null);
            TextDecomposition rooted5 = new TextDecomposition("and this", null);

            Statement statement5 = new Statement(
                parentText, 81, 88,
                "and this",
                new List<string>
                {
                },
                injective5, rooted5);

            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement2, statement3, statement4, statement5 }, statement1);

            var entry1 = ret[0].Item1;
            var entry2 = ret[1].Item1;
            var entry3 = ret[2].Item1;
            var entry4 = ret[3].Item1;

            var insertions1 = entry1.ContextDeltaInsertionsDescendingIndex;
            var insertions2 = entry2.ContextDeltaInsertionsDescendingIndex;
            var insertions3 = entry3.ContextDeltaInsertionsDescendingIndex;
            var insertions4 = entry4.ContextDeltaInsertionsDescendingIndex;

            var removals1 = entry1.ContextDeltaRemovalsDescendingIndex;
            var removals2 = entry2.ContextDeltaRemovalsDescendingIndex;
            var removals3 = entry3.ContextDeltaRemovalsDescendingIndex;
            var removals4 = entry4.ContextDeltaRemovalsDescendingIndex;

            Assert.AreEqual(insertions1.Count, 0);
            Assert.AreEqual(removals1.Count, 0);

            Assert.AreEqual(removals2.Count, 1);
            Assert.AreEqual(removals2[0], 1);
            Assert.AreEqual(insertions2.Count, 2);
            Assert.AreEqual(insertions2[0].Item1, 1);
            Assert.AreEqual(insertions2[0].Item2, "trailing off");
            Assert.AreEqual(insertions2[1].Item1, 1);
            Assert.AreEqual(insertions2[1].Item2, "now testing changing context");


            Assert.AreEqual(removals3.Count, 2);
            Assert.AreEqual(removals3[0], 1);
            Assert.AreEqual(removals3[1], 0);
            Assert.AreEqual(insertions3.Count, 3);
            Assert.AreEqual(insertions3[0].Item1, 1);
            Assert.AreEqual(insertions3[0].Item2, "near the end");
            Assert.AreEqual(insertions3[1].Item1, 0);
            Assert.AreEqual(insertions3[1].Item2, "testing changing context");
            Assert.AreEqual(insertions3[2].Item1, 0);
            Assert.AreEqual(insertions3[2].Item2, "inside a test statement");

            Assert.AreEqual(insertions4.Count, 0);
            Assert.AreEqual(removals4.Count, 4);
            Assert.AreEqual(removals4[0], 3);
            Assert.AreEqual(removals4[1], 2);
            Assert.AreEqual(removals4[2], 1);
            Assert.AreEqual(removals4[3], 0);
        }

        */
    }
}
