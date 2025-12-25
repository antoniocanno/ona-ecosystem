using Ona.Core.Common.Exceptions;
using Ona.Domain.Shared.Entities;
using Ona.Domain.Shared.Interfaces;

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

        public ICollection<Appointment> Appointments { get; private set; }

        protected Customer() { }

        public Customer(Guid userId, string name, string phoneNumber, string? email, string? internalNotes)
        {
            if (userId == Guid.Empty)
                throw new ValidationException("O cliente deve ter um usuário vinculado.");

            if (string.IsNullOrEmpty(name))
                throw new ValidationException("O cliente deve ter um nome.");

            if (string.IsNullOrEmpty(phoneNumber))
                throw new ValidationException("O cliente deve ter um telefone.");

            UserId = userId;
            Name = name;
            PhoneNumber = phoneNumber;
            Email = email;
            InternalNotes = internalNotes;
        }
        public void UpdateName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ValidationException("O cliente deve ter um nome.");

            Name = name;
            Update();
        }

        public void UpdatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                throw new ValidationException("O cliente deve ter um telefone.");

            PhoneNumber = phoneNumber;
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
    }
}
