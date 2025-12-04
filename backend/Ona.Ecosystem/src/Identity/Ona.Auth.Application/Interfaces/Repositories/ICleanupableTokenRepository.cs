namespace Ona.Auth.Application.Interfaces.Repositories
{
    public interface ICleanupableTokenRepository
    {
        Task CleanupExpiredTokensAsync();
    }
}
