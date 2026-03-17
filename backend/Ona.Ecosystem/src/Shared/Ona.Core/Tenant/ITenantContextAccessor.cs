namespace Ona.Core.Tenant
{
    public interface ITenantContextAccessor
    {
        TenantContext Current { get; }
        void SetCurrent(TenantContext context);
        Task<TenantContext> GetCurrentContextAsync();
    }
}
