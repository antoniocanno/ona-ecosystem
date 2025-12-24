namespace Ona.Commit.Application.DTOs.Request
{
    public record CreateCustomerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? InternalNotes { get; set; }
    }
}
