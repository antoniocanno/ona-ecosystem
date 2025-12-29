using Ona.Core.Common.Exceptions;
using Ona.Core.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Entities
{
    public class Customer : TenantEntity, IUserEntity
    {
        public Guid UserId { get; private set; }
        public string Name { get; private set; } = string.Empty;

        public string PhoneNumber { get; private set; } = string.Empty;
        public string? Email { get; private set; }

        public string? InternalNotes { get; private set; }
        public int TotalNoShows { get; private set; }

        public ICollection<Appointment>? Appointments { get; private set; }

        protected Customer() { }

        public Customer(Guid userId, string name, string phoneNumber, string? email, string? internalNotes)
        {
            if (userId == Guid.Empty)
                throw new ValidationException("Usuário inválido.");

            SetName(name);
            SetPhoneNumber(phoneNumber);

            UserId = userId;
            Email = email;
            InternalNotes = internalNotes;
        }

        private void SetPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ValidationException("O telefone do cliente é obrigatório.");

            var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
            PhoneNumber = $"+{digitsOnly}";
        }

        private void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ValidationException("O nome do cliente é obrigatório.");

            Name = name;
        }

        public void UpdateName(string name)
        {
            SetName(name);
            Update();
        }

        public void UpdatePhoneNumber(string phoneNumber)
        {
            SetPhoneNumber(phoneNumber);
            Update();
        }

        public void UpdateEmail(string? email)
        {
            Email = string.IsNullOrEmpty(email) ? Email : email;
            Update();
        }

        public void UpdateInternalNotes(string internalNotes)
        {
            InternalNotes = internalNotes;
            Update();
        }

        public void IncrementTotalNoShows() => TotalNoShows++;
    }
}
