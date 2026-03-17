namespace Ona.Core.Interfaces
{
    public interface ICurrentTenant
    {
        Guid? Id { get; }
        bool IsAvailable { get; }
    }
}
