namespace Ona.Commit.Domain.Exceptions
{
    public abstract class WhatsAppIntegrationException : Exception
    {
        protected WhatsAppIntegrationException(string message) : base(message) { }
        protected WhatsAppIntegrationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class WhatsAppTransientException : WhatsAppIntegrationException
    {
        public WhatsAppTransientException(string message) : base(message) { }
        public WhatsAppTransientException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class WhatsAppPermanentException : WhatsAppIntegrationException
    {
        public WhatsAppPermanentException(string message) : base(message) { }
        public WhatsAppPermanentException(string message, Exception innerException) : base(message, innerException) { }
    }
}
