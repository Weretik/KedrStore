using System.Net;
using BuildingBlocks.Observability;

namespace UnitTests.BuildingBlocks.Observability;

public sealed class GoogleCloudAuthHandlerTests
{
    [Fact]
    public async Task Request_CarriesTheRuntimeAccountToken()
    {
        var inner = new RecordingHandler();
        using var client = new HttpClient(new GoogleCloudAuthHandler(_ => Task.FromResult("token-1")) { InnerHandler = inner });

        await client.PostAsync("https://telemetry.googleapis.com/v1/traces", new ByteArrayContent([]));

        Assert.Equal("Bearer", inner.Authorization?.Scheme);
        Assert.Equal("token-1", inner.Authorization?.Parameter);
    }

    [Fact]
    public void SynchronousRequest_CarriesTheToken_TooBecauseTheOtlpExporterSendsSynchronously()
    {
        var inner = new RecordingHandler();
        using var client = new HttpClient(new GoogleCloudAuthHandler(_ => Task.FromResult("token-2")) { InnerHandler = inner });

        client.Send(new HttpRequestMessage(HttpMethod.Post, "https://telemetry.googleapis.com/v1/traces") { Content = new ByteArrayContent([]) });

        Assert.Equal("Bearer", inner.Authorization?.Scheme);
        Assert.Equal("token-2", inner.Authorization?.Parameter);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public System.Net.Http.Headers.AuthenticationHeaderValue? Authorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization;
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
