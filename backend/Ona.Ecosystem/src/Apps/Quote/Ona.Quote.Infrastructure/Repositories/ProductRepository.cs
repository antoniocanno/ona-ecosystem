using Ona.Infrastructure.Shared.Repositories;
using Ona.Quote.Domain.Entities;
using Ona.Quote.Domain.Interfaces.Repositories;
using Ona.Quote.Infrastructure.Data;

namespace Ona.Quote.Infrastructure.Repositories
{
    internal class ProductRepository : BaseRepository<QuoteDbContext, Product>, IProductRepository
    {
        public ProductRepository(QuoteDbContext context)
            : base(context) { }
    }
}
