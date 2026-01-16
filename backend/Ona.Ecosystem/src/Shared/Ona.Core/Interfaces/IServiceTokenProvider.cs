namespace Ona.Core.Interfaces
{
    public interface IServiceTokenProvider
    {
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
