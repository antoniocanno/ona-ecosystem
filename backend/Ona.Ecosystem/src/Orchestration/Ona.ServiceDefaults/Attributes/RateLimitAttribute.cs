namespace Ona.ServiceDefaults.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RateLimitAttribute : Attribute
    {
        public int MaxRequests { get; }
        public int WindowMinutes { get; }
        public string Scope { get; }

        /// <summary>
        /// Define uma regra de Rate Limit para um endpoint.
        /// </summary>
        /// <param name="maxRequests">Número máximo de requisições permitidas.</param>
        /// <param name="windowMinutes">Janela de tempo em minutos para o limite.</param>
        /// <param name="scope">Identificador de escopo (ex: "global", "write", "users").</param>
        public RateLimitAttribute(int maxRequests, int windowMinutes, string scope = "global")
        {
            MaxRequests = maxRequests;
            WindowMinutes = windowMinutes;
            Scope = scope;
        }
    }
}
