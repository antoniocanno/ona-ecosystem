namespace Ona.Commit.Application.Interfaces.Services
{
    public interface IIdentityService
    {
        Task<Guid> CreateUserAsync(string email, string fullName, string password, string role);
    }
}
