namespace webapi_full.Entities.Request;

/// <summary>
/// The user's login credentials.
/// </summary>
public class UserCredentials
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}