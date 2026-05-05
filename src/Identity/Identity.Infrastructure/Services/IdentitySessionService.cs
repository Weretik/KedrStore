using Identity.Application.Features.Auth.SessionLogin.DTOs;
using Identity.Application.Features.Auth.SessionMe.DTOs;
using Identity.Application.Features.Auth.SessionRefresh.DTOs;
using Identity.Application.Services;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Options;

namespace Identity.Infrastructure.Services;

public sealed class IdentitySessionService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptionsMonitor,
    ILogger<IdentitySessionService> logger,
    IOptions<IdentitySessionPerformanceOptions> performanceOptions) : IIdentitySessionService
{
    private static readonly JsonSerializerOptions TokenJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<SessionLoginResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var stepWatch = Stopwatch.StartNew();
        var user = await userManager.FindByEmailAsync(email);
        LogStepDuration("Login.FindByEmail", stepWatch.ElapsedMilliseconds);
        if (user is null)
        {
            return null;
        }

        stepWatch.Restart();
        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        LogStepDuration("Login.CheckPassword", stepWatch.ElapsedMilliseconds);
        if (!signInResult.Succeeded)
        {
            return null;
        }

        stepWatch.Restart();
        var principal = await signInManager.CreateUserPrincipalAsync(user);
        LogStepDuration("Login.CreatePrincipal", stepWatch.ElapsedMilliseconds);
        stepWatch.Restart();
        var tokenPayload = await IssueTokenPayloadAsync(principal, cancellationToken);
        LogStepDuration("Login.IssueTokenPayload", stepWatch.ElapsedMilliseconds);
        if (tokenPayload is null)
        {
            return null;
        }

        return new SessionLoginResult(
            TokenType: tokenPayload.TokenType,
            AccessToken: tokenPayload.AccessToken,
            ExpiresIn: tokenPayload.ExpiresIn,
            RefreshToken: tokenPayload.RefreshToken);
    }

    public async Task<SessionRefreshResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var stepWatch = Stopwatch.StartNew();
        var options = bearerTokenOptionsMonitor.Get(IdentityConstants.BearerScheme);
        var refreshTicket = options.RefreshTokenProtector.Unprotect(refreshToken);
        LogStepDuration("Refresh.UnprotectToken", stepWatch.ElapsedMilliseconds);

        var expiresUtc = refreshTicket?.Properties?.ExpiresUtc;
        if (!expiresUtc.HasValue || expiresUtc.Value <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        var userId = refreshTicket?.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        stepWatch.Restart();
        var user = await userManager.FindByIdAsync(userId);
        LogStepDuration("Refresh.FindById", stepWatch.ElapsedMilliseconds);
        stepWatch.Restart();
        if (user is null || !await signInManager.CanSignInAsync(user))
        {
            return null;
        }
        LogStepDuration("Refresh.CanSignIn", stepWatch.ElapsedMilliseconds);

        stepWatch.Restart();
        var principal = await signInManager.CreateUserPrincipalAsync(user);
        LogStepDuration("Refresh.CreatePrincipal", stepWatch.ElapsedMilliseconds);
        stepWatch.Restart();
        var tokenPayload = await IssueTokenPayloadAsync(principal, cancellationToken);
        LogStepDuration("Refresh.IssueTokenPayload", stepWatch.ElapsedMilliseconds);
        if (tokenPayload is null)
        {
            return null;
        }

        return new SessionRefreshResult(
            TokenType: tokenPayload.TokenType,
            AccessToken: tokenPayload.AccessToken,
            ExpiresIn: tokenPayload.ExpiresIn,
            RefreshToken: tokenPayload.RefreshToken);
    }

    public async Task<SessionMeResult?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await GetCurrentUserEntityAsync(cancellationToken);
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);

        return new SessionMeResult(
            UserId: user.Id.ToString(),
            Email: user.Email ?? string.Empty,
            Roles: roles.ToArray());
    }

    public async Task<bool> LogoutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await GetCurrentUserEntityAsync(cancellationToken);
        if (user is null)
        {
            return false;
        }

        // Revokes existing refresh tokens tied to prior security stamp.
        await userManager.UpdateSecurityStampAsync(user);
        return true;
    }

    private async Task<IdentityBearerTokenPayload?> IssueTokenPayloadAsync(
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
            .ToDictionary(
                pair => pair.Key,
                pair => pair.Value);

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

    private sealed record IdentityBearerTokenPayload(
        string TokenType,
        string AccessToken,
        int ExpiresIn,
        string RefreshToken);

    private async Task<AppUser?> GetCurrentUserEntityAsync(CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !await signInManager.CanSignInAsync(user))
        {
            return null;
        }

        return user;
    }

    private void LogStepDuration(string step, long elapsedMilliseconds)
    {
        if (elapsedMilliseconds >= performanceOptions.Value.SlowStepThresholdMs &&
            logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Identity session step {Step} took {ElapsedMs} ms", step, elapsedMilliseconds);
        }
    }
}
