using webapi_full.IUtils;

namespace webapi_full.Utils;

public class EncryptionUtils : IEncryptionUtils
{
    /// <inheritdoc />
    public string Encrypt(string value) => BCrypt.Net.BCrypt.HashPassword(value);

    /// <inheritdoc />
    public bool Check(string value, string encryptedValue) => BCrypt.Net.BCrypt.Verify(value, encryptedValue);
}