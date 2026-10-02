using System.Net;
using System.Net.Http.Headers;
using Catalog.Application.Jobs.ProductPhotos;
using Catalog.Domain.Enums;
using Catalog.Infrastructure.Integrations.ProductPhotos;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Catalog.ProductPhotos;

public sealed class ProductPhotoProbeTests
{
    [Fact]
    public async Task ProbeAsync_ImageHead_IsAvailableWithoutReadingBody()
    {
        var content = new TrackingContent("image/jpeg");
        var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content
        }));
        var probe = CreateProbe(handler);

        var result = await probe.ProbeAsync(PhotoUrl, CancellationToken.None);

        Assert.Equal(ProductPhotoStatus.Available, result.Status);
        Assert.Equal(200, result.HttpStatusCode);
        Assert.Equal([HttpMethod.Head], handler.Methods);
        Assert.False(content.WasSerialized);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Gone)]
    public async Task ProbeAsync_MissingStatus_IsMissing(HttpStatusCode statusCode)
    {
        var probe = CreateProbe(new RecordingHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(statusCode))));

        var result = await probe.ProbeAsync(PhotoUrl, CancellationToken.None);

        Assert.Equal(ProductPhotoStatus.Missing, result.Status);
        Assert.Equal((int)statusCode, result.HttpStatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("text/html")]
    public async Task ProbeAsync_SuccessWithoutImageMediaType_IsInvalidContentType(string? mediaType)
    {
        var handler = new RecordingHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            if (mediaType is not null)
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return Task.FromResult(response);
        });

        var result = await CreateProbe(handler).ProbeAsync(PhotoUrl, CancellationToken.None);

        Assert.Equal(ProductPhotoStatus.InvalidContentType, result.Status);
        Assert.Equal("invalid_content_type", result.DiagnosticCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.NotImplemented)]
    public async Task ProbeAsync_UnsupportedHead_FallsBackToHeaderOnlyGet(HttpStatusCode headStatus)
    {
        var content = new TrackingContent("image/png");
        var handler = new RecordingHandler((request, _) => Task.FromResult(
            request.Method == HttpMethod.Head
                ? new HttpResponseMessage(headStatus)
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = content }));

        var result = await CreateProbe(handler).ProbeAsync(PhotoUrl, CancellationToken.None);

        Assert.Equal(ProductPhotoStatus.Available, result.Status);
        Assert.Equal([HttpMethod.Head, HttpMethod.Get], handler.Methods);
        Assert.False(content.WasSerialized);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task ProbeAsync_TechnicalHttpFailure_IsCheckFailed(HttpStatusCode statusCode)
    {
        var probe = CreateProbe(new RecordingHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(statusCode))));

        var result = await probe.ProbeAsync(PhotoUrl, CancellationToken.None);

        Assert.Equal(ProductPhotoStatus.CheckFailed, result.Status);
        Assert.Equal($"http_{(int)statusCode}", result.DiagnosticCode);
    }

    [Fact]
    public async Task ProbeAsync_DisallowedHost_DoesNotSendRequest()
    {
        var handler = new RecordingHandler((_, _) => throw new InvalidOperationException("Request must not be sent."));

        var result = await CreateProbe(handler).ProbeAsync("https://example.test/product.jpg", CancellationToken.None);

        Assert.Equal(ProductPhotoStatus.CheckFailed, result.Status);
        Assert.Equal("invalid_or_disallowed_url", result.DiagnosticCode);
        Assert.Empty(handler.Methods);
    }

    [Fact]
    public async Task ProbeAsync_RedirectOutsideAllowedHost_IsCheckFailed()
    {
        var handler = new RecordingHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Found);
            response.Headers.Location = new Uri("https://example.test/product.jpg");
            return Task.FromResult(response);
        });

        var result = await CreateProbe(handler).ProbeAsync(PhotoUrl, CancellationToken.None);

        Assert.Equal(ProductPhotoStatus.CheckFailed, result.Status);
        Assert.Equal("network_error", result.DiagnosticCode);
    }

    [Fact]
    public async Task ProbeAsync_TransportCancellationWithoutCallerCancellation_IsTimeout()
    {
        var handler = new RecordingHandler((_, _) => throw new OperationCanceledException());

        var result = await CreateProbe(handler).ProbeAsync(PhotoUrl, CancellationToken.None);

        Assert.Equal(ProductPhotoStatus.CheckFailed, result.Status);
        Assert.Equal("timeout", result.DiagnosticCode);
    }

    [Fact]
    public async Task ProbeAsync_CallerCancellation_IsPropagated()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var handler = new RecordingHandler((_, token) => throw new OperationCanceledException(token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateProbe(handler).ProbeAsync(PhotoUrl, source.Token));
    }

    private const string PhotoUrl = "https://images-kedr.cdn.express/products/7.jpg";

    private static ProductPhotoProbe CreateProbe(HttpMessageHandler handler)
        => new(
            new HttpClient(handler),
            Options.Create(new ProductPhotoCheckOptions()));

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        : HttpMessageHandler
    {
        public List<HttpMethod> Methods { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Methods.Add(request.Method);
            return responseFactory(request, cancellationToken);
        }
    }

    private sealed class TrackingContent : HttpContent
    {
        public TrackingContent(string mediaType)
        {
            Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        }

        public bool WasSerialized { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            WasSerialized = true;
            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 1024;
            return true;
        }
    }
}
