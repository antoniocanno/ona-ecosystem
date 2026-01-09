using Mapster;
using Ona.Commit.Domain.Entities;

namespace Ona.Commit.Application.DTOs.Responses
{
    public record CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? InternalNotes { get; set; }
        public int TotalNoShows { get; set; }

        public static implicit operator CustomerDto(Customer customer)
            => customer.Adapt<CustomerDto>();
    }
}
