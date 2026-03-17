using Microsoft.AspNetCore.Authorization;
using Ona.Core.Common.Enums;
using Ona.Core.Common.Extensions;
using System.Data;

namespace Ona.ServiceDefaults.Attributes
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params Role[] roles)
        {
            if (roles.Length == 0)
                return;

            var allAllowed = roles
                .SelectMany(r => r.GetHierarchy())
                .Select(r => r.ToString())
                .Distinct();

            Roles = string.Join(",", allAllowed);
        }
    }
}
