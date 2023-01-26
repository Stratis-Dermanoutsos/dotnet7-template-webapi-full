using webapi_full.Models;

namespace webapi_full.Entities.Request;

/// <summary>
/// The user's information.
/// </summary>
public class UserToCreate
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Convert the <see cref="UserToCreate" /> to a <see cref="User" />.
    /// </summary>
    /// <returns>The <see cref="User" />.</returns>
    public User ToUser() => new()
    {
        Email = this.Email,
        Username = this.Username,
        Password = this.Password,
        FirstName = this.FirstName,
        LastName = this.LastName
    };
}