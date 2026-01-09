using Ona.Commit.Domain.Entities;

namespace Ona.Commit.Application.DTOs.Responses
{
    public class InitiateCalendarAuthRequest
    {
        public Guid ProfessionalId { get; set; }
        public CalendarProvider Provider { get; set; }
    }

    public class CompleteCalendarAuthRequest
    {
        public Guid ProfessionalId { get; set; }
        public CalendarProvider Provider { get; set; }
        public string Code { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }

    public class CalendarIntegrationResponse
    {
        public Guid Id { get; set; }
        public string Provider { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Email { get; set; }
    }
}
