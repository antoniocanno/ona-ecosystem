using System.ComponentModel.DataAnnotations.Schema;

namespace Ona.Auth.Domain.Entities
{
    public class UnlockUserToken : BaseToken
    {
        public UnlockUserToken()
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(30);
        }
    }
}
