namespace Ona.Auth.Infrastructure.Services
{
    public static class GuidGenerator
    {
        public static Guid NewSequentialGuid()
        {
            var guidArray = Guid.NewGuid().ToByteArray();
            var now = DateTime.UtcNow;
            var days = BitConverter.GetBytes(now.Ticks / TimeSpan.TicksPerDay);
            var msecs = BitConverter.GetBytes((long)(now.TimeOfDay.TotalMilliseconds / 3.333333));

            Array.Reverse(days);
            Array.Reverse(msecs);

            Array.Copy(days, days.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecs, msecs.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }
    }
}
