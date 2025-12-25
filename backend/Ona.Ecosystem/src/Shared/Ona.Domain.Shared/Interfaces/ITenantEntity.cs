namespace Ona.Domain.Shared.Interfaces
{
    public interface ITenantEntity
    {
        Guid TenantId { get; }
        void SetTenantId(Guid tenantId);
    }
}
