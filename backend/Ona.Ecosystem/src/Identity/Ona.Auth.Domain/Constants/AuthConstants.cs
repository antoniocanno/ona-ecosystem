namespace Ona.Auth.Domain.Constants
{
    public static class AuthConstants
    {
        public static class Errors
        {
            public const string InvalidCredentials = "Credenciais inválidas.";
            public const string AccountLocked = "O acesso à conta está temporariamente indisponível. Tente novamente mais tarde.";
            public const string UserNotFound = "Usuário não encontrado.";
            public const string InvalidEmail = "Email inválido.";
            public const string EmailAlreadyVerified = "Este email já foi verificado.";
            public const string TooManyAttempts = "Muitas tentativas. Tente novamente em 1 hora.";
            public const string TokenInvalidOrExpired = "Token inválido ou expirado.";
            public const string RoleNotFound = "Role '{0}' não encontrada.";
            public const string TenantContextRequired = "Contexto do Tenant é obrigatório.";
            public const string UserContextRequired = "Contexto do usuário é obrigatório.";
            public const string UserAlreadyExists = "Usuário já existe.";
            public const string InviteInvalidOrExpired = "Convite inválido ou expirado.";
            public const string UserRegistrationFailed = "Falha ao registrar usuário: {0}";
            public const string GoogleUserCreationError = "Falha ao criar usuário Google: {0}";
            public const string UserCreationError = "Falha ao criar usuário: {0}";
            public const string RoleRequired = "Role é obrigatório.";
            public const string PasswordResetFailed = "Falha ao redefinir senha: {0}";
            public const string PasswordChangeFailed = "Falha ao alterar senha: {0}";
            public const string InvalidToken = "Token inválido.";
        }

        public static class CacheKeys
        {
            public const string ResendVerificationAttempts = "resend_verification_attempts:{0}";
            public const string ResetPasswordAttempts = "reset_password_attempts:{0}";
        }
    }
}
