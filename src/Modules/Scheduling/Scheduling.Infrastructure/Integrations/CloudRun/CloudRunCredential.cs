using Google.Apis.Auth.OAuth2;

namespace Scheduling.Infrastructure.Integrations.CloudRun;

/// <summary>
/// The API's own service account for the Cloud Run Admin API. On Cloud Run (<c>K_SERVICE</c> or <c>CLOUD_RUN_JOB</c>
/// is set) it is the metadata server's credential, built directly: Application Default Credentials first ping the
/// metadata server for 3 × 500 ms and cache a miss for the life of the process, so one slow ping on a throttled
/// instance turned every later launch into "Your default credentials were not found". Elsewhere (a developer's
/// machine) it stays Application Default Credentials.
/// </summary>
internal static class CloudRunCredential
{
    public static bool IsCloudRun(Func<string, string?> environment) =>
        !string.IsNullOrWhiteSpace(environment("K_SERVICE")) || !string.IsNullOrWhiteSpace(environment("CLOUD_RUN_JOB"));

    public static async Task<GoogleCredential> GetAsync(Func<string, string?> environment, CancellationToken cancellationToken) =>
        IsCloudRun(environment)
            ? GoogleCredential.FromComputeCredential()
            : await GoogleCredential.GetApplicationDefaultAsync(cancellationToken);
}
