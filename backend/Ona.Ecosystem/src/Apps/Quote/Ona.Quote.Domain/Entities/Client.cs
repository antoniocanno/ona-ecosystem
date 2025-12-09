using Ona.Domain.Shared.Entities;

namespace Ona.Quote.Domain.Entities
{
    public class Client : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public Client(Guid userId, string name, string email, string phone)
        {
            UserId = userId;
            Name = name;
            Email = email;
            Phone = phone;
        }
    }
}
