namespace Ona.ServiceDefaults.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SkipTenantValidationAttribute : Attribute
    {
    }
}
