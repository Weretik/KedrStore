using Application.Catalog.GetProducts;
using Application.Catalog.Shared;

namespace Presentation.Shared.States.Catalog.Effects;

public sealed class CatalogLoadEffect(IState<CatalogState> state, IMediator mediator, ILogger<CatalogLoadEffect> logger)
{
    private CancellationTokenSource? _cancellationToken;

    [EffectMethod]
    public async Task OnLoad(CatalogLoadAction.Load action, IDispatcher dispatcher)
    {
        _cancellationToken?.Cancel();
        _cancellationToken = new CancellationTokenSource();
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
                dispatcher.Dispatch(new CatalogLoadAction.LoadSuccess(result.Value));
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

                    dispatcher.Dispatch(new CatalogLoadAction.LoadSuccess(empty));
                    return;
                }
                case ResultStatus.Invalid:
                {
                    var message = string.Join("; ",
                        result.ValidationErrors.Select(error => $"{error.Identifier}: {error.ErrorMessage}"));

                    dispatcher.Dispatch(new CatalogLoadAction.LoadFailure(message));
                    break;
                }
                default:
                {
                    var fallback = result.Errors?.FirstOrDefault() ?? "Помилка завантаження каталогу.";

                    dispatcher.Dispatch(new CatalogLoadAction.LoadFailure(fallback));
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
            dispatcher.Dispatch(new CatalogLoadAction.LoadFailure(ex.Message));
        }
    }
}
