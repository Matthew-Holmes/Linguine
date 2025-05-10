using DataClasses;
using Infrastructure.AntiCorruptionLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class LingineReadonlyDbContext
    {
        private LinguineDbContext _dbContext;

        internal LingineReadonlyDbContext(LinguineDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<DictionaryDefinition> DictionaryDefinitions => _dbContext.DictionaryDefinitions.AsNoTracking();
        public IQueryable<VariantRoot> Variants => _dbContext.Variants.AsNoTracking();
        public IQueryable<TextualMedia> TextualMedia => _dbContext.TextualMedia.AsNoTracking();
        public IQueryable<TextualMediaSession> TextualMediaSessions => _dbContext.TextualMediaSessions.AsNoTracking();
        public IQueryable<StatementDatabaseEntry> Statements => _dbContext.Statements.AsNoTracking();
        public IQueryable<StatementDefinitionNode> StatementDefinitions => _dbContext.StatementDefinitions.AsNoTracking();
        public IQueryable<ParsedDictionaryDefinition> ParsedDictionaryDefinitions => _dbContext.ParsedDictionaryDefinitions.AsNoTracking();
        public IQueryable<TestRecord> TestRecords => _dbContext.TestRecords.AsNoTracking();
        public IQueryable<VocalisedDefinitionFile> VocalisedDefinitionFiles => _dbContext.VocalisedDefinitionFiles.AsNoTracking();
        public IQueryable<TranslatedStatementDatabaseEntry> TranslatedStatements => _dbContext.TranslatedStatements.AsNoTracking();

    }
}
