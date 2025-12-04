using System.ComponentModel.DataAnnotations.Schema;

namespace Ona.Auth.Domain.Entities
{
    [Table("UnlockUserTokens")]
    public class UnlockUserToken : BaseToken
    {
        public UnlockUserToken()
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(30);
        }
    }
}
