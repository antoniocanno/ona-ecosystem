namespace Ona.Commit.Domain.Interfaces.Workers
{
    public interface ICalendarTokenRefreshWorker
    {
        Task RefreshExpiringTokensAsync();
    }
}
