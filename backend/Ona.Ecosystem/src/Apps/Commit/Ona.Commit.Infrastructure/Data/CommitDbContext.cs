using Microsoft.EntityFrameworkCore;
using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;
using Ona.Infrastructure.Shared.Data;

namespace Ona.Commit.Infrastructure.Data
{
    public class CommitDbContext : DbContext
    {
        private readonly ICurrentTenant? _currentTenant;

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<CalendarIntegration> CalendarIntegrations { get; set; }
        public DbSet<ExternalCalendarEventMapping> ExternalCalendarEventMappings { get; set; }

        public CommitDbContext() : base() { }

        public CommitDbContext(DbContextOptions<CommitDbContext> options, ICurrentTenant currentTenant) : base(options)
        {
            _currentTenant = currentTenant;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureTablesNames(modelBuilder);
            ConfigureCustomerEntity(modelBuilder);
            ConfigureAppointmentEntity(modelBuilder);
            ConfigureNotificationLogEntity(modelBuilder);

            modelBuilder.ApplyTenantFilters(_currentTenant);
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
    }
}
