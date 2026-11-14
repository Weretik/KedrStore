using BuildingBlocks.Integrations.OneC.Factory;
using BuildingBlocks.Integrations.OneC.Generated;

namespace Sales.Infrastructure.Integrations.OneC;

internal sealed class OneCSiteRequestSender(OneCSoapClientFactory factory) : IOneCSiteRequestSender
{
    public Task<RequestDataOut?> SendAsync(RequestData request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return factory.Create().CreateSiteRequestAsync(request);
    }
}
