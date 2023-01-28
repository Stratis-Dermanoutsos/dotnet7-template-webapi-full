namespace webapi_full.Entities.Request;

/// <summary>
/// The old and new passwords and new password confirmation.
/// </summary>
public class PasswordToUpdate: PasswordConfirm
{
    public string OldPassword { get; set; } = string.Empty;
}