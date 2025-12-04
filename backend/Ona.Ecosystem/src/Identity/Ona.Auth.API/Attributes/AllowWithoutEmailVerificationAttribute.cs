namespace Ona.Auth.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AllowWithoutEmailVerificationAttribute : Attribute
    {
    }
}
