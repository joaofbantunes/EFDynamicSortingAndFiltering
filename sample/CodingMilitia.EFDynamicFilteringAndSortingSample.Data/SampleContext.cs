using System.ComponentModel.DataAnnotations.Schema;
using CodingMilitia.EFDynamicFilteringAndSortingSample.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace CodingMilitia.EFDynamicFilteringAndSortingSample.Data
{
    public class SampleContext : DbContext
    {
        public DbSet<SampleEntity> SampleEntities { get; set; }

        public SampleContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<SampleEntity>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<SampleEntity>().Property(e => e.Id)
                .UseNpgsqlSerialColumn();
        }
    }
}