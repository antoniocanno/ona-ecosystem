using Ona.Domain.Shared.Entities;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Commit.Domain.Entities
{
    public class Customer : BaseEntity, ITenantEntity, IUserEntity
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
    }
}
