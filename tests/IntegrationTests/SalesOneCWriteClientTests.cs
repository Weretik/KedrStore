using System.Collections.Concurrent;
using BuildingBlocks.Integrations.OneC.Generated;
using Microsoft.Extensions.Logging;
using Sales.Application.Integrations.OneC.DTOs;
using Sales.Infrastructure.Integrations.OneC;

namespace IntegrationTests;

public sealed class SalesOneCWriteClientTests
{
    [Fact]
    public async Task SendOrderAsync_MapsRequestAndReturnsAccepted()
    {
        var sender = new FakeSiteRequestSender(_ => new RequestDataOut
        {
            DocId = "1C-123",
            Comment = " accepted\n"
        });
        using var loggerProvider = new LogCollector();
        var client = CreateClient(sender, loggerProvider);

        var result = await client.SendOrderAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(OneCOrderDeliveryOutcome.Accepted, result.Outcome);
        Assert.Equal("1C-123", result.OneCDocumentId);
        Assert.Null(result.Diagnostic);
        Assert.NotNull(sender.Request);
        Assert.Equal("counterparty-1", sender.Request!.CounterpartyId);
        Assert.Equal("SO-20260818-0001", sender.Request.OrderId);
        Assert.Equal(new DateTime(2026, 8, 18, 10, 30, 0, DateTimeKind.Utc), sender.Request.Date);
        Assert.Equal("manager comment", sender.Request.Comment);
        var item = Assert.Single(sender.Request.Items);
        Assert.Equal("000008190", item.ProductId);
        Assert.Equal("2", item.Quantity);
        Assert.Equal(12500.50m, item.Amount);
    }

    [Fact]
    public async Task SendOrderAsync_ReturnsBusinessErrorAndDoesNotLogRequestData()
    {
        var sender = new FakeSiteRequestSender(_ => new RequestDataOut
        {
            DocId = "",
            Comment = "Unknown product"
        });
        using var loggerProvider = new LogCollector();
        var client = CreateClient(sender, loggerProvider);
        var request = CreateRequest() with
        {
            CounterpartyId = "sensitive-counterparty",
            Comment = "sensitive-request-comment",
            Lines = [new OneCOrderDeliveryLineDto("sensitive-product", 1, 1m)]
        };

        var result = await client.SendOrderAsync(request, CancellationToken.None);

        Assert.Equal(OneCOrderDeliveryOutcome.BusinessError, result.Outcome);
        Assert.Equal("OneCRejectedRequest", result.Diagnostic);
        Assert.Contains("Unknown product", loggerProvider.Messages);
        Assert.DoesNotContain("sensitive-counterparty", loggerProvider.Messages);
        Assert.DoesNotContain("sensitive-request-comment", loggerProvider.Messages);
        Assert.DoesNotContain("sensitive-product", loggerProvider.Messages);
    }

    [Fact]
    public async Task SendOrderAsync_ReturnsBusinessErrorWhenOneCReportsDocumentWasNotCreated()
    {
        var oneCInternalError = "Error writing D:\\1C_BASES\\Protocol8\\internal.txt";
        var sender = new FakeSiteRequestSender(_ => new RequestDataOut
        {
            DocId = "Не создан! Ошибка записи документа.",
            Comment = oneCInternalError
        });
        using var loggerProvider = new LogCollector();
        var client = CreateClient(sender, loggerProvider);

        var result = await client.SendOrderAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(OneCOrderDeliveryOutcome.BusinessError, result.Outcome);
        Assert.Null(result.OneCDocumentId);
        Assert.Equal("OneCDocumentWasNotCreated", result.Diagnostic);
        Assert.Contains("Не создан", loggerProvider.Messages);
        Assert.Contains(oneCInternalError, loggerProvider.Messages);
    }

    [Fact]
    public async Task SendOrderAsync_ReturnsTransportErrorForTimeout()
    {
        var sender = new FakeSiteRequestSender(_ => throw new TimeoutException());
        using var loggerProvider = new LogCollector();
        var client = CreateClient(sender, loggerProvider);

        var result = await client.SendOrderAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(OneCOrderDeliveryOutcome.TransportError, result.Outcome);
        Assert.Equal("TimeoutException", result.Diagnostic);
    }

    [Fact]
    public async Task SendOrderAsync_ReturnsTransportErrorForNetworkFailure()
    {
        var sender = new FakeSiteRequestSender(_ => throw new HttpRequestException());
        using var loggerProvider = new LogCollector();
        var client = CreateClient(sender, loggerProvider);

        var result = await client.SendOrderAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(OneCOrderDeliveryOutcome.TransportError, result.Outcome);
        Assert.Equal("HttpRequestException", result.Diagnostic);
    }

    private static SalesOneCWriteClient CreateClient(FakeSiteRequestSender sender, LogCollector loggerProvider)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(loggerProvider));
        return new SalesOneCWriteClient(sender, loggerFactory.CreateLogger<SalesOneCWriteClient>());
    }

    private static OneCOrderDeliveryRequest CreateRequest()
        => new(
            CounterpartyId: "counterparty-1",
            OrderNumber: "SO-20260818-0001",
            CreatedAtUtc: new DateTimeOffset(2026, 8, 18, 10, 30, 0, TimeSpan.Zero),
            Comment: "manager comment",
            Lines: [new OneCOrderDeliveryLineDto("8190", 2, 12500.50m)]);

    private sealed class FakeSiteRequestSender(Func<RequestData, RequestDataOut?> send) : IOneCSiteRequestSender
    {
        public RequestData? Request { get; private set; }

        public Task<RequestDataOut?> SendAsync(RequestData request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(send(request));
        }
    }

    private sealed class LogCollector : ILoggerProvider
    {
        private readonly ConcurrentQueue<string> _messages = new();
        public string Messages => string.Join(Environment.NewLine, _messages);

        public ILogger CreateLogger(string categoryName) => new CollectorLogger(_messages);
        public void Dispose() { }

        private sealed class CollectorLogger(ConcurrentQueue<string> messages) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
                => messages.Enqueue(formatter(state, exception));
        }
    }
}
