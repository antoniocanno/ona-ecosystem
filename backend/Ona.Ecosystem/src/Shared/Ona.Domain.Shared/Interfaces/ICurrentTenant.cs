namespace Ona.Domain.Shared.Interfaces
{
    public interface ICurrentTenant
    {
        Guid? Id { get; }
        bool IsAvailable { get; }
    }
}
