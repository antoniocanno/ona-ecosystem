using Ona.Application.Shared.DTOs.Tenants;
using Ona.Core.Entities;

namespace Ona.Auth.Application.Interfaces.Services
{
    public interface ITenantService
    {
        Task<Tenant> CreateTenantAsync(CreateTenantRequest request);
        Task<TenantDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<TenantDto>> ListAsync();
        Task<TenantDto> UpdateAsync(Guid id, UpdateTenantRequest request);
        Task SuspendAsync(Guid id);
        Task ActivateAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
