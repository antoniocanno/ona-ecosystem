using Ona.Domain.Shared.Interfaces;
using Ona.Quote.Domain.Entities;

namespace Ona.Quote.Domain.Interfaces.Repositories
{
    public interface IClientRepository : IBaseRepository<Client>
    {
        Task<Client?> GetByIdAsync(Guid userId, Guid id);
    }
}
