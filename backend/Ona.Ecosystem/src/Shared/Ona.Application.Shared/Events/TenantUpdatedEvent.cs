namespace Ona.Application.Shared.Events
{
    public sealed record TenantUpdatedEvent
    (
        Guid TenantId,
        string TimeZone,
        DateTimeOffset OccurredAt
    );
}
