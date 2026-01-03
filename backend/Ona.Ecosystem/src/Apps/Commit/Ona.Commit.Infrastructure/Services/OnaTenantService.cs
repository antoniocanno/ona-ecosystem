using Microsoft.EntityFrameworkCore;
using Ona.Application.Shared.DTOs.Tenants;
using Ona.Application.Shared.Interfaces.Services;
using Ona.Commit.Infrastructure.Data;
using Ona.Core.Entities;

namespace Ona.Commit.Infrastructure.Services
{
    public class OnaTenantService : ITenantService
    {
        private readonly CommitDbContext _context;

        public OnaTenantService(CommitDbContext context)
        {
            _context = context;
        }

        public async Task<TenantDto?> GetByIdAsync(Guid id)
        {
            var settings = await _context.TenantSettings
                .FirstOrDefaultAsync(x => x.TenantId == id);

            if (settings == null)
            {
                return new TenantDto
                {
                    Id = id,
                    Name = "Tenant",
                    TimeZone = "America/Sao_Paulo"
                };
            }

            return new TenantDto
            {
                Id = id,
                Name = settings.BusinessName,
                TimeZone = settings.TimezoneId
            };
        }

        public Task<IEnumerable<TenantDto>> ListAsync() => throw new NotSupportedException();
        public Task<TenantDto> UpdateAsync(Guid id, UpdateTenantRequest request) => throw new NotSupportedException();
        public Task SuspendAsync(Guid id) => throw new NotSupportedException();
        public Task ActivateAsync(Guid id) => throw new NotSupportedException();
        public Task DeleteAsync(Guid id) => throw new NotSupportedException();
        public Task<Tenant> CreateTenantAsync(CreateTenantRequest request) => throw new NotSupportedException();
    }
}
