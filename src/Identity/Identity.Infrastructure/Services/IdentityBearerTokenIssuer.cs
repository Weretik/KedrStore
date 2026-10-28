namespace Identity.Infrastructure.Services;

/// <summary>
/// Issues bearer access and refresh tokens by running the bearer sign-in handler into a buffer.
/// The handler writes the token JSON to the response, so the real response is restored afterwards.
/// </summary>
public sealed class IdentityBearerTokenIssuer(IHttpContextAccessor httpContextAccessor)
{
    private static readonly JsonSerializerOptions TokenJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IdentityBearerTokenPayload?> IssueAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        await using var buffer = new MemoryStream();
        var originalBody = httpContext.Response.Body;
        var originalStatusCode = httpContext.Response.StatusCode;
        var originalContentType = httpContext.Response.ContentType;
        var originalContentLength = httpContext.Response.ContentLength;
        var originalHeaders = httpContext.Response.Headers
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        httpContext.Response.Body = buffer;

        try
        {
            await httpContext.SignInAsync(IdentityConstants.BearerScheme, principal);
            buffer.Position = 0;

            return await JsonSerializer.DeserializeAsync<IdentityBearerTokenPayload>(
                buffer,
                TokenJsonOptions,
                cancellationToken);
        }
        finally
        {
            httpContext.Response.Body = originalBody;
            httpContext.Response.StatusCode = originalStatusCode;
            httpContext.Response.ContentType = originalContentType;
            httpContext.Response.ContentLength = originalContentLength;
            httpContext.Response.Headers.Clear();

            foreach (var (key, value) in originalHeaders)
            {
                httpContext.Response.Headers[key] = value;
            }
        }
    }
}

public sealed record IdentityBearerTokenPayload(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);
