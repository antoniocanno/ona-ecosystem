using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ona.Auth.Domain.Entities;
using Ona.Core.Entities;
using Ona.Core.Interfaces;
using Ona.Infrastructure.Shared.Data;

namespace Ona.Auth.Infrastructure.Data
{
    public class AuthDbContext : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        UserTenantRole,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
    {
        private readonly ICurrentTenant? _currentTenant;

        public AuthDbContext() : base() { }

        public AuthDbContext(DbContextOptions<AuthDbContext> options, ICurrentTenant currentTenant) : base(options)
        {
            _currentTenant = currentTenant;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<IdentityUserRole<Guid>>();
            modelBuilder.Ignore<IdentityRole<Guid>>();

            base.OnModelCreating(modelBuilder);

            ConfigureTablesNames(modelBuilder);
            ConfigureUserEntity(modelBuilder);
            ConfigureEmailVerificationTokenEntity(modelBuilder);
            ConfigurePasswordResetTokenEntity(modelBuilder);
            ConfigureRefreshTokenEntity(modelBuilder);
            ConfigureUnlockUserTokenEntity(modelBuilder);
            ConfigureUnlockUserTokenEntity(modelBuilder);
            ConfigureUserTenantRole(modelBuilder);
            ConfigureTenantInviteEntity(modelBuilder);
            ConfigureTenantEntity(modelBuilder);

            modelBuilder.ApplyTenantFilters(_currentTenant);
            SetTenantQueryFilter(modelBuilder);
        }

        private static void ConfigureTenantEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name).IsRequired();
                entity.Property(t => t.Domain).IsRequired();
                entity.Property(t => t.TimeZone).IsRequired().HasDefaultValue("America/Sao_Paulo");
            });
        }

        private static void ConfigureTablesNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
            modelBuilder.Entity<UserTenantRole>().ToTable("UserRoles");
            modelBuilder.Entity<Tenant>().ToTable("Tenants");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            modelBuilder.Entity<EmailVerificationToken>().ToTable("EmailVerificationTokens");
            modelBuilder.Entity<PasswordResetToken>().ToTable("PasswordResetTokens");
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
            modelBuilder.Entity<PasswordResetToken>().ToTable("PasswordResetTokens");
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
            modelBuilder.Entity<UnlockUserToken>().ToTable("UnlockUserTokens");
            modelBuilder.Entity<TenantInvite>().ToTable("TenantInvites");
        }

        private static void ConfigureUserEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName).IsRequired();
                entity.Property(u => u.CreatedAt).IsRequired();
                entity.Property(u => u.UpdatedAt).IsRequired();
            });
        }

        private static void ConfigureEmailVerificationTokenEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailVerificationToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);

                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.HasIndex(rt => rt.UserId);
                entity.HasIndex(rt => rt.ExpiresAt);
                entity.HasIndex(rt => rt.IsRevoked);

                entity.Property(rt => rt.IsRevoked)
                    .HasDefaultValue(false);

                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .HasPrincipalKey(u => u.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureRefreshTokenEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);

                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.HasIndex(rt => rt.UserId);
                entity.HasIndex(rt => rt.ExpiresAt);
                entity.HasIndex(rt => rt.IsRevoked);

                entity.Property(rt => rt.IsRevoked)
                    .HasDefaultValue(false);

                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .HasPrincipalKey(u => u.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigurePasswordResetTokenEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);

                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.HasIndex(rt => rt.UserId);
                entity.HasIndex(rt => rt.ExpiresAt);
                entity.HasIndex(rt => rt.IsRevoked);

                entity.Property(rt => rt.IsRevoked)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .HasPrincipalKey(u => u.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureUnlockUserTokenEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UnlockUserToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);

                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.HasIndex(rt => rt.UserId);
                entity.HasIndex(rt => rt.ExpiresAt);
                entity.HasIndex(rt => rt.IsRevoked);

                entity.Property(rt => rt.IsRevoked)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .HasPrincipalKey(u => u.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureUserTenantRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTenantRole>(b =>
            {
                b.HasKey(tr => new { tr.UserId, tr.RoleId, tr.TenantId });
            });
        }

        private static void ConfigureTenantInviteEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TenantInvite>(entity =>
            {
                entity.HasKey(ti => ti.Id);

                entity.HasIndex(ti => ti.Token).IsUnique();
                entity.HasIndex(ti => ti.Email);
                entity.HasIndex(ti => ti.TenantId);

                entity.Property(ti => ti.Email).IsRequired();
                entity.Property(ti => ti.Role).IsRequired();
                entity.Property(ti => ti.Token).IsRequired();
                entity.Property(ti => ti.ExpiresAt).IsRequired();
                entity.Property(ti => ti.IsConsumed).HasDefaultValue(false);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.SetTenantId(_currentTenant);
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetTenantQueryFilter(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>().HasQueryFilter(e =>
                (_currentTenant == null ||
                !_currentTenant.Id.HasValue ||
                e.Id == _currentTenant.Id.GetValueOrDefault()) &&
                !e.IsDeleted);
        }
    }
}
