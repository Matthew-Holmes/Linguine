using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    // one context per target language should be replaced on config change
    public class LinguineDataHandler : DbContext
    {
        private readonly String _connectionString;

        // Tables
        public DbSet<DictionaryDefinition> DictionaryDefinitions { get; set; }
        public DbSet<VariantRoot> Variants { get; set; }
        public DbSet<TextualMedia> TextualMedia { get; set; }
        public DbSet<TextualMediaSession> TextualMediaSessions { get; set; }


        public LinguineDataHandler(String connectionString)
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
                .HasKey(e => e.DatabasePrimaryKey)
                .HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);

            modelBuilder.Entity<VariantRoot>()
                .HasKey(e => e.DatabasePrimaryKey)
                .HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);

            modelBuilder.Entity<TextualMedia>()
                .HasKey(e => e.DatabasePrimaryKey)
                .HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);

            modelBuilder.Entity<TextualMediaSession>()
                .HasKey(e => e.DatabasePrimaryKey)
                .HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);

            // Other configurations...
        }
    }
}
