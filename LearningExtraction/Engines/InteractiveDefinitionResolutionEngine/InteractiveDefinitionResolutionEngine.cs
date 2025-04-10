using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public interface ICanResolveDefinitions 
    {
        public bool ResolveFromExisting(Statement stat, int defIndex, DictionaryDefinition def);
        public bool ResolveFromNew(     Statement stat, int defIndex, DictionaryDefinition newDef);


        public Task<StatementTranslation>        GetTranslation(Statement stat);
        public Task<InitialDefinitionAnalyis>    GetInitialAnalysis(   Statement stat, int defIndex);
        public Task<DictionaryDefinition>        GenerateNewDefinition(Statement stat, int defIndex);
    }


    class InteractiveDefinitionResolutionEngine : ICanResolveDefinitions
    {
        public Task<DictionaryDefinition> GenerateNewDefinition(Statement stat, int defIndex)
        {
            throw new NotImplementedException();
        }

        public Task<InitialDefinitionAnalyis> GetInitialAnalysis(Statement stat, int defIndex)
        {
            throw new NotImplementedException();
        }

        public Task<StatementTranslation> GetTranslation(Statement stat)
        {
            throw new NotImplementedException();
        }

        public bool ResolveFromExisting(Statement stat, int defIndex, DictionaryDefinition def)
        {
            throw new NotImplementedException();
        }

        public bool ResolveFromNew(Statement stat, int defIndex, DictionaryDefinition newDef)
        {
            throw new NotImplementedException();
        }
    }
}
