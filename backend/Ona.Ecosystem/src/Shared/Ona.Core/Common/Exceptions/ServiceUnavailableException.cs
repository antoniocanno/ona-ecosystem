namespace Ona.Core.Common.Exceptions
{
    /// <summary>
    /// Exceção lançada quando um serviço está temporariamente indisponível.
    /// Retorna HTTP 503 Service Unavailable.
    /// </summary>
    public class ServiceUnavailableException : Exception
    {
        /// <summary>
        /// Tempo sugerido para retry em segundos.
        /// </summary>
        public int? RetryAfterSeconds { get; }

        public ServiceUnavailableException(string message, int? retryAfterSeconds = null)
            : base(message)
        {
            RetryAfterSeconds = retryAfterSeconds;
        }

        public ServiceUnavailableException(string message, Exception innerException, int? retryAfterSeconds = null)
            : base(message, innerException)
        {
            RetryAfterSeconds = retryAfterSeconds;
        }
    }
}
