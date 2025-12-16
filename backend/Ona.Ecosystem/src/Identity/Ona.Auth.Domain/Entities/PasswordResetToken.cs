namespace Ona.Auth.Domain.Entities
{
    public class PasswordResetToken : BaseToken
    {
        public PasswordResetToken()
        {
            ExpiresAt = DateTime.UtcNow.AddHours(2);
        }
    }
}
