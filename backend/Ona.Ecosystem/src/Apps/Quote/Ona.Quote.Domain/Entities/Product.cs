using Ona.Domain.Shared.Entities;

namespace Ona.Quote.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; } = 0;
    }
}
