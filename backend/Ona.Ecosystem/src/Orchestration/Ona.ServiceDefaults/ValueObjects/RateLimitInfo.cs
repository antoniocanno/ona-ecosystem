namespace Ona.ServiceDefaults.ValueObjects
{
    public class RateLimitInfo
    {
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
    }
}
