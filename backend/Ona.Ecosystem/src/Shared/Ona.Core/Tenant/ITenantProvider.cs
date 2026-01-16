namespace Ona.Core.Tenant
{
    public interface ITenantProvider
    {
        Task<TenantContext> GetAsync(Guid tenantId);
        void Invalidate(Guid tenantId);
    }
}
