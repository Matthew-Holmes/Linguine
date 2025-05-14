using Serilog;
using System;
using System.Linq;
using DataClasses;
using System.Text.RegularExpressions;

namespace Linguine
{
    public static class StatementHelper
    {
        public static WordInContext? AsWordInContext(DictionaryDefinition def, Statement context)
        {
            Tuple<int, int, int> startLenIndex = StatementHelper.GetStartLenIndexOfDefinition(context, def);

            if (startLenIndex is null) { return null; }

            WordInContext ret = new WordInContext(
                context.StatementText,
                startLenIndex.Item1,
                startLenIndex.Item2,
                context,
                startLenIndex.Item3
            );
            return ret;
        }


        public static Tuple<string, string, string>? RunFromStatementDefIndex(Statement statement, int defIndex)
        {
            Tuple<int, int>? startLen = GetStartLenFromIndex(statement, defIndex);

            if (startLen is null)
            {
                return null;
            }

            WordInContext wic = new WordInContext(statement.StatementText, startLen.Item1, startLen.Item2, statement, defIndex);

            return AsRun(wic);
        }

        public static Tuple<string, string, string> AsRun(WordInContext wic)
        {
            string prepend;

            if (wic.WordStart == 0)
            {
                prepend = "";
            }
            else
            {
                prepend = wic.StatementText.Substring(0, wic.WordStart);
            }

            string word = wic.StatementText.Substring(wic.WordStart, wic.Len);

            int appendIndex = wic.WordStart + wic.Len;
            string append;
            if (appendIndex >= wic.StatementText.Length)
            {
                append = "";
            }
            else
            {
                append = wic.StatementText.Substring(appendIndex);
            }
            // remove newlines since they make it look weird
            // TODO - what about song lyrics/subtitles etc
            // should we have a "meaningul newlines" flag??
            prepend = Regex.Replace(prepend, @"\t|\n|\r", " ");
            word    = Regex.Replace(word,    @"\t|\n|\r", " ");
            append  = Regex.Replace(append,  @"\t|\n|\r", " ");

            return Tuple.Create(prepend, word, append);

        }


        public static Tuple<int, int, int>? GetStartLenIndexOfDefinition(
            Statement statement, DictionaryDefinition def)
        {
            TextDecomposition flatInjective = statement.InjectiveDecomposition.Flattened();
            TextDecomposition flatRooted    = statement.RootedDecomposition.Flattened();

            if (flatInjective.Decomposition.Count != flatRooted.Decomposition.Count)
            {
                throw new Exception("mismatched decompositions");
            }

            int start = 0;
            int index = 0;

            if (flatInjective.Decomposition is null)
            {
                // if the statement a leaf
                return Tuple.Create(0, statement.StatementText.Length, 0);
            }

            while (index < flatInjective.Decomposition.Count)
            {
                String toFind = flatInjective.Decomposition[index].Total;

                if (statement.StatementText.Substring(start, toFind.Length) != toFind)
                {
                    start++; continue;
                }

                DictionaryDefinition? thisDef = flatRooted.Decomposition[index].Definition;

                if (thisDef is null)
                {
                    index++; continue;
                }

                if (thisDef.DatabasePrimaryKey == def.DatabasePrimaryKey)
                {
                    return Tuple.Create(start, flatInjective.Decomposition[index].Total.Length, index);
                }

                index++;
            }

            Log.Warning("failed to find definition {definition} in {statememt}", def.Word, statement.StatementText);
            return null;
        }

        public static Tuple<int, int>? GetStartLenFromIndex(
            Statement statement, int defIndex)
        {
            TextDecomposition flatInjective = statement.InjectiveDecomposition.Flattened();
            TextDecomposition flatRooted = statement.RootedDecomposition.Flattened();

            int start = 0;
            int index = 0;

            while (index < flatInjective.Decomposition.Count)
            {
                String toFind = flatInjective.Decomposition[index].Total;

                if (statement.StatementText.Substring(start, toFind.Length) != toFind)
                {
                    start++; continue;
                }

                if (index == defIndex)
                {
                    return Tuple.Create(start, toFind.Length);
                }

                index++;
            }

            Log.Warning("failed to find definition at index {defIndex} in {statememt}", defIndex, statement.StatementText);
            return null;
        }

    }
}
