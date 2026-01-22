using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Application.Interfaces.Repositories
{
    public interface IOperatorAlertRepository : IBaseRepository<OperatorAlert>
    {
        Task<IEnumerable<OperatorAlert>> GetUnreadAsync();
    }
}
