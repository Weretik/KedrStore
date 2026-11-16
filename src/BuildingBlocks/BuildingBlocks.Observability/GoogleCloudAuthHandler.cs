using System.Net.Http.Headers;
using Google.Apis.Auth.OAuth2;

namespace BuildingBlocks.Observability;

/// <summary>
/// Adds the bearer token of the Cloud Run runtime account (Application Default Credentials) to the trace exporter's
/// calls to the Telemetry API. The Google library caches and refreshes the token. The OTLP exporter sends with the
/// synchronous <see cref="HttpClient.Send(HttpRequestMessage)"/>, so both send paths add the token: without it the
/// Telemetry API answers 403 "unregistered callers" and no span is stored.
/// </summary>
public sealed class GoogleCloudAuthHandler(Func<CancellationToken, Task<string>> accessToken) : DelegatingHandler
{
    private const string CloudPlatformScope = "https://www.googleapis.com/auth/cloud-platform";

    public static GoogleCloudAuthHandler FromApplicationDefaultCredentials()
    {
        var credential = new Lazy<Task<ITokenAccess>>(async () =>
            (await GoogleCredential.GetApplicationDefaultAsync()).CreateScoped(CloudPlatformScope));

        return new GoogleCloudAuthHandler(async cancellationToken =>
            await (await credential.Value).GetAccessTokenForRequestAsync(cancellationToken: cancellationToken));
    }

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
