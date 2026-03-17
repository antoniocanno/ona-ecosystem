using Ona.Commit.Application.DTOs.Responses;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface IAlertAppService
    {
        Task<IEnumerable<AlertDto>> GetUnreadAsync();
        Task MarkAsReadAsync(Guid id);
    }
}
