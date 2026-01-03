using Microsoft.EntityFrameworkCore;
using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;
using Ona.Infrastructure.Shared.Data;

namespace Ona.Commit.Infrastructure.Data
{
    public class CommitDbContext : DbContext
    {
        private readonly ICurrentTenant? _currentTenant;
        private readonly ICurrentUser? _currentUser;

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<CalendarIntegration> CalendarIntegrations { get; set; } = null!;
        public DbSet<ExternalCalendarEventMapping> ExternalCalendarEventMappings { get; set; } = null!;
        public DbSet<TenantSettings> TenantSettings { get; set; } = null!;

        public CommitDbContext() : base() { }

        public CommitDbContext(DbContextOptions<CommitDbContext> options, ICurrentTenant currentTenant, ICurrentUser currentUser) : base(options)
        {
            _currentTenant = currentTenant;
            _currentUser = currentUser;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureTablesNames(modelBuilder);
            ConfigureCustomerEntity(modelBuilder);
            ConfigureAppointmentEntity(modelBuilder);
            ConfigureNotificationLogEntity(modelBuilder);
            ConfigureCalendarIntegrationEntity(modelBuilder);
            ConfigureExternalCalendarEventMappingEntity(modelBuilder);
            ConfigureMessageTemplateEntity(modelBuilder);
            ConfigureTenantSettingsEntity(modelBuilder);

            modelBuilder.ApplyTenantFilters(_currentTenant);
        }

        private static void ConfigureTablesNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
            modelBuilder.Entity<NotificationLog>().ToTable("NotificationLogs");
            modelBuilder.Entity<MessageTemplate>().ToTable("MessageTemplates");
            modelBuilder.Entity<TenantSettings>().ToTable("TenantSettings");
            modelBuilder.Entity<CalendarIntegration>().ToTable("CalendarIntegrations");
            modelBuilder.Entity<ExternalCalendarEventMapping>().ToTable("ExternalCalendarEventMappings");
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

        private static void ConfigureCalendarIntegrationEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalendarIntegration>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => new { c.TenantId, c.CustomerId });
            });
        }

        private static void ConfigureExternalCalendarEventMappingEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExternalCalendarEventMapping>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.AppointmentId });
            });
        }

        private static void ConfigureMessageTemplateEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageTemplate>(entity =>
            {
                entity.HasKey(m => m.Id);
            });
        }

        private static void ConfigureTenantSettingsEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TenantSettings>(entity =>
            {
                entity.HasKey(t => t.Id);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.SetTenantId(_currentTenant);
            this.SetUserId(_currentUser);
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
