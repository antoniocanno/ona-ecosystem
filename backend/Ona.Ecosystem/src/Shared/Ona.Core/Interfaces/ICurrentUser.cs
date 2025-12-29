namespace Ona.Core.Interfaces
{
    public interface ICurrentUser
    {
        Guid? Id { get; }
        bool IsAuthenticated { get; }
        string? Email { get; }
        bool IsEmailVerified { get; }
        string AuthMethod { get; }
    }
}
