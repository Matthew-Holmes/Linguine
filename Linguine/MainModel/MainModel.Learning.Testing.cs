using Config;
using DataClasses;
using Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    // data classes for UI
    public record WordInContext(string StatementText, int WordStart, int Len);
    public record DefinitionForTesting(
        String Prompt,
        String CorrectAnswer,
        List<WordInContext> Contexts,
        DictionaryDefinition Parent,
        String? SoundFileName);


    // TODO - un-extract ToTestFromKey
    partial class MainModel
    {
        #region get definition methods
        public DefinitionForTesting GetHighLearningDefinition()
        {
            int defId = DefLearningService.GetHighLearningDefinitionID();

            if (lastNcorrect > 10) { defId = DefLearningService.GetFrequentDefinition(1); } // edge case not giving amything hard
            
            DictionaryDefinition toTest = ToTestFromKey(defId);

            DefinitionForTesting forTesting = AsDefinitionForTesting(toTest);

            if (forTesting.Contexts.Count == 0)
            {
                // edge case where user knows every word every seen
                Log.Warning("had to revert to frequent definition, since no contexts");
                return GetRandomDefinition(freq: 1);
            }
            else
            {
                return forTesting;
            }
        }

        public DefinitionForTesting GetRandomDefinition(int freq = 5)
        {
            int toTestKey = DefLearningService.GetFrequentDefinition(freq);

            DictionaryDefinition toTest = ToTestFromKey(toTestKey);

            return AsDefinitionForTesting(toTest);
        }


        public DefinitionForTesting GetHighInformationDefinition()
        {
            int toTestKey = DefLearningService.GetInitialVocabEstimationDefinition();

            DictionaryDefinition toTest = ToTestFromKey(toTestKey);

            return AsDefinitionForTesting(toTest);

        }
        #endregion

        #region UI data wrappers

        // TODO - could this be a service?
        public DefinitionForTesting AsDefinitionForTesting(DictionaryDefinition def)
        {
            List<Statement> uses = SM.Managers!.Statements.GetNStatementsFor(def, MaxContextExamples);

            List<WordInContext> contexts = uses.Where(use => use is not null).Select(use => StatementHelper.AsWordInContext(def, use)).ToList();

            contexts = contexts.Where(c => c is not null).ToList();

            String? defText = null;

            List<VocalisedDefinitionFile> soundFiles = SM.Managers!.Vocalisations.GetAudioFilesForDefinition(def.DatabasePrimaryKey);

            String? soundFile = null;

            if (soundFiles.Count != 0)
            {
                soundFile = soundFiles[_rng.Next(soundFiles.Count)].FileName;

                soundFile = Path.Combine(ConfigManager.Config.AudioStoreDirectory, soundFile);
            }

            if (soundFiles.Count < Enum.GetNames(typeof(Voice)).Length)
            {
                var voices = Enum.GetValues(typeof(Voice));

                HashSet<Voice> heard = soundFiles.Select(
                    sf => sf.Voice).ToHashSet();

                List<Voice> unheard = new List<Voice>();

                foreach (Voice voice in voices)
                {
                    if (heard.Contains(voice))
                    {
                        continue;
                    }
                    else
                    {
                        unheard.Add(voice);
                    }
                }

                if (unheard.Count != 0)
                {
                    Voice newVoice = unheard[_rng.Next(unheard.Count)];

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await VocaliseDefinition(def, newVoice);

                            // uncomment to tidy - TODO - add a button somewhere??
                            //await DefinitionVocalisationManager.CleanupMissingFilesAsync();

                            // go off in the background and get another sound file
                            // since we probably care about this definition
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Exception in background vocalisation task for definition {def.Word}: {ex.Message}");
                        }
                    });
                }
                else
                {
                    Log.Information("something weird happened");
                }
            }

            if (ConfigManager.Config.LearningForeignLanguage())
            {
                // TODO - should these be implicit in the parsed dictionary definition manager?
                ParsedDictionaryDefinition? pdef = SM.Managers.ParsedDefinitions.GetParsedDictionaryDefinition(
                    def, ConfigManager.Config.GetLearnerLevel(), ConfigManager.Config.Languages.NativeLanguage);

                // TODO - once have multiple parsing levels - use a fallback to neares parsed level if the correct one is not available
                // but then spin off a thread to go do the parsing at the right level for future uses

                if (pdef is not null)
                {
                    defText = pdef.ParsedDefinition;
                }
            }

            if (defText is null)
            {
                defText = def.Definition;
            }

            if (def.RomanisedPronuncation is not null)
            {
                defText = def.RomanisedPronuncation + '\n' + defText;
            }

            return new DefinitionForTesting(def.Word, defText, contexts, def, soundFile);
        }


        private DictionaryDefinition ToTestFromKey(int toTestKey)
        {
            DictionaryDefinition? maybeToTest = SM.Managers!.Definitions.TryGetDefinitionByKey(toTestKey);

            DictionaryDefinition toTest;

            if (maybeToTest is null)
            {
                Log.Warning("couldn't get the definition {defKey} requested - testing user on random one instead!", toTestKey);
                toTest = SM.Managers.Definitions.GetRandomDefinition();
            }
            else
            {
                toTest = maybeToTest;
            }

            return toTest;

        }

        #endregion

        #region recording test data

        private int lastNcorrect = 0;
        internal void RecordTest(DefinitionForTesting definitionForTesting,
                                 DateTime posed, DateTime answered, DateTime finished,
                                 bool correct)
        {
            using var context = LinguineFactory.CreateDbContext();

            TestRecord added = SM.Managers!.TestRecords.AddRecord(definitionForTesting.Parent, posed, answered, finished, correct, context);

            if (added.Correct) { lastNcorrect++; } else { lastNcorrect = 0; }

            DefLearningService.Inform(added);
        }
        #endregion
    }
}
