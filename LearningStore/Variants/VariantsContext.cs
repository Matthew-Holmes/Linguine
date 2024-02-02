using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningStore
{
    public class VariantsContext : DbContext
    {
        public DbSet<VariantRoot> Variants { get; set; }
        private readonly String _connectionString;

        public VariantsContext(String ConnectionString)
        {
            _connectionString = ConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VariantRoot>()
                .HasKey(e => e.DatabasePrimaryKey)
                .HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);
        }
    }
}
