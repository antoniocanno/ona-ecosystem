using Ona.Auth.Application.DTOs.Requests;
using Ona.Auth.Application.DTOs.Responses;

namespace Ona.Auth.Application.Interfaces.Services
{
    public interface IUserAppService
    {
        Task<IEnumerable<UserDto>> ListAsync();
        Task<UserDto> GetByIdAsync(Guid id);
        Task<UserDto> UpdateAsync(Guid id, UserUpdateRequest request);
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task InviteUserAsync(InviteUserRequest request);
        Task AcceptInviteAsync(AcceptInviteRequest request);
        Task<UserDto> GetMeAsync();
        Task<UserDto> UpdateMeAsync(UserUpdateRequest request);
        Task BlockUserAsync(Guid userId);
        Task UnblockUserAsync(Guid userId);
    }
}
