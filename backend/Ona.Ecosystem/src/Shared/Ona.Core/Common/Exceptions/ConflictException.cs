namespace Ona.Core.Common.Exceptions
{
    /// <summary>
    /// Exceção lançada quando há conflito de recursos (ex: registro duplicado).
    /// Retorna HTTP 409 Conflict.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
        public ConflictException(string message, Exception innerException) : base(message, innerException) { }
    }
}
