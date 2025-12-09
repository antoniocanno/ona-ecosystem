using Microsoft.EntityFrameworkCore;
using Ona.Infrastructure.Shared.Repositories;
using Ona.Quote.Domain.Entities;
using Ona.Quote.Domain.Interfaces.Repositories;
using Ona.Quote.Infrastructure.Data;

namespace Ona.Quote.Infrastructure.Repositories
{
    internal class ClientRepository : BaseRepository<QuoteDbContext, Client>, IClientRepository
    {
        public ClientRepository(QuoteDbContext context)
            : base(context) { }

        public async Task<Client?> GetByIdAsync(Guid userId, Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }
    }
}
