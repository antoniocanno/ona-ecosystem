namespace Ona.Core.Common.Extensions
{
    public static class GuidExtension
    {
        public static Guid ToGuid(this string value)
        {
            return Guid.TryParse(value, out Guid result) ? result : Guid.Empty;
        }
    }
}
