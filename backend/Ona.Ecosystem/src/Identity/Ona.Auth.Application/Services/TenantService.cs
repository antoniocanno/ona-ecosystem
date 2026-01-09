using Mapster;
using Microsoft.AspNetCore.Identity;
using Ona.Application.Shared.DTOs.Tenants;
using Ona.Application.Shared.Interfaces.Services;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Domain.Entities;
using Ona.Core.Common.Enums;
using Ona.Core.Common.Exceptions;
using Ona.Core.Entities;
using Ona.Core.Interfaces;

namespace Ona.Auth.Application.Services
{
    public class TenantService : ITenantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantRepository _tenantRepository;
        private readonly IUserTenantRoleRepository _userTenantRoleRepository;
        private readonly IApplicationRoleRepository _roleRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICurrentUser _currentUser;

        public TenantService(
            IUnitOfWork unitOfWork,
            ITenantRepository tenantRepository,
            IUserTenantRoleRepository userTenantRoleRepository,
            IApplicationRoleRepository roleRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ICurrentUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _tenantRepository = tenantRepository;
            _userTenantRoleRepository = userTenantRoleRepository;
            _roleRepository = roleRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUser = currentUser;
        }

        public async Task<Tenant> CreateTenantAsync(CreateTenantRequest request)
        {
            if (!_currentUser.Id.HasValue)
                throw new ValidationException("Contexto de usuário necessário");

            var userId = _currentUser.Id.Value;
            var tenant = request.Adapt<Tenant>();
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null) throw new ValidationException("Usuário não encontrado");

            if (user.TenantId != Guid.Empty) throw new ValidationException("Usuário já possui um tenant");

            await _tenantRepository.CreateAsync(tenant);

            await CreateDefaultRolesAsync(tenant.Id);

            await AddUserToRoleAsync(user, nameof(Role.Manager), tenant.Id);

            if (user.TenantId == Guid.Empty)
            {
                user.SetTenantId(tenant.Id);
                await _userManager.UpdateAsync(user);
            }

            await _unitOfWork.CommitAsync();

            return tenant;
        }

        public async Task<TenantDto?> GetByIdAsync(Guid id)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            return tenant?.Adapt<TenantDto>();
        }

        public async Task<IEnumerable<TenantDto>> ListAsync()
        {
            var tenants = await _tenantRepository.GetAllAsync();
            return tenants.Adapt<IEnumerable<TenantDto>>();
        }

        public async Task<TenantDto> UpdateAsync(Guid id, UpdateTenantRequest request)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null) throw new NotFoundException("Tenant não encontrado.");

            if (!string.IsNullOrEmpty(request.Name))
                tenant.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Domain))
                tenant.Domain = request.Domain;

            if (!string.IsNullOrEmpty(request.TimeZone))
                tenant.TimeZone = request.TimeZone;

            if (request.WhatsAppInstanceId != null)
                tenant.WhatsAppInstanceId = request.WhatsAppInstanceId;

            if (request.WhatsAppApiKey != null)
                tenant.WhatsAppApiKey = request.WhatsAppApiKey;

            if (request.IsWhatsAppConnected.HasValue)
                tenant.IsWhatsAppConnected = request.IsWhatsAppConnected.Value;

            tenant.Update();

            _tenantRepository.Update(tenant);
            await _unitOfWork.CommitAsync();

            return tenant.Adapt<TenantDto>();
        }

        public async Task SuspendAsync(Guid id)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null) throw new NotFoundException("Tenant não encontrado.");

            tenant.Status = TenantStatus.Suspended;
            tenant.Update();

            _tenantRepository.Update(tenant);
            await _unitOfWork.CommitAsync();
        }

        public async Task ActivateAsync(Guid id)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null) throw new NotFoundException("Tenant não encontrado.");

            tenant.Status = TenantStatus.Active;
            tenant.Update();

            _tenantRepository.Update(tenant);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null) throw new NotFoundException("Tenant não encontrado.");

            tenant.Delete();

            _tenantRepository.Update(tenant);
            await _unitOfWork.CommitAsync();
        }

        private async Task CreateDefaultRolesAsync(Guid tenantId)
        {
            await CreateRoleAsync(tenantId, nameof(Role.Manager));
            await CreateRoleAsync(tenantId, nameof(Role.Operator));
            await CreateRoleAsync(tenantId, nameof(Role.ReadOnly));
        }

        private async Task CreateRoleAsync(Guid tenantId, string roleName)
        {
            var role = new ApplicationRole(roleName, tenantId);

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                throw new ValidationException($"Falha ao criar role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        private async Task AddUserToRoleAsync(ApplicationUser user, string roleName, Guid tenantId)
        {
            var role = await _roleRepository.GetByNameAndTenantAsync(roleName, tenantId);

            if (role == null) throw new ValidationException($"Role {roleName} não encontrada para o tenant {tenantId}");

            var userRole = new UserTenantRole(user.Id, role.Id, tenantId);

            await _userTenantRoleRepository.CreateAsync(userRole);
            await _unitOfWork.CommitAsync();
        }
    }
}
