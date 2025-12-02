using Application.Catalog.GetProducts;
using Application.Catalog.Shared;

namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogLoadEffect(
    IState<CatalogState> state,
    ICatalogStore store,
    IMediator mediator,
    ILogger<CatalogLoadEffect> logger)
{
    private CancellationTokenSource? _cancellationToken;

    [EffectMethod(typeof(CatalogLoadAction.Load))]
    public async Task OnLoad(IDispatcher dispatcher)
    {
        _cancellationToken?.Cancel();
        _cancellationToken = new CancellationTokenSource();
        await Task.Delay(150, _cancellationToken.Token);

        var cancellationToken = _cancellationToken.Token;
        var catalogState = state.Value;

        var query = new GetProductsQuery(
            Filter: catalogState.ProductsFilter,
            Pagination: catalogState.ProductsPagination,
            Sorter: catalogState.ProductsSorter,
            PricingOptions: catalogState.PricingOptions
        );

        try
        {
            var result = await mediator.Send(query, cancellationToken);

            if (result is { Status: ResultStatus.Ok, Value: not null })
            {
                store.LoadSuccess(result.Value);
                return;
            }

            switch (result.Status)
            {
                case ResultStatus.NotFound:
                {
                    var empty = new GetProductsQueryResult<ProductDto>(
                        Items: [],
                        TotalItems: 0,
                        ProductPagination: new()
                    );
                    store.LoadSuccess(empty);
                    return;
                }
                case ResultStatus.Invalid:
                {
                    var validateResult = result.ValidationErrors
                        .Select(error => $"{error.Identifier}: {error.ErrorMessage}");
                    var message = string.Join("; ", validateResult);

                    store.LoadFailure(message);
                    break;
                }
                default:
                {
                    var fallback = result.Errors?.FirstOrDefault() ?? "Помилка завантаження каталогу.";
                    store.LoadFailure(fallback);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Catalog load canceled");
        }
        catch (Exception ex)
        {
            store.LoadFailure(ex.Message);
        }
    }

    [EffectMethod(typeof(CatalogLoadAction.Reset))]
    public Task OnReset(IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }
}
