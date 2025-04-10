using Config;
using DataClasses;
using Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public interface ICanResolveDefinitions 
    {
        public bool Resolve(Statement stat, int defIndex, DictionaryDefinition def);

        
        public Task<StatementTranslation?>        GetTranslation(Statement stat);
        public Task<InitialDefinitionAnalyis>     GetInitialAnalysis(   Statement stat, int defIndex);
        public Task<DictionaryDefinition>         GenerateNewDefinition(Statement stat, int defIndex);
    }


    public class InteractiveDefinitionResolutionEngine : ICanResolveDefinitions
    {
        public Task<DictionaryDefinition> GenerateNewDefinition(Statement stat, int defIndex)
        {
            throw new NotImplementedException();
        }

        public Task<InitialDefinitionAnalyis> GetInitialAnalysis(Statement stat, int defIndex)
        {
            throw new NotImplementedException();
        }

        public async Task<StatementTranslation?> GetTranslation(Statement stat)
        {
            if (!ConfigManager.Config.LearningForeignLanguage())
            {
                throw new Exception("why is translation happening when in native language!");
            }

            LanguageCode native = ConfigManager.Config.Languages.NativeLanguage;
            LanguageCode target = ConfigManager.Config.Languages.TargetLanguage;



            if (stat.StatementText == "")
            {
                Log.Warning("translating empty statement");
                return new StatementTranslation 
                { 
                    TranslatedLanguage = native, 
                    Translation        = "", 
                    Underlying         = stat };
            }

            String translation = await Translator.Translate(stat.StatementText, target, native);

            if (translation is null)
            {
                Log.Error("translation failed");
                return null;
            }

            return new StatementTranslation 
            { 
                TranslatedLanguage = native, 
                Translation        = translation, 
                Underlying         = stat 
            };
        }

        public bool Resolve(Statement stat, int defIndex, DictionaryDefinition def)
        {
            throw new NotImplementedException();
        }
    }
}
