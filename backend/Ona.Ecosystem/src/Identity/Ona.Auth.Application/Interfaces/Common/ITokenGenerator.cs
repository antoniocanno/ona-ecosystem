namespace Ona.Auth.Application.Interfaces.Common
{
    public interface ITokenGenerator
    {
        string GenerateVerificationToken();
        string GenerateTimeBasedToken();
        string GenerateSecureToken(int length = 32);
        bool TryExtractTicksFromToken(string token, out long ticks);
    }
}
