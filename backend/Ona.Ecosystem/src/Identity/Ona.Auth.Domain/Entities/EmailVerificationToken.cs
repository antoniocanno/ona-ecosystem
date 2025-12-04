using System.ComponentModel.DataAnnotations.Schema;

namespace Ona.Auth.Domain.Entities
{
    [Table("EmailVerificationTokens")]
    public class EmailVerificationToken : BaseToken
    {
    }
}
