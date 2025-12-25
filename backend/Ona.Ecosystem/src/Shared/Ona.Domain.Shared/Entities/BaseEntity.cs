using Ona.Core.Common.Helpers;

namespace Ona.Domain.Shared.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; } = GuidGenerator.NewSequentialGuid();
        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; protected set; }
        public bool IsDeleted { get; protected set; } = false;

        public void Update()
        {
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Delete()
        {
            IsDeleted = true;
            Update();
        }
    }
}
