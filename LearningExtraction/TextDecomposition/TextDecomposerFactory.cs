using Agents;
using Agents.DummyAgents;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class TextDecomposerFactory
    {
        public static TextDecomposer MakeStatementsDecomposer(string openAI_APIKey)
        {
            TextDecomposer ret = new TextDecomposer();

            ret.MaxVolumeToProcess    = 5000;
            ret.PaddingCharacterCount = 500;
            ret.JoinCharacterCount    = 500;

            ret.StandardAgent        = new StatementGeneratorAgent(openAI_APIKey); // too expensive for dev
            ret.HighPerformanceAgent = new StatementGeneratorAgent(openAI_APIKey/*, highPowered: true*/);
            ret.FallbackAgent        = new SentenceDecompositionAgent();

            return ret;
        }

        public static TextDecomposer MakeUnitsDecomposer(string openAI_APIKey)
        {
            TextDecomposer ret = new TextDecomposer();

            ret.MaxVolumeToProcess    = 5000;
            ret.PaddingCharacterCount = 20;
            ret.JoinCharacterCount    = 30; // longest English word is 45 letters 20+30=50

            ret.StandardAgent        = new TextDecompositionAgent(openAI_APIKey);
            ret.HighPerformanceAgent = new TextDecompositionAgent(openAI_APIKey/*, highPowered: true*/);
            ret.FallbackAgent        = new WhitespaceDecompositionAgent();

            return ret;
        }
    }
}
