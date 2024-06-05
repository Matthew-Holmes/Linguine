using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public void FromDatabaseEntries_TwoStatementsNoDecompositionWithoutContextChanging_CorrectContexts()
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
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionWithoutContextChanging_CorrectContexts()
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

            Assert.AreEqual(reret.Count, 2);

            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[1].StatementContext.Count, 1);
            Assert.AreEqual(reret[1].StatementContext[0], "used to test a factory method involving text broken into units");
        }

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

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 5);

            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[1].StatementContext.Count, 2);
            Assert.AreEqual(reret[1].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[1].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[2].StatementContext.Count, 1);
            Assert.AreEqual(reret[2].StatementContext[0], "in testing code");

            Assert.AreEqual(reret[3].StatementContext.Count, 1);
            Assert.AreEqual(reret[3].StatementContext[0], "in testing code");

            Assert.AreEqual(reret[4].StatementContext.Count, 0);
        }


        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChangesDestructivelyB_CorrectContexts()
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

            Assert.AreEqual(reret[3].StatementContext.Count, 1);
            Assert.AreEqual(reret[3].StatementContext[0], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[4].StatementContext.Count, 0);
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

            var reret = StatementFactory.FromDatabaseEntries(ret);
        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextInsertions_CorrectContexts()
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

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 5);

            Assert.AreEqual(reret[0].StatementContext.Count, 1);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");

            Assert.AreEqual(reret[1].StatementContext.Count, 2);
            Assert.AreEqual(reret[1].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[1].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[2].StatementContext.Count, 2);
            Assert.AreEqual(reret[2].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[2].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[3].StatementContext.Count, 3);
            Assert.AreEqual(reret[3].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[3].StatementContext[1], "used to test a factory method involving text broken into units");
            Assert.AreEqual(reret[3].StatementContext[2], "now trailing off");


            Assert.AreEqual(reret[4].StatementContext.Count, 3);
            Assert.AreEqual(reret[4].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[4].StatementContext[1], "used to test a factory method involving text broken into units");
            Assert.AreEqual(reret[4].StatementContext[2], "now trailing off");
        }


        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextIntraInsertions_CorrectContexts()
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

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 5);

            Assert.AreEqual(reret[0].StatementContext.Count, 1);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");

            Assert.AreEqual(reret[1].StatementContext.Count, 2);
            Assert.AreEqual(reret[1].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[1].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[2].StatementContext.Count, 3);
            Assert.AreEqual(reret[2].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[2].StatementContext[1], "intra inserted");
            Assert.AreEqual(reret[2].StatementContext[2], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[3].StatementContext.Count, 5);
            Assert.AreEqual(reret[3].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[3].StatementContext[1], "intra inserted");
            Assert.AreEqual(reret[3].StatementContext[2], "another intra inserted");
            Assert.AreEqual(reret[3].StatementContext[3], "used to test a factory method involving text broken into units");
            Assert.AreEqual(reret[3].StatementContext[4], "now trailing off");

            Assert.AreEqual(reret[4].StatementContext.Count, 5);
            Assert.AreEqual(reret[4].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[4].StatementContext[1], "intra inserted");
            Assert.AreEqual(reret[4].StatementContext[2], "another intra inserted");
            Assert.AreEqual(reret[4].StatementContext[3], "used to test a factory method involving text broken into units");
            Assert.AreEqual(reret[4].StatementContext[4], "now trailing off");
        }



        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextSwapsLastOne_CorrectContexts()
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

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 2);

            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[1].StatementContext.Count, 2);
            Assert.AreEqual(reret[1].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[1].StatementContext[1], "testing context changing");


        }

        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextSwapsFirstOne_CorrectContexts()
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
            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 2);

            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[1].StatementContext.Count, 2);
            Assert.AreEqual(reret[1].StatementContext[0], "outer changed");
            Assert.AreEqual(reret[1].StatementContext[1], "used to test a factory method involving text broken into units");

        }

        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextSwapsMiddleOne_CorrectContexts()
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

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 2);

            Assert.AreEqual(reret[0].StatementContext.Count, 3);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");
            Assert.AreEqual(reret[0].StatementContext[2], "more context");

            Assert.AreEqual(reret[1].StatementContext.Count, 3);
            Assert.AreEqual(reret[1].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[1].StatementContext[1], "specifically the context updating");
            Assert.AreEqual(reret[1].StatementContext[2], "more context");

        }

        [TestMethod]
        public void FromDatabaseEntries_TwoStatementsNoDecompositionContextSwapsAll_CorrectContexts()
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

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 2);

            Assert.AreEqual(reret[0].StatementContext.Count, 3);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");
            Assert.AreEqual(reret[0].StatementContext[2], "more context");

            Assert.AreEqual(reret[1].StatementContext.Count, 3);
            Assert.AreEqual(reret[1].StatementContext[0], "in more testing code");
            Assert.AreEqual(reret[1].StatementContext[1], "specifically the context updating");
            Assert.AreEqual(reret[1].StatementContext[2], "even more context");
        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChanges_CorrectContexts()
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

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 5);

            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[1].StatementContext.Count, 2);
            Assert.AreEqual(reret[1].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[1].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[2].StatementContext.Count, 3);
            Assert.AreEqual(reret[2].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[2].StatementContext[1], "now testing changing context");
            Assert.AreEqual(reret[2].StatementContext[2], "trailing off");

            Assert.AreEqual(reret[3].StatementContext.Count, 4);
            Assert.AreEqual(reret[3].StatementContext[0], "inside a test statement");
            Assert.AreEqual(reret[3].StatementContext[1], "testing changing context");
            Assert.AreEqual(reret[3].StatementContext[2], "trailing off");
            Assert.AreEqual(reret[3].StatementContext[3], "near the end");

            Assert.AreEqual(reret[4].StatementContext.Count, 0);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChanges_WithNoCheckpoint_Throws()
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

            StatementDatabaseEntry previousEntry = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1 }, null)[0].Item1; // begin the chain
            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement2, statement3, statement4, statement5 }, statement1, previousEntry);

            var reret = StatementFactory.FromDatabaseEntries(ret);

        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChanges_MergedEntriesForCheckpoint_Runs()
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

            var previousEntry = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1 }, null); // begin the chain
            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement2, statement3, statement4, statement5 }, statement1, previousEntry[0].Item1);

            ret.Insert(0, previousEntry[0]);

            var reret = StatementFactory.FromDatabaseEntries(ret);

        }

        [TestMethod]
        public void FromDatabaseEntries_MultipleStatementsNoDecompositionContextChanges_MergedEntriesForCheckpoint_CorrectContexts()
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

            var previousEntry = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement1 }, null); // begin the chain
            var ret = StatementDatabaseEntryFactory.FromStatements(
                new List<Statement> { statement2, statement3, statement4, statement5 }, statement1, previousEntry[0].Item1);

            ret.Insert(0, previousEntry[0]);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 5);

            Assert.AreEqual(reret[0].StatementContext.Count, 2);
            Assert.AreEqual(reret[0].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[0].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[1].StatementContext.Count, 2);
            Assert.AreEqual(reret[1].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[1].StatementContext[1], "used to test a factory method involving text broken into units");

            Assert.AreEqual(reret[2].StatementContext.Count, 3);
            Assert.AreEqual(reret[2].StatementContext[0], "in testing code");
            Assert.AreEqual(reret[2].StatementContext[1], "now testing changing context");
            Assert.AreEqual(reret[2].StatementContext[2], "trailing off");

            Assert.AreEqual(reret[3].StatementContext.Count, 4);
            Assert.AreEqual(reret[3].StatementContext[0], "inside a test statement");
            Assert.AreEqual(reret[3].StatementContext[1], "testing changing context");
            Assert.AreEqual(reret[3].StatementContext[2], "trailing off");
            Assert.AreEqual(reret[3].StatementContext[3], "near the end");

            Assert.AreEqual(reret[4].StatementContext.Count, 0);

        }


    }
}
