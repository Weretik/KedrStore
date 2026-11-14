namespace Identity.Application.Features.Auth.Shared;

/// <summary>
/// Length limits of sign-in input. Only the shape is checked here: a wrong email or password stays 401, so the
/// answer does not tell whether an account exists.
/// </summary>
public static class LoginInputRules
{
    public const int MaxEmailLength = 256;
    public const int MaxPasswordLength = 512;
    public const int MaxRefreshTokenLength = 1024;
}
