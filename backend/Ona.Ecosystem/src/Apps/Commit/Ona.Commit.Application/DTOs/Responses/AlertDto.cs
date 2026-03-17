namespace Ona.Commit.Application.DTOs.Responses
{
    public class AlertDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Guid? RelatedAppointmentId { get; set; }
    }
}
