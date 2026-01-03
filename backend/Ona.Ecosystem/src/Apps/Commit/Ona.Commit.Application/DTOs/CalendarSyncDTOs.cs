namespace Ona.Commit.Application.DTOs
{
    public class ExternalEventDto
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "confirmed", "cancelled"
        public string Summary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public DateTimeOffset Updated { get; set; }
    }
}
