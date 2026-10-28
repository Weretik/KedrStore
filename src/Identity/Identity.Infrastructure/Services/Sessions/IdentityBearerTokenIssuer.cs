using System.Buffers.Text;
using System.Security.Cryptography;

namespace Identity.Infrastructure.Services.Sessions;

/// <summary>
/// Issues a bearer access token with the bearer scheme's protector and a random refresh token.
/// The refresh token is only a lookup key: its hash identifies the refresh session.
/// </summary>
public sealed class IdentityBearerTokenIssuer(IOptionsMonitor<BearerTokenOptions> bearerTokenOptionsMonitor)
{
    private const int RefreshTokenBytes = 32;

    public IdentityBearerTokenPayload Issue(ClaimsPrincipal principal)
    {
        var options = bearerTokenOptionsMonitor.Get(IdentityConstants.BearerScheme);
        var now = (options.TimeProvider ?? TimeProvider.System).GetUtcNow();
        var properties = new AuthenticationProperties
        {
            IssuedUtc = now,
            ExpiresUtc = now + options.BearerTokenExpiration
        };

        var accessToken = options.BearerTokenProtector.Protect(
            new AuthenticationTicket(principal, properties, IdentityConstants.BearerScheme));
        var refreshToken = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(RefreshTokenBytes));

        return new IdentityBearerTokenPayload(
            TokenType: "Bearer",
            AccessToken: accessToken,
            ExpiresIn: (int)options.BearerTokenExpiration.TotalSeconds,
            RefreshToken: refreshToken);
    }
}

public sealed record IdentityBearerTokenPayload(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);
