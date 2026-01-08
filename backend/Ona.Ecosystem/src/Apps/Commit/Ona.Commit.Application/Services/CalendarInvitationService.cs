using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Repositories;

namespace Ona.Commit.Application.Services
{
    public interface ICalendarInvitationService
    {
        string GenerateInvitationToken(Guid professionalId);
        bool ValidateInvitationToken(string token, out Guid professionalId);
        Task LinkIntegrationAsync(Guid professionalId, CalendarProvider provider, string externalEmail, string accessToken, string refreshToken, int expiresIn);
    }

    public class CalendarInvitationService : ICalendarInvitationService
    {
        private readonly ICalendarIntegrationRepository _repository;

        public CalendarInvitationService(ICalendarIntegrationRepository repository)
        {
            _repository = repository;
        }

        public string GenerateInvitationToken(Guid professionalId)
        {
            var expiry = DateTime.UtcNow.AddHours(24);
            var payload = $"{professionalId}|{expiry.Ticks}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
        }

        public bool ValidateInvitationToken(string token, out Guid professionalId)
        {
            professionalId = Guid.Empty;
            try
            {
                var payload = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = payload.Split('|');
                if (parts.Length != 2) return false;

                if (!Guid.TryParse(parts[0], out var id) || !long.TryParse(parts[1], out var ticks)) return false;

                var expiry = new DateTime(ticks, DateTimeKind.Utc);
                if (expiry < DateTime.UtcNow) return false;

                professionalId = id;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LinkIntegrationAsync(Guid professionalId, CalendarProvider provider, string externalEmail, string accessToken, string refreshToken, int expiresIn)
        {
            var integration = await _repository.GetByProfessionalAndProviderAsync(professionalId, provider);

            if (integration == null)
            {
                integration = new CalendarIntegration
                {
                    ProfessionalId = professionalId,
                    Provider = provider,
                    IsActive = true
                };
                await _repository.CreateAsync(integration);
            }

            integration.ExternalEmailAddress = externalEmail;
            integration.AccessToken = accessToken;
            integration.EncryptedRefreshToken = refreshToken;
            integration.TokenIssuedAtUtc = DateTime.UtcNow;
            integration.TokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);
            integration.IsActive = true;

            _repository.Update(integration);
            await _repository.SaveChangesAsync();
        }
    }
}
