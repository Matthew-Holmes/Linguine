using Infrastructure;
using Infrastructure.DataClasses;
using LearningExtraction;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_LearningExtraction
{
    // [TestClass] - don't need as other partial class file has this attribute
    public partial class StatementDatabaseEntryFactoryTests
    {
        // NOTE - text decompositions that are leaves are not the standard use case
        // but include this case as it simple
        [TestMethod]
        public void FromStatement_DecompIsLeaf_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "word, another word";

            TextDecomposition injective = new TextDecomposition("word", null);
            TextDecomposition rooted = new TextDecomposition("word", null);

            Statement statement = new Statement(parentText, 0, 3, "word", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
        }

        [TestMethod]
        public void FromStatement_DecompIsLeaf_JSONsDeserialise()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "word, another word";

            TextDecomposition injective = new TextDecomposition("word", null);
            TextDecomposition rooted = new TextDecomposition("word", null);

            Statement statement = new Statement(parentText, 0, 3, "word", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var entry = ret[0].Item1;


            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            TextDecomposition? injectiveOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessInjectiveDecompositionJSON, settings);

            TextDecomposition? rootedOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessRootedDecompositionJSON, settings);

            Assert.IsNotNull(injectiveOut);
            Assert.IsNotNull(rootedOut);
        }

        [TestMethod]
        public void FromStatement_DecompIsLeaf_JSONsAreHeadless()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "word, another word";

            TextDecomposition injective = new TextDecomposition("word", null);
            TextDecomposition rooted = new TextDecomposition("word", null);

            Statement statement = new Statement(parentText, 0, 3, "word", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var entry = ret[0].Item1;


            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            TextDecomposition? injectiveOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessInjectiveDecompositionJSON, settings);

            TextDecomposition? rootedOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessRootedDecompositionJSON, settings);

            Assert.IsNotNull(injectiveOut);
            Assert.IsNotNull(rootedOut);

            Assert.AreEqual(injectiveOut.Total, "");
            Assert.AreEqual(rootedOut.Total, "");
        }

        [TestMethod]
        public void FromStatement_DecompIsLeaf_JSONsMatchOtherThanTotals()
        {
            TextualMedia parentText = new TextualMedia();
            parentText.Text = "word, another word";

            TextDecomposition injective = new TextDecomposition("word", null);
            TextDecomposition rooted = new TextDecomposition("word", null);

            Statement statement = new Statement(parentText, 0, 3, "word", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var entry = ret[0].Item1;


            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            TextDecomposition? injectiveOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessInjectiveDecompositionJSON, settings);

            TextDecomposition? rootedOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessRootedDecompositionJSON, settings);

            Assert.IsNotNull(injectiveOut);
            Assert.IsNotNull(rootedOut);

            Assert.AreEqual(injectiveOut.Decomposition, null);
            Assert.AreEqual(rootedOut.Decomposition, null);
        }

        [TestMethod]
        public void FromStatement_DecompIsLeaf_WithDefinition_Runs()
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
        }

        [TestMethod]
        public void FromStatement_DecompIsLeaf_WithDefinition_OneInResult()
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
            var defs = ret[0].Item2;

            Assert.AreEqual(defs.Count, 1);
        }

        [TestMethod]
        public void FromStatement_DecompIsLeaf_WithDefinition_ResultPropertiesMatch()
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
            var defs = ret[0].Item2;

            Assert.AreEqual(defs.Count, 1);

            StatementDefinitionNode node = defs[0];

            Assert.IsNotNull(node);
            Assert.AreEqual(node.DictionaryDefinition.Source, "Made up test definitions");
            Assert.AreEqual(node.DictionaryDefinition.Word, "word");
            Assert.AreEqual(node.DictionaryDefinition.Definition, "a distinct or meaningful part of speech or writing");
        }

        [TestMethod]
        public void FromStatement_DecompIsLeaf_WithDefinition_ResultCorrectLocationInTree()
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
            var defs = ret[0].Item2;

            Assert.AreEqual(defs.Count, 1);

            StatementDefinitionNode node = defs[0];

            Assert.IsNotNull(node);
            Assert.AreEqual(node.IndexAtCurrentLevel, 0);
            Assert.AreEqual(node.CurrentLevel, 0);
        }

        [TestMethod]
        public void FromStatement_DecompHasLeaves_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "a\nsample\ntext\ndecomposition");

            Statement statement = new Statement(parentText, 0, 26, "A sample text decomposition", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
        }

        [TestMethod]
        public void FromStatement_DecompHasLeaves_JSONsDeserialise()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "a\nsample\ntext\ndecomposition");

            Statement statement = new Statement(parentText, 0, 26, "A sample text decomposition", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var entry = ret[0].Item1;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            TextDecomposition? injectiveOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessInjectiveDecompositionJSON, settings);

            TextDecomposition? rootedOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessRootedDecompositionJSON, settings);

            Assert.IsNotNull(injectiveOut);
            Assert.IsNotNull(rootedOut);
        }

        [TestMethod]
        public void FromStatement_DecompHasLeaves_JSONsAreHeadless()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "a\nsample\ntext\ndecomposition");

            Statement statement = new Statement(parentText, 0, 26, "A sample text decomposition", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var entry = ret[0].Item1;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            TextDecomposition? injectiveOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessInjectiveDecompositionJSON, settings);

            TextDecomposition? rootedOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessRootedDecompositionJSON, settings);

            Assert.IsNotNull(injectiveOut);
            Assert.IsNotNull(rootedOut);
            Assert.AreEqual(injectiveOut.Total, "");
            Assert.AreEqual(rootedOut.Total, "");
        }

        [TestMethod]
        public void FromStatement_DecompHasLeaves_JSONsMatchOtherThanTotal()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "a\nsample\ntext\ndecomposition");

            Statement statement = new Statement(parentText, 0, 26, "A sample text decomposition", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var entry = ret[0].Item1;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            TextDecomposition? injectiveOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessInjectiveDecompositionJSON, settings);

            TextDecomposition? rootedOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessRootedDecompositionJSON, settings);

            Assert.IsNotNull(injectiveOut);
            Assert.IsNotNull(rootedOut);
            // we already have tests for the json serialisation methods, so this is just quick check
            Assert.AreEqual(injectiveOut.Decomposition.Count, 4);
            Assert.AreEqual(rootedOut.Decomposition.Count, 4);
        }

        [TestMethod]
        public void FromStatement_DecompHasLeavesWithDefinitions_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = TextDecomposition.FromNewLinedString(
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
            var defs = ret[0].Item2;
        }

        [TestMethod]
        public void FromStatement_DecompHasLeavesWithDefinitions_CorrectNumberReturned()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = TextDecomposition.FromNewLinedString(
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
            var defs = ret[0].Item2;

            Assert.AreEqual(defs.Count, 3);
        }

        [TestMethod]
        public void FromStatement_DecompHasLeavesWithDefinitions_HaveCorrectProperties()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = TextDecomposition.FromNewLinedString(
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
            var defs = ret[0].Item2;

            Assert.AreEqual(defs.Count, 3);
            Assert.AreEqual(defs[0].DictionaryDefinition.Word, "sample");
            Assert.AreEqual(defs[1].DictionaryDefinition.Word, "text");
            Assert.AreEqual(defs[2].DictionaryDefinition.Word, "decomposition");

            Assert.AreEqual(
                defs[0].DictionaryDefinition.Definition, "an example");
            Assert.AreEqual(
                defs[1].DictionaryDefinition.Definition, "natural language stored according to a writing system");
            Assert.AreEqual(
                defs[2].DictionaryDefinition.Definition, "the result of breaking an object of multiple components into its parts");

            Assert.AreEqual(defs[0].DictionaryDefinition.Source, "definitions I made up");
            Assert.AreEqual(defs[1].DictionaryDefinition.Source, "definitions I made up");
            Assert.AreEqual(defs[2].DictionaryDefinition.Source, "definitions I made up");
        }

        [TestMethod]
        public void FromStatement_DecompHasLeavesWithDefinitions_HaveCorrectIndices()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------012345678901234567890123456
            parentText.Text = "A sample text decomposition, then some more";

            TextDecomposition injective = TextDecomposition.FromNewLinedString(
                "A sample text decomposition", "A\nsample\ntext\ndecomposition");
            TextDecomposition rooted = TextDecomposition.FromNewLinedString(
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
            var defs = ret[0].Item2;

            Assert.AreEqual(defs.Count, 3);
            Assert.AreEqual(defs[0].CurrentLevel, 1);
            Assert.AreEqual(defs[1].CurrentLevel, 1);
            Assert.AreEqual(defs[2].CurrentLevel, 1);

            Assert.AreEqual(defs[0].IndexAtCurrentLevel, 1);
            Assert.AreEqual(defs[1].IndexAtCurrentLevel, 2);
            Assert.AreEqual(defs[2].IndexAtCurrentLevel, 3);

        }


        [TestMethod]
        public void FromStatement_DecompHasMultipleLevels_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------0000000000111111111122222222223333333333
            //-----------------0123456789012345678901234567890123456789
            parentText.Text = "My brother-in-law doesn't like hot dogs, and more text";

            TextDecomposition injectiveA = TextDecomposition.FromNewLinedString(
                "brother-in-law", "brother\nin\nlaw");

            TextDecomposition injectiveB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nn't");

            TextDecomposition injectiveC = TextDecomposition.FromNewLinedString(
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


            TextDecomposition rootedA = TextDecomposition.FromNewLinedString(
               "brother-in-law", "brother\nin\nlaw");

            TextDecomposition rootedB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nnot");

            TextDecomposition rootedC = TextDecomposition.FromNewLinedString(
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
        }

        [TestMethod]
        public void FromStatement_DecompHasMultipleLevels_JSONsDeserialise()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------0000000000111111111122222222223333333333
            //-----------------0123456789012345678901234567890123456789
            parentText.Text = "My brother-in-law doesn't like hot dogs, and more text";

            TextDecomposition injectiveA = TextDecomposition.FromNewLinedString(
                "brother-in-law", "brother\nin\nlaw");

            TextDecomposition injectiveB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nn't");

            TextDecomposition injectiveC = TextDecomposition.FromNewLinedString(
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


            TextDecomposition rootedA = TextDecomposition.FromNewLinedString(
               "brother-in-law", "brother\nin\nlaw");

            TextDecomposition rootedB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nnot");

            TextDecomposition rootedC = TextDecomposition.FromNewLinedString(
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
            var entry = ret[0].Item1;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            TextDecomposition? injectiveOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessInjectiveDecompositionJSON, settings);

            TextDecomposition? rootedOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessRootedDecompositionJSON, settings);

            Assert.IsNotNull(injectiveOut);
            Assert.IsNotNull(rootedOut);
        }

        [TestMethod]
        public void FromStatement_DecompHasMultipleLevels_JSONsAreHeadless()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------0000000000111111111122222222223333333333
            //-----------------0123456789012345678901234567890123456789
            parentText.Text = "My brother-in-law doesn't like hot dogs, and more text";

            TextDecomposition injectiveA = TextDecomposition.FromNewLinedString(
                "brother-in-law", "brother\nin\nlaw");

            TextDecomposition injectiveB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nn't");

            TextDecomposition injectiveC = TextDecomposition.FromNewLinedString(
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


            TextDecomposition rootedA = TextDecomposition.FromNewLinedString(
               "brother-in-law", "brother\nin\nlaw");

            TextDecomposition rootedB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nnot");

            TextDecomposition rootedC = TextDecomposition.FromNewLinedString(
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
            var entry = ret[0].Item1;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            TextDecomposition? injectiveOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessInjectiveDecompositionJSON, settings);

            TextDecomposition? rootedOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessRootedDecompositionJSON, settings);

            Assert.IsNotNull(injectiveOut);
            Assert.IsNotNull(rootedOut);
            Assert.AreEqual(injectiveOut.Total, "");
            Assert.AreEqual(rootedOut.Total, "");
        }

        [TestMethod]
        public void FromStatement_DecompHasMultipleLevels_JSONsMatchOtherThanTotal()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------0000000000111111111122222222223333333333
            //-----------------0123456789012345678901234567890123456789
            parentText.Text = "My brother-in-law doesn't like hot dogs, and more text";

            TextDecomposition injectiveA = TextDecomposition.FromNewLinedString(
                "brother-in-law", "brother\nin\nlaw");

            TextDecomposition injectiveB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nn't");

            TextDecomposition injectiveC = TextDecomposition.FromNewLinedString(
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


            TextDecomposition rootedA = TextDecomposition.FromNewLinedString(
               "brother-in-law", "brother\nin\nlaw");

            TextDecomposition rootedB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nnot");

            TextDecomposition rootedC = TextDecomposition.FromNewLinedString(
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
            var entry = ret[0].Item1;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            TextDecomposition? injectiveOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessInjectiveDecompositionJSON, settings);

            TextDecomposition? rootedOut = JsonConvert.DeserializeObject<TextDecomposition>(entry.HeadlessRootedDecompositionJSON, settings);

            Assert.IsNotNull(injectiveOut);
            Assert.IsNotNull(rootedOut);
            Assert.AreEqual(injectiveOut.Decomposition.Count, 5);
            Assert.AreEqual(rootedOut.Decomposition.Count, 5);

            Assert.AreEqual(injectiveOut.Decomposition[1].Decomposition.Count, 3);
            Assert.AreEqual(rootedOut.Decomposition[1].Decomposition.Count, 3);

            Assert.AreEqual(injectiveOut.Decomposition[1].Decomposition[0].Total, "brother");
            Assert.AreEqual(rootedOut.Decomposition[1].Decomposition[0].Total, "brother");
        }

        [TestMethod]
        public void FromStatement_DecompHasMultipleLevelsWithDefinitions_Runs()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------0000000000111111111122222222223333333333
            //-----------------0123456789012345678901234567890123456789
            parentText.Text = "My brother-in-law doesn't like hot dogs, and more text";

            TextDecomposition injectiveA = TextDecomposition.FromNewLinedString(
                "brother-in-law", "brother\nin\nlaw");

            TextDecomposition injectiveB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nn't");

            TextDecomposition injectiveC = TextDecomposition.FromNewLinedString(
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


            TextDecomposition rootedA = TextDecomposition.FromNewLinedString(
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

            TextDecomposition rootedB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nnot");

            rootedB.Decomposition[1].Definition = new DictionaryDefinition
            {
                Word = "not",
                Definition = "word indicating negation",
                Source = "definitions I made up"
            };

            TextDecomposition rootedC = TextDecomposition.FromNewLinedString(
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
            var defs = ret[0].Item2;
        }


        [TestMethod]
        public void FromStatement_DecompHasMultipleLevelsWithDefinitions_CorrectNumberReturned()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------0000000000111111111122222222223333333333
            //-----------------0123456789012345678901234567890123456789
            parentText.Text = "My brother-in-law doesn't like hot dogs, and more text";

            TextDecomposition injectiveA = TextDecomposition.FromNewLinedString(
                "brother-in-law", "brother\nin\nlaw");

            TextDecomposition injectiveB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nn't");

            TextDecomposition injectiveC = TextDecomposition.FromNewLinedString(
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


            TextDecomposition rootedA = TextDecomposition.FromNewLinedString(
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

            TextDecomposition rootedB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nnot");

            rootedB.Decomposition[1].Definition = new DictionaryDefinition
            {
                Word = "not",
                Definition = "word indicating negation",
                Source = "definitions I made up"
            };

            TextDecomposition rootedC = TextDecomposition.FromNewLinedString(
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
            var defs = ret[0].Item2;

            Assert.AreEqual(defs.Count, 6);
        }

        [TestMethod]
        public void FromStatement_DecompHasMultipleLevelsWithDefinitions_CorrectIndices()
        {
            TextualMedia parentText = new TextualMedia();
            //-----------------0000000000111111111122222222223333333333
            //-----------------0123456789012345678901234567890123456789
            parentText.Text = "My brother-in-law doesn't like hot dogs, and more text";

            TextDecomposition injectiveA = TextDecomposition.FromNewLinedString(
                "brother-in-law", "brother\nin\nlaw");

            TextDecomposition injectiveB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nn't");

            TextDecomposition injectiveC = TextDecomposition.FromNewLinedString(
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


            TextDecomposition rootedA = TextDecomposition.FromNewLinedString(
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

            TextDecomposition rootedB = TextDecomposition.FromNewLinedString(
                "doesn't", "does\nnot");

            rootedB.Decomposition[1].Definition = new DictionaryDefinition
            {
                Word = "not",
                Definition = "word indicating negation",
                Source = "definitions I made up"
            };

            TextDecomposition rootedC = TextDecomposition.FromNewLinedString(
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

            // my brother-in-law doesn't like hot dogs    level 0
            //  *          |         |     *      |       level 1
            //       *     *   *    *   *       *   *     level 2 

            Statement statement = new Statement(parentText, 0, 38, "My brother-in-law doesn't like hot dogs", new List<string>(), injective, rooted);

            var ret = StatementDatabaseEntryFactory.FromStatements(new List<Statement> { statement }, null);
            var defs = ret[0].Item2;

            Assert.AreEqual(defs.Count, 6);

            StatementDefinitionNode bilDef = defs.Where(def => def.DictionaryDefinition.Word == "brother-in-law").First();
            Assert.AreEqual(bilDef.CurrentLevel, 1);
            Assert.AreEqual(bilDef.IndexAtCurrentLevel, 1);

            StatementDefinitionNode bDef = defs.Where(def => def.DictionaryDefinition.Word == "brother").First();
            Assert.AreEqual(bDef.CurrentLevel, 2);
            Assert.AreEqual(bDef.IndexAtCurrentLevel, 0);

            StatementDefinitionNode lawDef = defs.Where(def => def.DictionaryDefinition.Word == "law").First();
            Assert.AreEqual(lawDef.CurrentLevel, 2);
            Assert.AreEqual(lawDef.IndexAtCurrentLevel, 2);

            StatementDefinitionNode notDef = defs.Where(def => def.DictionaryDefinition.Word == "not").First();
            Assert.AreEqual(notDef.CurrentLevel, 2);
            Assert.AreEqual(notDef.IndexAtCurrentLevel, 4);

            StatementDefinitionNode hotDef = defs.Where(def => def.DictionaryDefinition.Word == "hot").First();
            Assert.AreEqual(hotDef.CurrentLevel, 2);
            Assert.AreEqual(hotDef.IndexAtCurrentLevel, 5);

            StatementDefinitionNode lkDef = defs.Where(def => def.DictionaryDefinition.Word == "like").First();
            Assert.AreEqual(lkDef.CurrentLevel, 1);
            Assert.AreEqual(lkDef.IndexAtCurrentLevel, 3);

        }

    }
}
