using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class WhatsAppNumberVerification : BaseEntity
    {
        public string PhoneNumber { get; private set; }
        public bool IsValid { get; private set; }
        public DateTime ValidatedAt { get; private set; }

        protected WhatsAppNumberVerification() { }

        public WhatsAppNumberVerification(string phoneNumber, bool isValid)
        {
            PhoneNumber = phoneNumber;
            IsValid = isValid;
            ValidatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(bool isValid)
        {
            IsValid = isValid;
            ValidatedAt = DateTime.UtcNow;
        }
    }
}
