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
    public class LinguineReadonlyDbContext : IDisposable
    {
        private LinguineDbContext _dbContext;

        internal LinguineReadonlyDbContext(LinguineDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<DictionaryDefinition> DictionaryDefinitions => _dbContext.DictionaryDefinitions.AsNoTracking();
        public IQueryable<VariantRoot> Variants => _dbContext.Variants.AsNoTracking();
        public IQueryable<TextualMedia> TextualMedia => _dbContext.TextualMedia.AsNoTracking();
        public IQueryable<TextualMediaSession> TextualMediaSessions => _dbContext.TextualMediaSessions.AsNoTracking();
        // TODO - maybe track the ones with text so the updates stream live to the user??
        public IQueryable<TextualMediaSession> TextualMediaSessionsWithText => _dbContext.TextualMediaSessions.Include(s => s.TextualMedia).AsNoTracking();
        public IQueryable<StatementDatabaseEntry> Statements => _dbContext.Statements.AsNoTracking();
        public IQueryable<StatementDefinitionNode> StatementDefinitions => _dbContext.StatementDefinitions.AsNoTracking();
        public IQueryable<ParsedDictionaryDefinition> ParsedDictionaryDefinitions => _dbContext.ParsedDictionaryDefinitions.AsNoTracking();
        public IQueryable<TestRecord> TestRecords => _dbContext.TestRecords.AsNoTracking();
        public IQueryable<VocalisedDefinitionFile> VocalisedDefinitionFiles => _dbContext.VocalisedDefinitionFiles.AsNoTracking();
        public IQueryable<TranslatedStatementDatabaseEntry> TranslatedStatements => _dbContext.TranslatedStatements.AsNoTracking();

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
