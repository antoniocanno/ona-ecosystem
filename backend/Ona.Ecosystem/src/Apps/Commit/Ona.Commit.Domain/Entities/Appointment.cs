using Ona.Commit.Domain.Enums;
using Ona.Core.Common.Exceptions;
using Ona.Domain.Shared.Entities;
using Ona.Domain.Shared.Interfaces;

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

        public string? ExternalCalendarEventId { get; private set; }

        public ICollection<NotificationLog> Notifications { get; private set; } = [];

        protected Appointment() { }

        public Appointment(Guid userId, Guid customerId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (userId == Guid.Empty)
                throw new ValidationException("O agendamento deve ter um usuário vinculado.");

            if (customerId == Guid.Empty)
                throw new ValidationException("O agendamento deve ter um cliente vinculado.");

            if (startDate >= endDate)
                throw new ValidationException("A data de início deve ser anterior à data de término.");
            if (startDate < DateTimeOffset.UtcNow)
                throw new ValidationException("A data de início não pode ser no passado.");

            UserId = userId;
            CustomerId = customerId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public void Reschedule(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (startDate >= endDate)
                throw new ValidationException("A data de início deve ser anterior à data de término.");
            if (startDate < DateTimeOffset.UtcNow)
                throw new ValidationException("A data de início não pode ser no passado.");

            StartDate = startDate;
            EndDate = endDate;

            Update();
        }

        public void UpdateStatus(AppointmentStatus status)
        {
            Status = status;
            Update();
        }
    }
}
