namespace Ona.Commit.Application.DTOs.Requests;

public class GetMessageStatusRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
}
