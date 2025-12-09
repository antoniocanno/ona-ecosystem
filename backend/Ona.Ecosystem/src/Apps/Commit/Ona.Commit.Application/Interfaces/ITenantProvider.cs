namespace Ona.Commit.Application.Interfaces
{
    public interface ITenantProvider
    {
        Guid TenantId { get; }
        bool HasTenant { get; }
    }
}
