namespace Ona.Quote.Application.DTOs.Request
{
    public class ClientUpdateRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
