using Mapster;
using Ona.Commit.Domain.Entities;

namespace Ona.Commit.Application.DTOs.Responses
{
    public class ProfessionalDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public Guid? ApplicationUserId { get; set; }

        public static implicit operator ProfessionalDto(Professional professional)
            => professional.Adapt<ProfessionalDto>();
    }
}
