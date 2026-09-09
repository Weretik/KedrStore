using System.Globalization;
using Sales.Application.Integrations.OneC.Contracts;
using Sales.Application.Integrations.OneC.DTOs;
using OneCItem = BuildingBlocks.Integrations.OneC.Generated.Item;
using OneCRequestData = BuildingBlocks.Integrations.OneC.Generated.RequestData;

namespace Sales.Infrastructure.Integrations.OneC;

public sealed class SalesOneCWriteClient(
    IOneCSiteRequestSender sender,
    ILogger<SalesOneCWriteClient> logger) : ISalesOneCWriteClient
{
    public async Task<OneCOrderDeliveryResult> SendOrderAsync(
        OneCOrderDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var response = await sender.SendAsync(MapRequest(request), cancellationToken);
            var documentId = Normalize(response?.DocId, 128);

            if (!string.IsNullOrEmpty(documentId) && !IndicatesDocumentWasNotCreated(documentId))
            {
                logger.LogInformation(
                    "1C accepted order delivery {OrderNumber} with outcome {Outcome}",
                    request.OrderNumber,
                    OneCOrderDeliveryOutcome.Accepted);
                return OneCOrderDeliveryResult.Accepted(documentId, diagnostic: null);
            }

            var diagnostic = string.IsNullOrEmpty(documentId)
                ? "OneCRejectedRequest"
                : "OneCDocumentWasNotCreated";

            logger.LogWarning(
                "1C rejected order delivery {OrderNumber} with outcome {Outcome} and diagnostic {Diagnostic}",
                request.OrderNumber,
                OneCOrderDeliveryOutcome.BusinessError,
                diagnostic);
            return OneCOrderDeliveryResult.BusinessError(diagnostic);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            var diagnostic = Normalize(exception.GetType().Name, 128) ?? "SoapTransportFailure";
            logger.LogWarning(
                "1C transport failure for order delivery {OrderNumber} with outcome {Outcome} and diagnostic {Diagnostic}",
                request.OrderNumber,
                OneCOrderDeliveryOutcome.TransportError,
                diagnostic);
            return OneCOrderDeliveryResult.TransportError(diagnostic);
        }
    }

    private static OneCRequestData MapRequest(OneCOrderDeliveryRequest request)
        => new()
        {
            CounterpartyId = request.CounterpartyId,
            OrderId = request.OrderNumber,
            Date = request.CreatedAtUtc.UtcDateTime,
            Comment = request.Comment ?? string.Empty,
            Items = request.Lines.Select(line => new OneCItem
            {
                ProductId = NormalizeProductId(line.ProductId),
                Quantity = line.Quantity.ToString(CultureInfo.InvariantCulture),
                Amount = line.Amount
            }).ToArray()
        };

    private static string NormalizeProductId(string productId)
    {
        var normalized = productId.Trim();
        return normalized.All(char.IsAsciiDigit) ? normalized.PadLeft(9, '0') : normalized;
    }

    private static bool IndicatesDocumentWasNotCreated(string documentId)
        => documentId.StartsWith("Не создан", StringComparison.OrdinalIgnoreCase);

    private static string? Normalize(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = new string(value
            .Trim()
            .Where(character => !char.IsControl(character))
            .ToArray());

        return normalized[..Math.Min(normalized.Length, maximumLength)];
    }
}
