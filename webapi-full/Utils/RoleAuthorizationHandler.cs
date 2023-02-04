using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace webapi_full.Utils;

/// <summary>
/// Authorization handler for roles.
/// <br/>
/// See <see href="https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies">https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies</see> for Policy-based authorization in ASP.NET Core.
/// <br/>
/// Also, see <see href="https://jakeydocs.readthedocs.io/en/latest/security/authorization/policies.html">https://jakeydocs.readthedocs.io/en/latest/security/authorization/policies.html</see> for Custom Policy-Based Authorization.
/// </summary>
public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        //* Get user and check if authenticated.
        var user = context.User;
        if (user.Identity is null || !user.Identity.IsAuthenticated) {
            context.Fail();
            return Task.CompletedTask;
        }

        //* Get user's role.
        string? roleValue = user?.FindFirst(claim => claim.Type == ClaimTypes.Role)?.Value;
        if (roleValue is null) {
            context.Fail();
            return Task.CompletedTask;
        }

        //* Check if user has sufficient rights for the requested resource.
        bool parsed = int.TryParse(roleValue, out int userRole);
        if (!parsed || userRole < (int)requirement.RequiredRole) {
            context.Fail();
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}