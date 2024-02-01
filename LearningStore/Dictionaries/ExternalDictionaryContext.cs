using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningStore
{
    public class ExternalDictionaryContext : DbContext
    {
        public DbSet<DictionaryDefinition> DictionaryDefinitions { get; set; }
        private readonly string _connectionString;

        public ExternalDictionaryContext(string connectionString)
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

            // Other configurations...
        }
    }
}

