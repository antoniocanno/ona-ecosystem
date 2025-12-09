using Microsoft.EntityFrameworkCore;
using Ona.Quote.Domain.Entities;

namespace Ona.Quote.Infrastructure.Data
{
    public class QuoteDbContext(DbContextOptions<QuoteDbContext> options) : DbContext(options)
    {
        public DbSet<Domain.Entities.Quote> Quotes { get; set; }
        public DbSet<QuoteLine> QuoteLines { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureTablesNames(modelBuilder);
            ConfigureQuoteEntity(modelBuilder);
            ConfigureQuoteLineEntity(modelBuilder);
            ConfigureClientEntity(modelBuilder);
            ConfigureProductEntity(modelBuilder);
        }

        private static void ConfigureTablesNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Entities.Quote>().ToTable("Quotes");
            modelBuilder.Entity<QuoteLine>().ToTable("QuoteLines");
            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<Product>().ToTable("Products");
        }

        private static void ConfigureQuoteEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Entities.Quote>(entity =>
            {
                entity.HasKey(q => q.Id);

                entity.HasMany(q => q.Lines)
                    .WithOne(q => q.Quote)
                    .HasForeignKey(q => q.QuoteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureQuoteLineEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QuoteLine>(entity =>
            {
                entity.HasKey(q => q.Id);

                entity.HasOne(q => q.Quote)
                    .WithMany(q => q.Lines)
                    .HasForeignKey(q => q.QuoteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(q => q.Description)
                    .IsRequired();

                entity.Property(q => q.Quantity)
                    .IsRequired();

                entity.Property(q => q.UnitPrice)
                    .IsRequired();
            });
        }

        private static void ConfigureClientEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                    .IsRequired();
            });
        }

        private static void ConfigureProductEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Description)
                    .IsRequired();
            });
        }
    }
}
