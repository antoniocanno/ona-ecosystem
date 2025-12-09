using Ona.Auth.Application.DTOs.Request;
using Ona.Auth.Application.DTOs.Responses;

namespace Ona.Auth.Application.Interfaces.Services
{
    public interface IUserAppService
    {
        Task<UserDto> GetDtoByIdAsync(Guid id);
        Task<UserDto> UpdateAsync(Guid id, UserUpdateRequest request);
    }
}
