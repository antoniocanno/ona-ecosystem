namespace Ona.Core.Common.Enums
{
    [Flags]
    public enum Role
    {
        None = 0,
        Admin = 1,
        Manager = 2,
        Operator = 4,
        ReadOnly = 8
    }
}
