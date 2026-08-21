using Sales.Application.Integrations.OneC.DTOs;

namespace Sales.Application.Integrations.OneC.Contracts;

public interface ISalesOneCWriteClient
{
    Task<OneCOrderDeliveryResult> SendOrderAsync(
        OneCOrderDeliveryRequest request,
        CancellationToken cancellationToken);
}
