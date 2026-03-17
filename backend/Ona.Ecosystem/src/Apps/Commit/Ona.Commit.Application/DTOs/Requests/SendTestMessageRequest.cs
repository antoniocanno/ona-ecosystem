namespace Ona.Commit.Application.DTOs.Requests;

public class SendTestMessageRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
