using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;
using DataClasses;

namespace Infrastructure
{
    // one context per target language should be replaced on config change
    public class LinguineDbContext : DbContext
    {
        #region custom dispose with logging
        private bool _disposed = false;

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Log.Verbose("Context Disposed. StackTrace: {StackTrace}", GetFilteredStackTrace());
                }
                _disposed = true;
            }
            base.Dispose();
        }
        private string GetFilteredStackTrace()
        {
            StackTrace stackTrace = new StackTrace(true); // true = include file info
            return string.Join("\n", stackTrace.GetFrames()
                .Where(f => f.GetMethod().DeclaringType != null &&
                            f.GetMethod().DeclaringType.Namespace != null &&
                            !f.GetMethod().DeclaringType.Namespace.StartsWith("System") &&
                            !f.GetMethod().DeclaringType.Namespace.StartsWith("MS"))
                .Select(f => $"{f.GetMethod().DeclaringType}.{f.GetMethod().Name} ({f.GetFileName()}:{f.GetFileLineNumber()})"));
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~LinguineDbContext()
        {
            Dispose(false);
        }
        #endregion
        private readonly String _connectionString;

        // Tables
        public DbSet<DictionaryDefinition> DictionaryDefinitions { get; set; }
        public DbSet<VariantRoot> Variants { get; set; }
        public DbSet<TextualMedia> TextualMedia { get; set; }
        public DbSet<TextualMediaSession> TextualMediaSessions { get; set; }

        public DbSet<StatementDatabaseEntry> Statements { get; set; }
        public DbSet<StatementDefinitionNode> StatementDefinitions { get; set; }

        public DbSet<ParsedDictionaryDefinition> ParsedDictionaryDefinitions { get; set; }
        public DbSet<TestRecord> TestRecords { get; set; }

        public DbSet<VocalisedDefinitionFile> VocalisedDefinitionFiles { get; set; }

        public LinguineDbContext()
        {
            _connectionString = "dummy_for_migrations";
        }

        public LinguineDbContext(String connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DictionaryDefinition>()
                .HasKey(e => e.DatabasePrimaryKey);
            modelBuilder.Entity<DictionaryDefinition>()
                .Property(e => e.DatabasePrimaryKey)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<VariantRoot>()
                .HasKey(e => e.DatabasePrimaryKey);
            modelBuilder.Entity<VariantRoot>()
                .Property(e => e.DatabasePrimaryKey)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TextualMedia>()
                .HasKey(e => e.DatabasePrimaryKey);
            modelBuilder.Entity<TextualMedia>()
                .Property(e => e.DatabasePrimaryKey)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TextualMediaSession>()
                .HasKey(e => e.DatabasePrimaryKey);
            modelBuilder.Entity<TextualMediaSession>()
                .Property(e => e.DatabasePrimaryKey)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<TextualMediaSession>()
                .HasOne(e => e.TextualMedia)
                .WithMany()
                .HasForeignKey(e => e.TextualMediaKey);

            modelBuilder.Entity<StatementDatabaseEntry>()
                .HasKey(e => e.DatabasePrimaryKey);
            modelBuilder.Entity<StatementDatabaseEntry>()
                .HasOne(e => e.Parent)
                .WithMany()
                .HasForeignKey(e => e.ParentKey);
            modelBuilder.Entity<StatementDatabaseEntry>()
                .HasOne(e => e.Previous)
                .WithOne()
                .HasForeignKey<StatementDatabaseEntry>(e => e.PreviousKey);
            modelBuilder.Entity<StatementDatabaseEntry>()
                .Property(e => e.ContextDeltaInsertionsDescendingIndex)
                .HasConversion(new InsertionsJSONConverter());
            modelBuilder.Entity<StatementDatabaseEntry>()
                .Property(e => e.ContextDeltaRemovalsDescendingIndex)
                .HasConversion(new RemovalsJSONConverter());
            modelBuilder.Entity<StatementDatabaseEntry>()
                .Property(e => e.ContextCheckpoint)
                .HasConversion(new ContextJSONConverter());

            modelBuilder.Entity<StatementDefinitionNode>()
                .HasKey(e => e.DatabasePrimaryKey);
            modelBuilder.Entity<StatementDefinitionNode>()
                .HasOne(e => e.StatementDatabaseEntry)
                .WithMany()
                .HasForeignKey(e => e.StatementKey);
            modelBuilder.Entity<StatementDefinitionNode>()
                .HasOne(e => e.DictionaryDefinition)
                .WithMany()
                .HasForeignKey(e => e.DefinitionKey);

            modelBuilder.Entity<ParsedDictionaryDefinition>()
                .HasKey(e => e.DatabasePrimaryKey);
            modelBuilder.Entity<ParsedDictionaryDefinition>()
                .Property(e => e.DatabasePrimaryKey)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<ParsedDictionaryDefinition>()
                .HasOne(e => e.CoreDefinition)
                .WithMany()
                .HasForeignKey(e => e.DictionaryDefinitionKey);

            modelBuilder.Entity<TestRecord>()
                .HasOne(e => e.Definition)
                .WithMany()
                .HasForeignKey(e => e.DictionaryDefinitionKey);

            modelBuilder.Entity<VocalisedDefinitionFile>()
                .HasOne(e => e.Definition)
                .WithMany()
                .HasForeignKey(e => e.DictionaryDefinitionKey);

            // Other configurations...
        }
    }
}
