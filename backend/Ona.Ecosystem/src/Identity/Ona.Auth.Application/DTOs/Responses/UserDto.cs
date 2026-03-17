using Mapster;
using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.DTOs.Responses
{
    public record UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IEnumerable<string>? Roles { get; set; }
        public bool LockoutEnabled { get; set; }

        public static implicit operator UserDto(ApplicationUser user)
            => user.Adapt<UserDto>();
    }
}
