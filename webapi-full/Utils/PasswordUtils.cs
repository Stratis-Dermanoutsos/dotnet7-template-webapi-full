using webapi_full.IUtils;

namespace webapi_full.Utils;

public class PasswordUtils : IPasswordUtils
{
    private readonly PasswordValidator validator;

    public PasswordUtils(PasswordValidator validator) => this.validator = validator;

    /// <inheritdoc />
    public string Encrypt(string value) => BCrypt.Net.BCrypt.HashPassword(value);

    /// <inheritdoc />
    public bool Check(string value, string encryptedValue) => BCrypt.Net.BCrypt.Verify(value, encryptedValue);
}