using Ona.Domain.Shared.Entities;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Commit.Domain.Entities
{
    public class Customer : TenantEntity, IUserEntity
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }

        public string? InternalNotes { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
    }
}
