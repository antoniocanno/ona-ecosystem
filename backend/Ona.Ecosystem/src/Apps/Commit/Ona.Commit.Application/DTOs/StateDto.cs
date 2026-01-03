namespace Ona.Commit.Application.DTOs
{
    public class StateDto
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public Guid Nonce { get; set; }
    }
}
