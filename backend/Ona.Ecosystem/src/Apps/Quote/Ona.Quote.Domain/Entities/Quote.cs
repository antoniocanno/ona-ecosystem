using Ona.Domain.Shared.Entities;
using Ona.Quote.Domain.Enum;

namespace Ona.Quote.Domain.Entities
{
    public class Quote : BaseEntity
    {
        public Guid ApplicationUserId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
        public decimal Total { get; private set; }
        public int ViewsCount { get; private set; }
        public ICollection<QuoteLine> Lines { get; } = [];

        public void AddLine(QuoteLine line)
        {
            Lines.Add(line);
            SetTotal();
        }

        public void RemoveLine(Guid lineId)
        {
            Lines.Remove(Lines.First(line => line.Id == lineId));
            SetTotal();
        }

        private void SetTotal()
        {
            Total = Lines.Sum(line => line.LineTotal);
        }

        public void IncreaseViewsCount()
        {
            ViewsCount++;
        }
    }
}
