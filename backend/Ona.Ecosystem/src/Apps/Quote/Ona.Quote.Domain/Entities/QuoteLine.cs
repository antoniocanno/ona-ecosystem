namespace Ona.Quote.Domain.Entities
{
    public class QuoteLine
    {
        public Guid Id { get; set; }
        public Guid QuoteId { get; set; }
        public Product? Product { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 0;
        public decimal LineTotal => Quantity * UnitPrice;

        public Quote Quote { get; set; } = null!;
    }
}
