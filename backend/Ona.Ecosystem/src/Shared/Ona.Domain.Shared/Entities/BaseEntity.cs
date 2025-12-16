using Ona.Core.Common.Helpers;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Domain.Shared.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; } = GuidGenerator.NewSequentialGuid();
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
