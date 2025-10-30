using Application.Catalog.GetProducts;
using Application.Catalog.GetProducts.DTOs;
using Application.Catalog.Shared.DTOs;

namespace Presentation.Shared.States.Catalog;

public sealed class CatalogEffects(IMediator mediator, IState<CatalogState> state, ILogger<CatalogEffects> logger)
{
    private CancellationTokenSource? _cancellationToken;

    [EffectMethod]
    public async Task HandleLoad(CatalogActions.Load action, IDispatcher dispatcher)
    {
        _cancellationToken?.Cancel();
        _cancellationToken = new CancellationTokenSource();

        var cancellationToken = _cancellationToken.Token;
        var parameters = state.Value.Params;

        ProductCategoryId? categoryId = parameters.CategoryId.HasValue
            ? ProductCategoryId.From(parameters.CategoryId.Value)
            : null;

        ProductPagination productPagination = new(parameters.PageNumber, parameters.PageSize);
        ProductSorter productSorter = new(parameters.SortKey, parameters.Desc);

        ProductFilter filters = new(
            SearchTerm: parameters.SearchTerm,
            CategoryId: categoryId,
            MinPrice: parameters.MinPrice,
            MaxPrice: parameters.MaxPrice,
            Stock: parameters.Stock
        );

        var query = new GetProductsQuery(filters, productPagination, productSorter, parameters.PriceTypeId);

        try
        {
            var result = await mediator.Send(query, cancellationToken);

            if (result is { Status: ResultStatus.Ok, Value: not null })
            {
                dispatcher.Dispatch(new CatalogActions.LoadSuccess(result.Value));
                return;
            }

            switch (result.Status)
            {
                case ResultStatus.NotFound:
                {
                    var empty = new PaginationResult<ProductDto>(
                        Items: [],
                        Total: 0,
                        productPagination
                    );

                    dispatcher.Dispatch(new CatalogActions.LoadSuccess(empty));
                    return;
                }
                case ResultStatus.Invalid:
                {
                    var msg = string.Join("; ",
                        result.ValidationErrors.Select(error => $"{error.Identifier}: {error.ErrorMessage}"));
                    dispatcher.Dispatch(new CatalogActions.LoadFailure(msg));
                    break;
                }
                default:
                {
                    var fallback = result.Errors?.FirstOrDefault() ?? "Помилка завантаження каталогу.";
                    dispatcher.Dispatch(new CatalogActions.LoadFailure(fallback));
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
            dispatcher.Dispatch(new CatalogActions.LoadFailure(ex.Message));
        }
    }

    [EffectMethod]
    public Task HandleReset(CatalogActions.ResetParams action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new CatalogActions.Load());
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task HandlePageNumberChanged(CatalogActions.SetPageNumber action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new CatalogActions.Load());
        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task HandleParamsChanged(CatalogActions.SetParams action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new CatalogActions.Load());
        return Task.CompletedTask;
    }
}
