using Ona.Core.Common.Enums;

namespace Ona.Core.Common.Extensions
{
    public static class RoleExtensions
    {
        public static IEnumerable<Role> GetHierarchy(this Role role)
        {
            return (int)role switch
            {
                (int)Role.ReadOnly => [Role.ReadOnly, Role.Operator, Role.Manager, Role.Admin],
                (int)Role.Operator => [Role.Operator, Role.Manager, Role.Admin],
                (int)Role.Manager => [Role.Manager, Role.Admin],
                (int)Role.Admin => [Role.Admin],
                _ => [role]
            };
        }
    }
}
