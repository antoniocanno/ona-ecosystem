using Microsoft.EntityFrameworkCore;
using Ona.Core.Interfaces;
using System.Linq.Expressions;

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
            if (currentTenant == null) return;

            var parameter = Expression.Parameter(entityType, "e");
            var tenantIdProperty = Expression.Property(parameter, nameof(ITenantEntity.TenantId));

            // Referência dinâmica para a instância do currentTenant
            var currentTenantConst = Expression.Constant(currentTenant);
            var currentTenantIdProperty = Expression.Property(currentTenantConst, nameof(ICurrentTenant.Id));

            // !currentTenant.Id.HasValue (Se não houver tenant no contexto, permite tudo)
            var hasValueProperty = Expression.Property(currentTenantIdProperty, "HasValue");
            var hasNoValue = Expression.Not(hasValueProperty);

            // e.TenantId == currentTenant.Id.Value
            var tenantIdValue = Expression.Property(currentTenantIdProperty, "Value");
            var isCurrentTenant = Expression.Equal(tenantIdProperty, tenantIdValue);

            // e.TenantId == Guid.Empty (Registros globais)
            var guidEmpty = Expression.Constant(Guid.Empty);
            var isGlobal = Expression.Equal(tenantIdProperty, guidEmpty);

            // Combinação: !HasValue || e.TenantId == Id.Value || e.TenantId == Guid.Empty
            var filter = Expression.OrElse(hasNoValue, Expression.OrElse(isCurrentTenant, isGlobal));

            var lambda = Expression.Lambda(filter, parameter);
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
                    entry.Entity.SetTenantId(tenantId.Value);
                }
            }
        }

        public static void SetUserId(this DbContext context, ICurrentUser? currentUser)
        {
            var userId = currentUser?.Id;
            if (!userId.HasValue) return;

            foreach (var entry in context.ChangeTracker.Entries<IUserEntity>())
            {
                if (entry.State == EntityState.Added && entry.Entity.UserId == Guid.Empty)
                {
                    entry.Entity.SetUserId(userId.Value);
                }
            }
        }
    }
}
