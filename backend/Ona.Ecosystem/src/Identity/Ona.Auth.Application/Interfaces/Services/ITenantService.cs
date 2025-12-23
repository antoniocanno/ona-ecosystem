using Ona.Auth.Application.DTOs.Request;
using Ona.Auth.Application.DTOs.Responses;
using Ona.Auth.Domain.Entities;

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
