namespace Ona.Auth.Application.Settings
{
    public record SecuritySettings
    {
        public int FailedLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
        public int SecureRandomTokenLength { get; set; } = 32;
        public int MaximumRefreshTokensPerUser { get; set; } = 5;
        public int RefreshTokenExpiryDays { get; set; } = 30;
        public int EmailVerificationTokenExpiryHours { get; set; } = 24;
        public int PasswordResetTokenExpiryHours { get; set; } = 2;
        public AttemptSettings? AttemptSettings { get; set; }
    }

    public record AttemptSettings
    {
        public int MaxAttemptsPerHour { get; set; } = 3;
    }
}
