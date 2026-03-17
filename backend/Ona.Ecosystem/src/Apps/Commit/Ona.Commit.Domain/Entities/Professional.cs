using Ona.Core.Common.Exceptions;
using Ona.Core.Entities;

namespace Ona.Commit.Domain.Entities
{
    public class Professional : TenantEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string? PhoneNumber { get; private set; }
        public Guid? ApplicationUserId { get; private set; }

        protected Professional() { }

        public Professional(string name, string email, string? phoneNumber = null, Guid? applicationUserId = null)
        {
            SetName(name);
            SetEmail(email);
            if (!string.IsNullOrEmpty(phoneNumber)) SetPhoneNumber(phoneNumber);
            if (applicationUserId.HasValue) SetApplicationUserId(applicationUserId.Value);
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ValidationException("Nome é obrigatório.");
            Name = name;
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) throw new ValidationException("Email é obrigatório.");
            Email = email;
        }

        public void SetPhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public void SetApplicationUserId(Guid applicationUserId)
        {
            ApplicationUserId = applicationUserId;
        }
    }
}
