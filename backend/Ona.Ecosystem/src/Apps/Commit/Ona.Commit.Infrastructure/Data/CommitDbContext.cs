using Microsoft.EntityFrameworkCore;
using Ona.Commit.Application.Interfaces;
using Ona.Commit.Domain.Entities;
using Ona.Domain.Shared.Interfaces;
using System.Linq.Expressions;

namespace Ona.Commit.Infrastructure.Data
{
    public class CommitDbContext(DbContextOptions<CommitDbContext> options, ITenantProvider _tenantProvider, IUserProvider _userProvider) : DbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureTablesNames(modelBuilder);
            ConfigureCustomerEntity(modelBuilder);
            ConfigureAppointmentEntity(modelBuilder);
            ConfigureQueryFilters(modelBuilder);
        }

        private static void ConfigureTablesNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
        }

        private static void ConfigureCustomerEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
            });
        }

        private static void ConfigureAppointmentEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.Id);
            });
        }

        private void ConfigureQueryFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                    var tenantId = Expression.Constant(_tenantProvider.TenantId);
                    var body = Expression.Equal(property, tenantId);
                    var lambda = Expression.Lambda(body, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IUserEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(IUserEntity.UserId));
                    var userId = Expression.Constant(_userProvider.UserId);
                    var body = Expression.Equal(property, userId);
                    var lambda = Expression.Lambda(body, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            modelBuilder.Entity<Appointment>()
                .HasQueryFilter(a => a.TenantId == _tenantProvider.TenantId);

            modelBuilder.Entity<Customer>()
                .HasQueryFilter(c => c.TenantId == _tenantProvider.TenantId);

            modelBuilder.Entity<Appointment>()
                .HasQueryFilter(a => a.UserId == _userProvider.UserId);

            modelBuilder.Entity<Customer>()
                .HasQueryFilter(c => c.UserId == _userProvider.UserId);
        }
    }
}
