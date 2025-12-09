using Ona.Infrastructure.Shared.Repositories;
using Ona.Quote.Domain.Interfaces.Repositories;
using Ona.Quote.Infrastructure.Data;

namespace Ona.Quote.Infrastructure.Repositories
{
    internal class QuoteRepository : BaseRepository<QuoteDbContext, Domain.Entities.Quote>, IQuoteRepository
    {
        public QuoteRepository(QuoteDbContext context)
            : base(context) { }

        public new async Task<Domain.Entities.Quote> CreateAsync(Domain.Entities.Quote quote)
        {
            var entry = await _dbSet.AddAsync(quote);
            return entry.Entity;
        }
    }
}
