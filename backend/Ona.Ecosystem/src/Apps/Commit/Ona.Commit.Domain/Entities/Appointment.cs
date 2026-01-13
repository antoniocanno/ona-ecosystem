using Ona.Commit.Domain.Enums;
using Ona.Core.Common.Exceptions;
using Ona.Core.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Entities
{
    public class Appointment : TenantEntity, IUserEntity
    {
        public Guid UserId { get; private set; }
        public Guid CustomerId { get; private set; }
        public Customer? Customer { get; private set; } = null!;

        public Guid ProfessionalId { get; private set; }
        public Professional? Professional { get; private set; } = null!;

        public string? Summary { get; private set; }
        public string? Description { get; private set; }

        public DateTimeOffset StartDate { get; private set; }
        public DateTimeOffset EndDate { get; private set; }

        public AppointmentStatus Status { get; private set; } = AppointmentStatus.Pending;
        public int? RiskScore { get; private set; }

        public string? ExternalCalendarEventId { get; private set; }

        public int? ReminderLeadTime { get; private set; }
        public ReminderStatus ReminderStatus { get; private set; } = ReminderStatus.Pending;

        public ICollection<NotificationLog> Notifications { get; private set; } = [];

        protected Appointment() { }

        public Appointment(Guid customerId, Guid professionalId, string summary, string description, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            SetCustomerId(customerId);
            SetProfessionalId(professionalId);
            SetSummary(summary);
            SetDescription(description);
            SetStartAndEndDate(startDate, endDate);
        }

        public void SetUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ValidationException("O agendamento deve ter um usuário vinculado.");

            UserId = userId;
        }

        private void SetCustomerId(Guid customerId)
        {
            if (customerId == Guid.Empty)
                throw new ValidationException("O agendamento deve ter um cliente vinculado.");

            CustomerId = customerId;
        }

        public void SetProfessionalId(Guid professionalId)
        {
            if (professionalId == Guid.Empty)
                throw new ValidationException("O agendamento deve ter um profissional vinculado.");

            ProfessionalId = professionalId;
        }

        public void SetSummary(string summary)
        {
            if (string.IsNullOrEmpty(summary))
                throw new ValidationException("O agendamento deve ter um resumo.");

            Summary = summary;
        }

        public void SetDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                throw new ValidationException("O agendamento deve ter uma descrição.");

            Description = description;
        }

        private void SetStartAndEndDate(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (startDate == default || endDate == default)
                throw new ValidationException("A data do agendamento deve ser informada.");

            if (startDate >= endDate)
                throw new ValidationException("A data de início deve ser anterior à data de término.");
            if (startDate < DateTimeOffset.UtcNow)
                throw new ValidationException("A data do agendamento não pode ser no passado.");

            StartDate = startDate;
            EndDate = endDate;
        }

        public void Reschedule(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            SetStartAndEndDate(startDate, endDate);
            Update();
        }

        public void UpdateStatus(AppointmentStatus status)
        {
            if (status == AppointmentStatus.Confirmed || status == AppointmentStatus.NoShow)
                throw new ValidationException("Status inválido.");

            if (status == AppointmentStatus.Canceled || status == AppointmentStatus.Rescheduled)
                Notifications = [];

            Status = status;
            Update();
        }

        public void UpdateSummary(string summary)
        {
            SetSummary(summary);
            Update();
        }

        public void UpdateDescription(string description)
        {
            SetDescription(description);
            Update();
        }

        public void Confirm()
        {
            Status = AppointmentStatus.Confirmed;
            Update();
        }

        public void NoShow()
        {
            Status = AppointmentStatus.NoShow;
            Update();
        }

        public void SetRiskScore(int riskScore)
        {
            if (riskScore < 0 || riskScore > 100)
                throw new ValidationException("O score de risco deve estar entre 0 e 100.");

            RiskScore = riskScore;
        }

        public void SetReminderLeadTime(int? reminderLeadTime)
        {
            if (reminderLeadTime.HasValue && reminderLeadTime.Value < 0)
                throw new ValidationException("O tempo de antecedência do lembrete deve ser maior ou igual a zero.");

            ReminderLeadTime = reminderLeadTime;
            Update();
        }

        public void MarkReminderAsScheduled()
        {
            ReminderStatus = ReminderStatus.Scheduled;
            Update();
        }

        public void MarkReminderAsSent()
        {
            ReminderStatus = ReminderStatus.Sent;
            Update();
        }

        public void MarkReminderAsFailed()
        {
            ReminderStatus = ReminderStatus.Failed;
            Update();
        }

        public void ResetReminderStatus()
        {
            ReminderStatus = ReminderStatus.Pending;
            Update();
        }
    }
}
