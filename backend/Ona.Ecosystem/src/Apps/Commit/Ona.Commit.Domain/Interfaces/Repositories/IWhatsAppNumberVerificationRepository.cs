using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Interfaces.Repositories
{
    public interface IWhatsAppNumberVerificationRepository : IBaseRepository<WhatsAppNumberVerification>
    {
        Task<WhatsAppNumberVerification?> GetByPhoneNumberAsync(string phoneNumber);
    }
}
