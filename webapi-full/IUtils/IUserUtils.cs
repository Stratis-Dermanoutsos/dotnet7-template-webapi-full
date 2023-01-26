using System.Security.Claims;
using webapi_full.Models;

namespace webapi_full.IUtils;

public interface IUserUtils
{
    /// <summary>
    /// Get the Id of the currently signed in user.
    /// </summary>
    int GetLoggedUserId(ClaimsPrincipal principal);

    /// <summary>
    /// Get a user by email.
    /// <br/>
    /// <paramref name="email" />: The email of the user user.
    /// <returns>Returns the user object.</returns>
    /// </summary>
    User? GetByEmail(string email);
}