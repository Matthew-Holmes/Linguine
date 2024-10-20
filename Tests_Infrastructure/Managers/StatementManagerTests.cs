using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_Infrastructure
{
    [TestClass]
    public class StatementManagerTests
    {

        [TestMethod]
        public void GetAllUniqueDefinitions_EmptyStatements_ReturnsEmptySet()
        {
            var statements = new List<Statement>();

            HashSet<DictionaryDefinition> result = StatementManager.GetAllUniqueDefinitions(statements);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetAllUniqueDefinitions_StatementsWithNoDefinitions_ReturnsEmptySet()
        {
            var statements = new List<Statement>
            {
                new Statement(
                    new TextualMedia(), 0, 10,
                    "statement text",
                    new List<string>(),
                    new TextDecomposition("statement text", null),
                    new TextDecomposition("statement text", null))
            };

            var result = StatementManager.GetAllUniqueDefinitions(statements);

            Assert.AreEqual(0, result.Count);
        }


        [TestMethod]
        public void GetAllUniqueDefinitions_StatementsWithUniqueDefinitions_ReturnsCorrectDefinitions()
        {
            // Arrange
            var def1 = new DictionaryDefinition { ID = 1, Word = "test", Definition = "Definition of test" };
            var def2 = new DictionaryDefinition { ID = 2, Word = "demo", Definition = "Definition of demo" };
            var def3 = new DictionaryDefinition { ID = 2, Word = "text", Definition = "Definition of text" };
            var def4 = new DictionaryDefinition { ID = 2, Word = "writing", Definition = "Definition of writing" };

            var statement1 = new Statement(
                new TextualMedia(), 0, 10,
                "test text",
                new List<string>(),
                new TextDecomposition("test text", null),
                new TextDecomposition("test text", new List<TextDecomposition>
                {
                    new TextDecomposition("test", null),
                    new TextDecomposition("text", null),
                }));

            var statement2 = new Statement(
                new TextualMedia(), 0, 10,
                "demo writing",
                new List<string>(),
                new TextDecomposition("demo writing", null),
                new TextDecomposition("demo writing", new List<TextDecomposition>
                {
                    new TextDecomposition("demo", null),
                    new TextDecomposition("writing", null)
                }));


            statement1.RootedDecomposition.Decomposition[0].Definition = def1;
            statement1.RootedDecomposition.Decomposition[1].Definition = def3;
            statement2.RootedDecomposition.Decomposition[0].Definition = def2;
            statement2.RootedDecomposition.Decomposition[1].Definition = def4;

            var statements = new List<Statement>
            {
                statement1,
                statement2
            };
  

            var result = StatementManager.GetAllUniqueDefinitions(statements);

            Assert.AreEqual(4, result.Count);
            Assert.IsTrue(result.Contains(def1));
            Assert.IsTrue(result.Contains(def2));
            Assert.IsTrue(result.Contains(def3));
            Assert.IsTrue(result.Contains(def4));
        }


        [TestMethod]
        public void GetAllUniqueDefinitions_StatementsWithSameDefinitions_ReturnsCorrectDefinitions()
        {
            // Arrange
            var def1 = new DictionaryDefinition { ID = 1, Word = "test", Definition = "Definition of test" };
            var def2 = new DictionaryDefinition { ID = 2, Word = "demo", Definition = "Definition of demo" };
            var def3 = new DictionaryDefinition { ID = 2, Word = "text", Definition = "Definition of text" };

            var statement1 = new Statement(
                new TextualMedia(), 0, 10,
                "test text",
                new List<string>(),
                new TextDecomposition("test text", null),
                new TextDecomposition("test text", new List<TextDecomposition>
                {
                    new TextDecomposition("test", null),
                    new TextDecomposition("text", null),
                }));

            var statement2 = new Statement(
                new TextualMedia(), 0, 10,
                "demo text ",
                new List<string>(),
                new TextDecomposition("demo text", null),
                new TextDecomposition("demo text", new List<TextDecomposition>
                {
                    new TextDecomposition("demo", null),
                    new TextDecomposition("text", null)
                }));


            statement1.RootedDecomposition.Decomposition[0].Definition = def1;
            statement1.RootedDecomposition.Decomposition[1].Definition = def3;
            statement2.RootedDecomposition.Decomposition[0].Definition = def2;
            statement2.RootedDecomposition.Decomposition[1].Definition = def3;

            var statements = new List<Statement>
            {
                statement1,
                statement2
            };


            var result = StatementManager.GetAllUniqueDefinitions(statements);

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Contains(def1));
            Assert.IsTrue(result.Contains(def2));
            Assert.IsTrue(result.Contains(def3));
        }

        [TestMethod]
        public void GetAllUniqueDefinitions_LeafAndTree_ReturnsCorrectDefinitions()
        {
            // Arrange
            var def1 = new DictionaryDefinition { ID = 1, Word = "test", Definition = "Definition of test" };
            var def2 = new DictionaryDefinition { ID = 2, Word = "demo", Definition = "Definition of demo" };
            var def3 = new DictionaryDefinition { ID = 2, Word = "text", Definition = "Definition of text" };

            var statement1 = new Statement(
                new TextualMedia(), 0, 10,
                "test text",
                new List<string>(),
                new TextDecomposition("test text", null),
                new TextDecomposition("test text", new List<TextDecomposition>
                {
                    new TextDecomposition("test", null),
                    new TextDecomposition("text", null),
                }));

            var statement2 = new Statement(
                new TextualMedia(), 0, 10,
                "demo ",
                new List<string>(),
                new TextDecomposition("demo", null),
                new TextDecomposition("demo", null)
                );


            statement1.RootedDecomposition.Decomposition[0].Definition = def1;
            statement1.RootedDecomposition.Decomposition[1].Definition = def3;
            statement2.RootedDecomposition.Definition = def2;

            var statements = new List<Statement>
            {
                statement1,
                statement2
            };


            var result = StatementManager.GetAllUniqueDefinitions(statements);

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Contains(def1));
            Assert.IsTrue(result.Contains(def2));
            Assert.IsTrue(result.Contains(def3));
        }
    }
}
