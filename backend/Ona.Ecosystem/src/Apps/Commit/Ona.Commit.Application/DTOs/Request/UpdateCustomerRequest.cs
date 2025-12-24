namespace Ona.Commit.Application.DTOs.Request
{
    public record UpdateCustomerRequest
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? InternalNotes { get; set; }
        public int? TotalNoShows { get; set; }
    }
}
