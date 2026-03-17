namespace Ona.Core.Interfaces
{
    public interface IUserEntity
    {
        Guid UserId { get; }

        void SetUserId(Guid userId);
    }
}
