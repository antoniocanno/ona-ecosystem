namespace Ona.Core.Interfaces
{
    public interface ITenantEntity
    {
        Guid TenantId { get; }
        void SetTenantId(Guid tenantId);
    }
}
