using Microsoft.EntityFrameworkCore;
using Ona.Commit.Application.Interfaces.Provider;
using Ona.Commit.Domain.Entities;
using Ona.Domain.Shared.Interfaces;
using System.Linq.Expressions;

namespace Ona.Commit.Infrastructure.Data
{
    public class CommitDbContext(DbContextOptions<CommitDbContext> options, ICurrentTenant currentTenant) : DbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureTablesNames(modelBuilder);
            ConfigureCustomerEntity(modelBuilder);
            ConfigureAppointmentEntity(modelBuilder);
            ConfigureNotificationLogEntity(modelBuilder);

            //ConfigureQueryFilters(modelBuilder);
        }

        private static void ConfigureTablesNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
            modelBuilder.Entity<NotificationLog>().ToTable("NotificationLogs");
            modelBuilder.Entity<MessageTemplate>().ToTable("MessageTemplates");
            modelBuilder.Entity<TenantSettings>().ToTable("TenantSettings");
        }

        private static void ConfigureCustomerEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => new { c.TenantId, c.PhoneNumber }).IsUnique();
            });
        }

        private static void ConfigureAppointmentEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => new { a.TenantId, a.StartDate });
            });
        }

        private static void ConfigureNotificationLogEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.HasIndex(n => n.ExternalMessageId);
            });
        }

        //private void ConfigureQueryFilters(ModelBuilder modelBuilder)
        //{
        //    var a = currentTenant.Id == tenantProvider.TenantId;
        //    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        //    {
        //        if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
        //        {
        //            var parameter = Expression.Parameter(entityType.ClrType, "e");
        //            var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
        //            var tenantId = Expression.Constant(tenantProvider.TenantId);
        //            var body = Expression.Equal(property, tenantId);
        //            var lambda = Expression.Lambda(body, parameter);

        //            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        //        }
        //    }

        //    modelBuilder.Entity<Appointment>()
        //        .HasQueryFilter(a => a.TenantId == tenantProvider.TenantId);

        //    modelBuilder.Entity<Customer>()
        //        .HasQueryFilter(c => c.TenantId == tenantProvider.TenantId);
        //}
    }
}
