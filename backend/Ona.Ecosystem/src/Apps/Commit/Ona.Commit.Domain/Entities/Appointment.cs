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
        public Customer Customer { get; private set; } = null!;

        public DateTimeOffset StartDate { get; private set; }
        public DateTimeOffset EndDate { get; private set; }

        public AppointmentStatus Status { get; private set; } = AppointmentStatus.Pending;
        public int? RiskScore { get; private set; }

        public string? ExternalCalendarEventId { get; private set; }

        public ICollection<NotificationLog> Notifications { get; private set; } = [];

        protected Appointment() { }

        public Appointment(Guid userId, Guid customerId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            SetUserId(userId);
            SetCustomerId(customerId);
            SetStartAndEndDate(startDate, endDate);
        }

        private void SetCustomerId(Guid customerId)
        {
            if (customerId == Guid.Empty)
                throw new ValidationException("O agendamento deve ter um cliente vinculado.");

            CustomerId = customerId;
        }

        private void SetUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ValidationException("O agendamento deve ter um usuário vinculado.");

            UserId = userId;
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
    }
}
