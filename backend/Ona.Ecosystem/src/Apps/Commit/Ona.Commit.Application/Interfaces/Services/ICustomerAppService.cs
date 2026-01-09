using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.DTOs.Responses;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface ICustomerAppService
    {
        Task<IEnumerable<CustomerDto>> ListAsync();
        Task<CustomerDto?> GetByIdAsync(Guid id);
        Task<CustomerDto> CreateAsync(CreateCustomerRequest request);
        Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request);
        Task DeleteAsync(Guid id);
    }
}
