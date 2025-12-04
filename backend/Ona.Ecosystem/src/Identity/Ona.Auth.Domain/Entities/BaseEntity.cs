using Ona.Core.Common.Helpers;

namespace Ona.Auth.Domain.Entities
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; } = GuidGenerator.NewSequentialGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
