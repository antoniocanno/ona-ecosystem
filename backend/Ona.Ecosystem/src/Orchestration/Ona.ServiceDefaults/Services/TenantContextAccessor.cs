using Ona.Core.Interfaces;
using Ona.Core.Tenant;

namespace Ona.ServiceDefaults.Services
{
    public class TenantContextAccessor : ITenantContextAccessor
    {
        private readonly ICurrentTenant _currentTenant;
        private readonly ITenantProvider _tenantProvider;

        public TenantContextAccessor() { }

        public TenantContextAccessor(ICurrentTenant currentTenant, ITenantProvider tenantProvider)
        {
            _currentTenant = currentTenant;
            _tenantProvider = tenantProvider;
        }

        private TenantContext? _current;

        public TenantContext Current
            => _current ?? throw new InvalidOperationException("Contexto do cliente não está disponível.");

        public void SetCurrent(TenantContext context)
            => _current = context;

        public async Task<TenantContext> GetCurrentContextAsync()
        {
            if (!_currentTenant.IsAvailable)
                throw new InvalidOperationException("");

            return await _tenantProvider.GetAsync(_currentTenant.Id!.Value);
        }
    }
}
