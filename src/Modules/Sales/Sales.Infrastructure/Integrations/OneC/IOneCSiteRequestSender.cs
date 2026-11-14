using BuildingBlocks.Integrations.OneC.Generated;

namespace Sales.Infrastructure.Integrations.OneC;

public interface IOneCSiteRequestSender
{
    Task<RequestDataOut?> SendAsync(RequestData request, CancellationToken cancellationToken);
}
