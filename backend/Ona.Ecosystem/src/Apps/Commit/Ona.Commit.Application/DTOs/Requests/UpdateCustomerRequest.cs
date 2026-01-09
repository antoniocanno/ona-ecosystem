namespace Ona.Commit.Application.DTOs.Requests
{
    public record UpdateCustomerRequest
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? InternalNotes { get; set; }
    }
}
