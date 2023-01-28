using webapi_full.Models;

namespace webapi_full.Entities.Request;

/// <summary>
/// The user's information.
/// </summary>
public class UserToUpdate
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Merge the <see cref="UserToUpdate" /> to a <see cref="User" />.
    /// </summary>
    /// <returns>The <see cref="User" />.</returns>
    public User MergeToUser(User user)
    {
        user.Email = this.Email;
        user.UserName = this.UserName;
        user.FirstName = this.FirstName;
        user.LastName = this.LastName;

        return user;
    }
}