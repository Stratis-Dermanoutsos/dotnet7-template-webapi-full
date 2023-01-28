namespace webapi_full.Utils;

public class PasswordValidator
{
    public string AllowedNonAlphanumeric { get; init; } = string.Empty;
    public int MaxLength { get; init; } = 0;
    public int MinLength { get; init; } = 0;
    public bool RequireDigit { get; init; } = false;
    public bool RequireLowercase { get; init; } = false;
    public bool RequireNonAlphanumeric { get; init; } = false;
    public bool RequireUppercase { get; init; } = false;
}