using Ona.Core.Common.Helpers;

namespace Ona.Core.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = GuidGenerator.NewSequentialGuid();
        public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
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
