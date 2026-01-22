using Ona.Core.Common.Exceptions;
using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class OperatorAlert : TenantEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public bool IsRead { get; private set; }

        public Guid? RelatedAppointmentId { get; private set; }
        public Appointment? RelatedAppointment { get; private set; }

        public Guid? ProfessionalId { get; private set; }

        protected OperatorAlert() { }

        public OperatorAlert(string title, string message, Guid? relatedAppointmentId = null, Guid? professionalId = null)
        {
            SetTitle(title);
            SetMessage(message);
            RelatedAppointmentId = relatedAppointmentId;
            ProfessionalId = professionalId;
            IsRead = false;
        }

        public void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ValidationException("Título não pode ser vazio");
            Title = title;
        }

        public void SetMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ValidationException("Mensagem não pode ser vazia");
            Message = message;
        }

        public void MarkAsRead()
        {
            IsRead = true;
            Update();
        }
    }
}
