namespace Ona.Core.Interfaces
{
    public interface IDomainEvent
    {
        DateTimeOffset OccurredOn { get; }
    }
}
