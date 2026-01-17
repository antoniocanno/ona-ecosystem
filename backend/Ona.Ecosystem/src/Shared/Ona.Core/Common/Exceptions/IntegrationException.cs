namespace Ona.Core.Common.Exceptions
{
    /// <summary>
    /// Exceção base para erros de integração com serviços externos.
    /// Usada quando há falhas na comunicação com APIs externas.
    /// </summary>
    public class IntegrationException : Exception
    {
        /// <summary>
        /// Nome do serviço de integração que falhou.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Indica se o erro é transitório e pode ser retentado.
        /// </summary>
        public bool IsTransient { get; }

        public IntegrationException(string serviceName, string message, bool isTransient = false)
            : base(message)
        {
            ServiceName = serviceName;
            IsTransient = isTransient;
        }

        public IntegrationException(string serviceName, string message, Exception innerException, bool isTransient = false)
            : base(message, innerException)
        {
            ServiceName = serviceName;
            IsTransient = isTransient;
        }
    }
}
