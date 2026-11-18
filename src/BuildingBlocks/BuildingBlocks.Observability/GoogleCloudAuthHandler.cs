using System.Net.Http.Headers;
using Google.Apis.Auth.OAuth2;

namespace BuildingBlocks.Observability;

/// <summary>
/// Adds the bearer token of the Cloud Run runtime account to the trace exporter's
/// calls to the Telemetry API. The Google library caches and refreshes the token. The OTLP exporter sends with the
/// synchronous <see cref="HttpClient.Send(HttpRequestMessage)"/>, so both send paths add the token: without it the
/// Telemetry API answers 403 "unregistered callers" and no span is stored. On Cloud Run the credential is the
/// metadata server's, built directly (<see cref="RuntimeCredentialAsync"/>).
/// </summary>
public sealed class GoogleCloudAuthHandler(Func<CancellationToken, Task<string>> accessToken) : DelegatingHandler
{
    private const string CloudPlatformScope = "https://www.googleapis.com/auth/cloud-platform";

    public static GoogleCloudAuthHandler FromRuntimeCredentials()
    {
        var credential = new Lazy<Task<ITokenAccess>>(async () =>
            (await RuntimeCredentialAsync(Environment.GetEnvironmentVariable)).CreateScoped(CloudPlatformScope));

        return new GoogleCloudAuthHandler(async cancellationToken =>
            await (await credential.Value).GetAccessTokenForRequestAsync(cancellationToken: cancellationToken));
    }

    /// <summary>
    /// On Cloud Run (<c>K_SERVICE</c> or <c>CLOUD_RUN_JOB</c> is set) the metadata server's credential without the
    /// Application Default Credentials ping: that ping (3 × 500 ms) runs here on the exporter's background thread, where
    /// Cloud Run throttles the CPU, and a miss is cached for the whole process, so it also broke the scheduler's job
    /// launch. Elsewhere Application Default Credentials.
    /// </summary>
    public static async Task<GoogleCredential> RuntimeCredentialAsync(Func<string, string?> environment) =>
        IsCloudRun(environment)
            ? GoogleCredential.FromComputeCredential()
            : await GoogleCredential.GetApplicationDefaultAsync();

    public static bool IsCloudRun(Func<string, string?> environment) =>
        !string.IsNullOrWhiteSpace(environment("K_SERVICE")) || !string.IsNullOrWhiteSpace(environment("CLOUD_RUN_JOB"));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await accessToken(cancellationToken));
        return await base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // The exporter calls this from its own background export thread, so blocking on the cached token is safe.
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken(cancellationToken).GetAwaiter().GetResult());
        return base.Send(request, cancellationToken);
    }
}
