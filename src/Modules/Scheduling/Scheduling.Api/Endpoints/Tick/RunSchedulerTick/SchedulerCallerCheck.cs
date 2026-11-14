using Google.Apis.Auth;
using Microsoft.Extensions.Logging;

namespace Scheduling.Api.Endpoints.Tick.RunSchedulerTick;

/// <summary>Checks the bearer token of a tick call (platform/007 R-10).</summary>
public interface ISchedulerCallerCheck
{
    Task<bool> IsSchedulerAsync(string token, SchedulerTickOptions options);
}

/// <summary>A Google-signed OIDC token for the configured audience of the configured, verified service account.</summary>
internal sealed class GoogleSchedulerCallerCheck(ILogger<GoogleSchedulerCallerCheck> logger) : ISchedulerCallerCheck
{
    public async Task<bool> IsSchedulerAsync(string token, SchedulerTickOptions options)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [options.Audience!] });
            return payload.EmailVerified
                   && string.Equals(payload.Email, options.SchedulerServiceAccount, StringComparison.OrdinalIgnoreCase);
        }
        catch (InvalidJwtException exception)
        {
            logger.LogWarning("Scheduler tick refused: {Reason}", exception.Message);
            return false;
        }
    }
}
