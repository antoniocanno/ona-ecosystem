using MassTransit;
using Ona.Application.Shared.Events;
using Ona.Core.Tenant;

namespace Ona.Infrastructure.Shared.Integrations
{
    public class TenantUpdatedConsumer : IConsumer<TenantUpdatedEvent>
    {
        private readonly ITenantProvider _tenantProvider;

        public TenantUpdatedConsumer(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        public Task Consume(ConsumeContext<TenantUpdatedEvent> context)
        {
            _tenantProvider.Invalidate(context.Message.TenantId);
            return Task.CompletedTask;
        }
    }
}
