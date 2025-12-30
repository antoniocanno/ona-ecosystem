namespace Ona.Commit.Domain.Interfaces.Workers
{
    public interface ICalendarSyncWorker
    {
        Task SyncFromGoogleAsync(string resourceId, string channelId);
        Task SyncFromOutlookAsync(string resourceId, string subscriptionId);
    }
}
