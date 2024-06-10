using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_LearningExtraction
{
    [TestClass]
    public partial class StatementFactoryTest
    {
        

        // NOTE - text decompositions that are leaves are not the standard use case
        // but include this case as it simple
        [TestMethod]
        public void FromDatabaseEntries_DecompIsLeaf_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "word, another word";

            TextDecomposition injective = new TextDecomposition("word", null);
            TextDecomposition rooted = new TextDecomposition("word", null);

            Statement statement = new Statement(parentText, 0, 3, "word", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var reret = StatementFactory.FromDatabaseEntries(ret);
        }

        
        [TestMethod]
        public void FromDatabaseEntries_DecompIsLeaf_DecompsReturn()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "word, another word";

            TextDecomposition injective = new TextDecomposition("word", null);
            TextDecomposition rooted = new TextDecomposition("word", null);

            Statement statement = new Statement(parentText, 0, 3, "word", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 1);

            var stat = reret[0];

            Assert.AreEqual(stat.InjectiveDecomposition.Total, "word");
            Assert.IsNull(stat.InjectiveDecomposition.Decomposition);
            Assert.AreEqual(stat.RootedDecomposition.Total, "word");
            Assert.IsNull(stat.RootedDecomposition.Decomposition);

        }
       

        [TestMethod]
        public void FromDatabaseEntries_DecompIsLeaf_WithDefinition_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "word, another word";

            TextDecomposition injective = new TextDecomposition("word", null);
            TextDecomposition rooted = new TextDecomposition("word", null);

            rooted.Definition = new DictionaryDefinition
            {
                Word = "word",
                Definition = "a distinct or meaningful part of speech or writing",
                Source = "Made up test definitions"
            };

            Statement statement = new Statement(parentText, 0, 3, "word", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);
        }

        [TestMethod]
        public void FromDatabaseEntries_DecompIsLeaf_WithDefinition_DefinitionReturns()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "word, another word";

            TextDecomposition injective = new TextDecomposition("word", null);
            TextDecomposition rooted = new TextDecomposition("word", null);

            rooted.Definition = new DictionaryDefinition
            {
                Word = "word",
                Definition = "a distinct or meaningful part of speech or writing",
                Source = "Made up test definitions"
            };

            Statement statement = new Statement(parentText, 0, 3, "word", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 1);
            Assert.AreEqual(reret[0].RootedDecomposition.Definition.Word, "word");
            Assert.AreEqual(
                reret[0].RootedDecomposition.Definition.Definition, "a distinct or meaningful part of speech or writing");
            Assert.AreEqual(reret[0].RootedDecomposition.Definition.Source , "Made up test definitions");

        }

   
        
        
        [TestMethod]
        public void FromDatabaseEntries_DecompHasLeaves_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = DecompositionHelper.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = DecompositionHelper.FromNewLinedString(
                "A sample text decomposition", "a\nsample\ntext\ndecomposition");

            Statement statement = new Statement(parentText, 0, 26, "A sample text decomposition", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var reret = StatementFactory.FromDatabaseEntries(ret);
        }

        [TestMethod]
        public void FromDatabaseEntries_DecompHasLeaves_DecompsReturn()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = DecompositionHelper.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = DecompositionHelper.FromNewLinedString(
                "A sample text decomposition", "a\nsample\ntext\ndecomposition");

            Statement statement = new Statement(parentText, 0, 26, "A sample text decomposition", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret.Count, 1);

            Assert.AreEqual(reret[0].InjectiveDecomposition.Total, injective.Total);
            Assert.AreEqual(reret[0].InjectiveDecomposition.Decomposition.Count, injective.Decomposition.Count);
            Assert.AreEqual(reret[0].InjectiveDecomposition.Decomposition[0].Total , injective.Decomposition[0].Total);
            Assert.AreEqual(reret[0].InjectiveDecomposition.Decomposition[1].Total , injective.Decomposition[1].Total);
            Assert.AreEqual(reret[0].InjectiveDecomposition.Decomposition[2].Total , injective.Decomposition[2].Total);
            Assert.AreEqual(reret[0].InjectiveDecomposition.Decomposition[3].Total , injective.Decomposition[3].Total);
            Assert.IsNull(reret[0].InjectiveDecomposition.Decomposition[0].Decomposition);
            Assert.IsNull(reret[0].InjectiveDecomposition.Decomposition[1].Decomposition);
            Assert.IsNull(reret[0].InjectiveDecomposition.Decomposition[2].Decomposition);
            Assert.IsNull(reret[0].InjectiveDecomposition.Decomposition[3].Decomposition);

            Assert.AreEqual(reret[0].RootedDecomposition.Total, rooted.Total);
            Assert.AreEqual(reret[0].RootedDecomposition.Decomposition.Count, rooted.Decomposition.Count);
            Assert.AreEqual(reret[0].RootedDecomposition.Decomposition[0].Total, rooted.Decomposition[0].Total );
            Assert.AreEqual(reret[0].RootedDecomposition.Decomposition[1].Total, rooted.Decomposition[1].Total );
            Assert.AreEqual(reret[0].RootedDecomposition.Decomposition[2].Total, rooted.Decomposition[2].Total );
            Assert.AreEqual(reret[0].RootedDecomposition.Decomposition[3].Total, rooted.Decomposition[3].Total );
            Assert.IsNull(reret[0].RootedDecomposition.Decomposition[0].Decomposition);
            Assert.IsNull(reret[0].RootedDecomposition.Decomposition[1].Decomposition);
            Assert.IsNull(reret[0].RootedDecomposition.Decomposition[2].Decomposition);
            Assert.IsNull(reret[0].RootedDecomposition.Decomposition[3].Decomposition);
        }

      
      
        [TestMethod]
        public void FromDatabaseEntries_DecompHasLeavesWithDefinitions_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = DecompositionHelper.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = DecompositionHelper.FromNewLinedString(
                "A sample text decomposition", "a\nsample\ntext\ndecomposition");

            Assert.IsNotNull(rooted.Decomposition);

            rooted.Decomposition[1].Definition = new DictionaryDefinition
            {
                Word = "sample",
                Definition = "an example",
                Source = "definitions I made up"
            };

            rooted.Decomposition[2].Definition = new DictionaryDefinition
            {
                Word = "text",
                Definition = "natural language stored according to a writing system",
                Source = "definitions I made up"
            };

            rooted.Decomposition[3].Definition = new DictionaryDefinition
            {
                Word = "decomposition",
                Definition = "the result of breaking an object of multiple components into its parts",
                Source = "definitions I made up"
            };

            Statement statement = new Statement(parentText, 0, 26, "A sample text decomposition", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var reret = StatementFactory.FromDatabaseEntries(ret);
        }

        [TestMethod]
        public void FromDatabaseEntries_DecompHasLeavesWithDefinitions_DefinitionsAdded()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = DecompositionHelper.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = DecompositionHelper.FromNewLinedString(
                "A sample text decomposition", "a\nsample\ntext\ndecomposition");

            Assert.IsNotNull(rooted.Decomposition);

            rooted.Decomposition[1].Definition = new DictionaryDefinition
            {
                Word = "sample",
                Definition = "an example",
                Source = "definitions I made up"
            };

            rooted.Decomposition[2].Definition = new DictionaryDefinition
            {
                Word = "text",
                Definition = "natural language stored according to a writing system",
                Source = "definitions I made up"
            };

            rooted.Decomposition[3].Definition = new DictionaryDefinition
            {
                Word = "decomposition",
                Definition = "the result of breaking an object of multiple components into its parts",
                Source = "definitions I made up"
            };

            Statement statement = new Statement(parentText, 0, 26, "A sample text decomposition", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var reret = StatementFactory.FromDatabaseEntries(ret);

            Assert.AreEqual(reret[0].RootedDecomposition.Decomposition[1].Definition.Word, "sample");
            Assert.AreEqual(reret[0].RootedDecomposition.Decomposition[2].Definition.Word, "text");
            Assert.AreEqual(reret[0].RootedDecomposition.Decomposition[3].Definition.Word, "decomposition");
        }

       
      

        [TestMethod]
        public void FromDatabaseEntries_DecompHasMultipleLevels_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------0000000000111111111122222222223333333333
            //-----------------0123456789012345678901234567890123456789
            parentText.Text = "My brother-in-law doesn't like hot dogs, and more text";

            TextDecomposition injectiveA = DecompositionHelper.FromNewLinedString(
                "brother-in-law", "brother\nin\nlaw");

            TextDecomposition injectiveB = DecompositionHelper.FromNewLinedString(
                "doesn't", "does\nn't");

            TextDecomposition injectiveC = DecompositionHelper.FromNewLinedString(
                "hot dogs", "hot\ndogs");

            TextDecomposition injective = new TextDecomposition(parentText.Text,
                new List<TextDecomposition>
                {
                    new TextDecomposition("My", null),
                    injectiveA,
                    injectiveB,
                    new TextDecomposition("like", null),
                    injectiveC
                });


            TextDecomposition rootedA = DecompositionHelper.FromNewLinedString(
               "brother-in-law", "brother\nin\nlaw");

            TextDecomposition rootedB = DecompositionHelper.FromNewLinedString(
                "doesn't", "does\nnot");

            TextDecomposition rootedC = DecompositionHelper.FromNewLinedString(
                "hot dog", "hot\ndog");

            TextDecomposition rooted = new TextDecomposition(parentText.Text,
                new List<TextDecomposition>
                {
                    new TextDecomposition("my", null),
                    rootedA,
                    rootedB,
                    new TextDecomposition("like", null),
                    rootedC
                });



            Statement statement = new Statement(parentText, 0, 38, "My brother-in-law doesn't like hot dogs", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var reret = StatementFactory.FromDatabaseEntries(ret);
        }

        

        [TestMethod]
        public void FromDatabaseEntries_DecompHasMultipleLevels_DecompReturns()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------0000000000111111111122222222223333333333
            //-----------------0123456789012345678901234567890123456789
            parentText.Text = "My brother-in-law doesn't like hot dogs, and more text";

            TextDecomposition injectiveA = DecompositionHelper.FromNewLinedString(
                "brother-in-law", "brother\nin\nlaw");

            TextDecomposition injectiveB = DecompositionHelper.FromNewLinedString(
                "doesn't", "does\nn't");

            TextDecomposition injectiveC = DecompositionHelper.FromNewLinedString(
                "hot dogs", "hot\ndogs");

            TextDecomposition injective = new TextDecomposition(parentText.Text,
                new List<TextDecomposition>
                {
                    new TextDecomposition("My", null),
                    injectiveA,
                    injectiveB,
                    new TextDecomposition("like", null),
                    injectiveC
                });


            TextDecomposition rootedA = DecompositionHelper.FromNewLinedString(
               "brother-in-law", "brother\nin\nlaw");

            TextDecomposition rootedB = DecompositionHelper.FromNewLinedString(
                "doesn't", "does\nnot");

            TextDecomposition rootedC = DecompositionHelper.FromNewLinedString(
                "hot dog", "hot\ndog");

            TextDecomposition rooted = new TextDecomposition(parentText.Text,
                new List<TextDecomposition>
                {
                    new TextDecomposition("my", null),
                    rootedA,
                    rootedB,
                    new TextDecomposition("like", null),
                    rootedC
                });



            Statement statement = new Statement(parentText, 0, 38, "My brother-in-law doesn't like hot dogs", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);

            var reret = StatementFactory.FromDatabaseEntries(ret);

            // already tested the serialisation code, so just check a necessary condition, not sufficient

            Assert.AreEqual(reret[0].InjectiveDecomposition.Flattened().Units.Count, injective.Flattened().Units.Count());
            Assert.AreEqual(reret[0].RootedDecomposition.Flattened().Units.Count, rooted.Flattened().Units.Count());
        }
        

     
      
        [TestMethod]
        public void FromDatabaseEntries_DecompHasMultipleLevelsWithDefinitions_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------0000000000111111111122222222223333333333
            //-----------------0123456789012345678901234567890123456789
            parentText.Text = "My brother-in-law doesn't like hot dogs, and more text";

            TextDecomposition injectiveA = DecompositionHelper.FromNewLinedString(
                "brother-in-law", "brother\nin\nlaw");

            TextDecomposition injectiveB = DecompositionHelper.FromNewLinedString(
                "doesn't", "does\nn't");

            TextDecomposition injectiveC = DecompositionHelper.FromNewLinedString(
                "hot dogs", "hot\ndogs");

            TextDecomposition injective = new TextDecomposition(parentText.Text,
                new List<TextDecomposition>
                {
                    new TextDecomposition("My", null),
                    injectiveA,
                    injectiveB,
                    new TextDecomposition("like", null),
                    injectiveC
                });


            TextDecomposition rootedA = DecompositionHelper.FromNewLinedString(
               "brother-in-law", "brother\nin\nlaw");

            rootedA.Definition = new DictionaryDefinition
            {
                Word = "brother-in-law",
                Definition = "spouses male sibling, or siblings husband",
                Source = "definitions I made up"
            };

            rootedA.Decomposition[0].Definition = new DictionaryDefinition
            {
                Word = "brother",
                Definition = "male sibling",
                Source = "definitions I made up"
            };

            rootedA.Decomposition[2].Definition = new DictionaryDefinition
            {
                Word = "law",
                Definition = "a system of rules backed up by a court or authority",
                Source = "definitions I made up"
            };

            TextDecomposition rootedB = DecompositionHelper.FromNewLinedString(
                "doesn't", "does\nnot");

            rootedB.Decomposition[1].Definition = new DictionaryDefinition
            {
                Word = "not",
                Definition = "word indicating negation",
                Source = "definitions I made up"
            };

            TextDecomposition rootedC = DecompositionHelper.FromNewLinedString(
                "hot dog", "hot\ndog");

            rootedC.Decomposition[0].Definition = new DictionaryDefinition
            {
                Word = "hot",
                Definition = "high in temperature",
                Source = "definitions I made up"
            };

            TextDecomposition rooted = new TextDecomposition(parentText.Text,
                new List<TextDecomposition>
                {
                    new TextDecomposition("my", null),
                    rootedA,
                    rootedB,
                    new TextDecomposition("like", null),
                    rootedC
                });

            rooted.Decomposition[3].Definition = new DictionaryDefinition
            {
                Word = "like",
                Definition = "have positive feelings for",
                Source = "definitions I made up"
            };

            Statement statement = new Statement(parentText, 0, 38, "My brother-in-law doesn't like hot dogs", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var reret = StatementFactory.FromDatabaseEntries(ret);
        }


        [TestMethod]
        public void FromDatabaseEntries_DecompHasMultipleLevelsWithDefinitions_DefinitionsAdded()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------0000000000111111111122222222223333333333
            //-----------------0123456789012345678901234567890123456789
            parentText.Text = "My brother-in-law doesn't like hot dogs, and more text";

            TextDecomposition injectiveA = DecompositionHelper.FromNewLinedString(
                "brother-in-law", "brother\nin\nlaw");

            TextDecomposition injectiveB = DecompositionHelper.FromNewLinedString(
                "doesn't", "does\nn't");

            TextDecomposition injectiveC = DecompositionHelper.FromNewLinedString(
                "hot dogs", "hot\ndogs");

            TextDecomposition injective = new TextDecomposition(parentText.Text,
                new List<TextDecomposition>
                {
                    new TextDecomposition("My", null),
                    injectiveA,
                    injectiveB,
                    new TextDecomposition("like", null),
                    injectiveC
                });


            TextDecomposition rootedA = DecompositionHelper.FromNewLinedString(
               "brother-in-law", "brother\nin\nlaw");

            rootedA.Definition = new DictionaryDefinition
            {
                Word = "brother-in-law",
                Definition = "spouses male sibling, or siblings husband",
                Source = "definitions I made up"
            };

            rootedA.Decomposition[0].Definition = new DictionaryDefinition
            {
                Word = "brother",
                Definition = "male sibling",
                Source = "definitions I made up"
            };

            rootedA.Decomposition[2].Definition = new DictionaryDefinition
            {
                Word = "law",
                Definition = "a system of rules backed up by a court or authority",
                Source = "definitions I made up"
            };

            TextDecomposition rootedB = DecompositionHelper.FromNewLinedString(
                "doesn't", "does\nnot");

            rootedB.Decomposition[1].Definition = new DictionaryDefinition
            {
                Word = "not",
                Definition = "word indicating negation",
                Source = "definitions I made up"
            };

            TextDecomposition rootedC = DecompositionHelper.FromNewLinedString(
                "hot dog", "hot\ndog");

            rootedC.Decomposition[0].Definition = new DictionaryDefinition
            {
                Word = "hot",
                Definition = "high in temperature",
                Source = "definitions I made up"
            };

            TextDecomposition rooted = new TextDecomposition(parentText.Text,
                new List<TextDecomposition>
                {
                    new TextDecomposition("my", null),
                    rootedA,
                    rootedB,
                    new TextDecomposition("like", null),
                    rootedC
                });

            rooted.Decomposition[3].Definition = new DictionaryDefinition
            {
                Word = "like",
                Definition = "have positive feelings for",
                Source = "definitions I made up"
            };

            Statement statement = new Statement(parentText, 0, 38, "My brother-in-law doesn't like hot dogs", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var reret = StatementFactory.FromDatabaseEntries(ret);

            // my brother-in-law doesn't like hot dogs    level 0
            //  *          |         |     *      |       level 1
            //       *     *   *    *   *       *   *     level 2 

            TextDecomposition back = reret[0].RootedDecomposition;

            // level 0
            Assert.IsNull(back.Definition);
            // level 1
            Assert.IsNull(back.Decomposition[0].Definition); // my
            Assert.AreEqual(back.Decomposition[1].Definition.Word, "brother-in-law");
            Assert.IsNull(back.Decomposition[2].Definition); // doesn't
            Assert.AreEqual(back.Decomposition[3].Definition.Word, "like");
            Assert.IsNull(back.Decomposition[4].Definition); // hot dogs
            // level 2
            Assert.AreEqual(back.Decomposition[1].Decomposition[0].Definition.Word, "brother");
            Assert.IsNull(back.Decomposition[1].Decomposition[1].Definition); // in
            Assert.AreEqual(back.Decomposition[1].Decomposition[2].Definition.Word, "law");
            Assert.IsNull(back.Decomposition[2].Decomposition[0].Definition); // does
            Assert.AreEqual(back.Decomposition[2].Decomposition[1].Definition.Word, "not");
            Assert.AreEqual(back.Decomposition[4].Decomposition[0].Definition.Word, "hot");
            Assert.IsNull(back.Decomposition[4].Decomposition[1].Definition); // dogs
        }
    }
}
