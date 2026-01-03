namespace Ona.Commit.Application.DTOs.Request
{
    public record CreateAppointmentRequest
    {
        public Guid CustomerId { get; set; }
        public string Summary { get; set; } = "Agendamento";
        public string Description { get; set; } = "Agendamento";
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }
}
