using System.Text;
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

    /// <inheritdoc />
    public void Validate(string value)
    {
        StringBuilder errorMessage = new();
        errorMessage.Append("<ul class='password-validation'>");

        //? Not allow whitespace
        errorMessage.Append("<li class='");
        errorMessage.Append(value.Any(Char.IsWhiteSpace) ? "invalid" : "valid");
        errorMessage.Append("'>Password cannot contain whitespaces.</li>");

        //? Allowed only specific non-alphanumeric
        if (!string.IsNullOrWhiteSpace(validator.AllowedNonAlphanumeric) && !value.All(char.IsLetterOrDigit)) {
            errorMessage.Append("<li class='");
            errorMessage.Append(
                value.Any(character => !char.IsLetterOrDigit(character)
                && !validator.AllowedNonAlphanumeric.Contains(character))
                    ? "invalid"
                    : "valid");
            errorMessage.Append($"'>The only allowed special characters are the following: {string.Join(", ", validator.AllowedNonAlphanumeric.ToCharArray())}</li>");
        }

        //? Maximum length
        if (validator.MaxLength > 0) {
            errorMessage.Append("<li class='");
            errorMessage.Append(value.Length > validator.MaxLength ? "invalid" : "valid");
            errorMessage.Append($"'>Password cannot exceed {validator.MaxLength} characters.</li>");
        }

        //? Minimum length
        if (validator.MinLength > 0) {
            errorMessage.Append("<li class='");
            errorMessage.Append(value.Length < validator.MinLength ? "invalid" : "valid");
            errorMessage.Append($"'>Password must be at least {validator.MinLength} characters long.</li>");
        }

        //? Require digit
        if (validator.RequireDigit) {
            errorMessage.Append("<li class='");
            errorMessage.Append(!value.Any(char.IsDigit) ? "invalid" : "valid");
            errorMessage.Append("'>Password must contain at least one digit.</li>");
        }

        //? Require lowercase
        if (validator.RequireLowercase) {
            errorMessage.Append("<li class='");
            errorMessage.Append(!value.Any(char.IsLower) ? "invalid" : "valid");
            errorMessage.Append("'>Password must contain at least one lowercase letter.</li>");
        }

        //? Require non-alphanumeric
        if (validator.RequireNonAlphanumeric) {
            errorMessage.Append("<li class='");
            errorMessage.Append(
                !string.IsNullOrWhiteSpace(validator.AllowedNonAlphanumeric)
                && value.All(char.IsLetterOrDigit)
                    ? "invalid"
                    : "valid");
            errorMessage.Append($"'>Password must contain at least one of the following characters: {string.Join(", ", validator.AllowedNonAlphanumeric.ToCharArray())}</li>");
        }

        //? Require uppercase
        if (validator.RequireUppercase) {
            errorMessage.Append("<li class='");
            errorMessage.Append(!value.Any(char.IsUpper) ? "invalid" : "valid");
            errorMessage.Append("'>Password must contain at least one uppercase letter.</li>");
        }

        errorMessage.Append("</ul>");
        if (errorMessage.ToString().Contains("invalid"))
            throw new ArgumentException(errorMessage.ToString());
    }
}