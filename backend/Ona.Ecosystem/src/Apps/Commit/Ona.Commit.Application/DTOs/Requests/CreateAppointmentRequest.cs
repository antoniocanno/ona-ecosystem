namespace Ona.Commit.Application.DTOs.Requests
{
    public record CreateAppointmentRequest
    {
        public Guid CustomerId { get; set; }
        public Guid ProfessionalId { get; set; }
        public string Summary { get; set; } = "Agendamento";
        public string Description { get; set; } = "Agendamento";
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }
}
