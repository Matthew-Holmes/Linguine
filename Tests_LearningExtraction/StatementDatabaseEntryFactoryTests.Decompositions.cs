using Infrastructure;
using LearningExtraction;
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
    }
}
