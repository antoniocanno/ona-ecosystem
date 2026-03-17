namespace Ona.Core.Common.Exceptions
{
    /// <summary>
    /// Exceção para erros de configuração da aplicação.
    /// Usada quando configurações obrigatórias estão faltando ou são inválidas.
    /// Esta exceção NÃO deve ter sua mensagem exposta ao usuário final.
    /// </summary>
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message) { }
        public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
