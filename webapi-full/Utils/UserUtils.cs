using System.Security.Claims;
using webapi_full.IUtils;
using webapi_full.Models;

namespace webapi_full.Utils;

public class UserUtils : IUserUtils
{
    private readonly ApplicationDbContext context;

    public UserUtils(ApplicationDbContext context) => this.context = context;

    public int GetLoggedUserId(ClaimsPrincipal principal)
    {
        ClaimsIdentity? claimsIdentity = principal.Identity as ClaimsIdentity;

        return Convert.ToInt32(claimsIdentity?.FindFirst("Id")?.Value);
    }

    public User? GetByEmail(string email) => this.context.Users.FirstOrDefault(user => user.Email == email);
}