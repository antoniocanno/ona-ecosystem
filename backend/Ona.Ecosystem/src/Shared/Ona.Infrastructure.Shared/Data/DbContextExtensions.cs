using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Infrastructure.Shared.Data
{
    public static class DbContextExtensions
    {
        public static void ApplyTenantFilters(this ModelBuilder modelBuilder, ICurrentTenant? currentTenant)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.SetTenantQueryFilter(entityType.ClrType, currentTenant);
                }
            }
        }

        private static void SetTenantQueryFilter(this ModelBuilder modelBuilder, Type entityType, ICurrentTenant? currentTenant)
        {
            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));

            // Expression: e => e.TenantId == (currentTenant.Id ?? Guid.Empty)
            var tenantIdExpression = Expression.Constant(currentTenant?.Id ?? Guid.Empty);
            var body = Expression.Equal(property, tenantIdExpression);
            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(entityType).HasQueryFilter(lambda);
        }

        public static void SetTenantId(this DbContext context, ICurrentTenant? currentTenant)
        {
            var tenantId = currentTenant?.Id;
            if (!tenantId.HasValue) return;

            foreach (var entry in context.ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = tenantId.Value;
                }
            }
        }
    }
}
