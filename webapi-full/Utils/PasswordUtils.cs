using webapi_full.IUtils;

namespace webapi_full.Utils;

public class PasswordUtils : IPasswordUtils
{
    /// <inheritdoc />
    public string Encrypt(string value) => BCrypt.Net.BCrypt.HashPassword(value);

    /// <inheritdoc />
    public bool Check(string value, string encryptedValue) => BCrypt.Net.BCrypt.Verify(value, encryptedValue);
}