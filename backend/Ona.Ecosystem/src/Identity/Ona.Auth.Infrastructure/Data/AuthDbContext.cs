using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Infrastructure.Data
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureTablesNames(modelBuilder);
            ConfigureUserEntity(modelBuilder);
            ConfigureEmailVerificationTokenEntity(modelBuilder);
            ConfigurePasswordResetTokenEntity(modelBuilder);
            ConfigureRefreshTokenEntity(modelBuilder);
            ConfigureUnlockUserTokenEntity(modelBuilder);
        }

        private static void ConfigureTablesNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            modelBuilder.Entity<EmailVerificationToken>().ToTable("EmailVerificationTokens");
            modelBuilder.Entity<PasswordResetToken>().ToTable("PasswordResetTokens");
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
            modelBuilder.Entity<UnlockUserToken>().ToTable("UnlockUserTokens");
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
    }
}
