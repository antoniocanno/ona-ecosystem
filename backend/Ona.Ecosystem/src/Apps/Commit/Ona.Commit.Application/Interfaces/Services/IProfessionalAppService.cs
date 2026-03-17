using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.DTOs.Responses;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface IProfessionalAppService
    {
        Task<IEnumerable<ProfessionalDto>> ListAsync();
        Task<ProfessionalDto?> GetByIdAsync(Guid id);
        Task<ProfessionalDto?> GetByEmailAsync(string email);
        Task<ProfessionalDto?> GetByCurrentUserIdAsync();
        Task<ProfessionalDto> CreateAsync(CreateProfessionalRequest request);
        Task<ProfessionalDto> RegisterProfessionalWithUserAsync(RegisterProfessionalRequest request);
        Task<ProfessionalDto> UpdateAsync(Guid id, UpdateProfessionalRequest request);
        Task DeleteAsync(Guid id);
    }
}
