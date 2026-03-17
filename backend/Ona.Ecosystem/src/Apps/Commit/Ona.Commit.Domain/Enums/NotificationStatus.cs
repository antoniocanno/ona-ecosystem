namespace Ona.Commit.Domain.Enums
{
    public enum NotificationStatus
    {
        Scheduled = 0,      // Está no Hangfire
        Sent = 1,           // Enviado para API do Whats
        Delivered = 2,      // Chegou no celular (dois tiques cinzas)
        Read = 3,           // Cliente leu (dois tiques azuis)
        Failed = 4,         // Erro na API ou número inválido
        Replied = 5         // Cliente respondeu essa mensagem específica
    }
}
