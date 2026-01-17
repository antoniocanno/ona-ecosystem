using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Application.Interfaces.Repositories
{
    public interface IProfessionalRepository : IBaseRepository<Professional>
    {
        Task<Professional?> GetByEmailAsync(string email);
        Task<Professional?> GetByApplicationUserIdAsync(Guid applicationUserId);
    }
}
