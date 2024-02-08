using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Agents
{
    public class RootGeneratorAgent : OpenAIBase
    {
        //private static String preamble = "You are a dictionary helper agent, generating roots of words/phrases while PRESERVING meaning while looking up the original. Multiword phrases/names can have multiword roots. Generate the root of the text marked \"Original:\", in the linguistic sense. For example return infinitives of verbs or singular versions of nouns, preserving uninflected meaning. Use the context only to resolve ambiguity, DO NOT return other text from it. favour lowercase if possible. Reply ONLY with the root of the text marked \"Original:\"";
        private static String preamble = "You are a lexicographer, to avoid redundant dictionary entries, produce a root of the following word/phrase marked \"Original:\". Remove inflection. The context is ONLY for resolving ambiguity DO NOT return other words from the context. Multiword phrases MUST NOT be cut short if it alters their uninflected meaning. Any \"Original\" text containing Proper nouns, place names and peoples'/characters' names MUST conform to initial capitalisation rules. Reply ONLY with the root, NO other information";
        //private static String preamble = "Find the basic form of the given word or phrase listed after 'Original:'. Only remove changes made for grammar reasons, like tense or number.Use the provided context only to clear up what the word means, but don't include any other words from it. If the phrase has more than one word, keep it complete unless it changes its simplest meaning. For any 'Original' words that are names of people, places, or characters, make sure the first letter of each name is uppercase. Only respond with the basic form of the word or phrase, nothing else.";
        public RootGeneratorAgent(string apiKey) : base(apiKey, preamble, 0, 4096, "gpt-4-0125-preview") { }

    }
}
