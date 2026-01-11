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
        public DbSet<Professional> Professionals { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<CalendarIntegration> CalendarIntegrations { get; set; } = null!;
        public DbSet<ExternalCalendarEventMapping> ExternalCalendarEventMappings { get; set; } = null!;

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
            ConfigureProfessionalEntity(modelBuilder);
            ConfigureAppointmentEntity(modelBuilder);
            ConfigureNotificationLogEntity(modelBuilder);
            ConfigureCalendarIntegrationEntity(modelBuilder);
            ConfigureExternalCalendarEventMappingEntity(modelBuilder);
            ConfigureMessageTemplateEntity(modelBuilder);

            modelBuilder.ApplyTenantFilters(_currentTenant);
        }

        private static void ConfigureTablesNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Professional>().ToTable("Professionals");
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
            modelBuilder.Entity<NotificationLog>().ToTable("NotificationLogs");
            modelBuilder.Entity<MessageTemplate>().ToTable("MessageTemplates");
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

        private static void ConfigureProfessionalEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Professional>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => new { p.TenantId, p.Email }).IsUnique();
            });
        }

        private static void ConfigureAppointmentEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => new { a.TenantId, a.StartDate });
                entity.HasOne(a => a.Professional)
                    .WithMany()
                    .HasForeignKey(a => a.ProfessionalId)
                    .OnDelete(DeleteBehavior.Restrict);
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
                entity.HasIndex(c => new { c.TenantId, c.ProfessionalId });
                entity.HasIndex(c => new { c.ExternalResourceId, c.Provider, c.IsActive });
                entity.HasOne(c => c.Professional)
                    .WithMany()
                    .HasForeignKey(c => c.ProfessionalId)
                    .OnDelete(DeleteBehavior.Restrict);
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

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.SetTenantId(_currentTenant);
            this.SetUserId(_currentUser);
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
