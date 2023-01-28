namespace webapi_full.Entities.Request;

/// <summary>
/// The new password and new password confirmation.
/// </summary>
public class PasswordConfirm
{
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirmation { get; set; } = string.Empty;
}