using Sales.Application.Abstractions.Integrations.OneC;

namespace Sales.Application.Abstractions.Integrations.OneC;

public interface ISalesOneCWriteClient
{
    Task<OneCOrderDeliveryResult> SendOrderAsync(
        OneCOrderDeliveryRequest request,
        CancellationToken cancellationToken);
}
