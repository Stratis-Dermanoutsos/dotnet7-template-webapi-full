using Microsoft.AspNetCore.Authorization;
using webapi_full.Enums;

namespace webapi_full.Utils;

/// <summary>
/// This class is used to define a role requirement for authorization.
/// </summary>
public class RoleRequirement : IAuthorizationRequirement
{
    public RoleRequirement(Role requiredRole) => this.RequiredRole = requiredRole;

    public Role RequiredRole { get; set; }
}