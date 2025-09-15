namespace Presentation.Shared.States.Catalog;

public sealed class CatalogEffects(
    IMediator mediator,
    IState<CatalogState> state,
    ILogger<CatalogEffects> logger)
{
    private CancellationTokenSource? _cancellationToken;

    [EffectMethod]
    public async Task HandleLoad(CatalogActions.Load action, IDispatcher dispatcher)
    {
        _cancellationToken?.Cancel();
        _cancellationToken = new CancellationTokenSource();

        var ct = _cancellationToken.Token;
        var p = state.Value.Params;

        CategoryId? categoryId = p.CategoryId.HasValue ? new CategoryId(p.CategoryId.Value) : null;

        var criteria = new ProductsCriteria(
            SearchTerm: p.SearchTerm,
            CategoryId: categoryId,
            MinPrice: p.MinPrice,
            MaxPrice: p.MaxPrice,
            Manufacturer: p.Manufacturer,
            Sort: p.Sort,
            PageNumber: p.PageNumber,
            PageSize: p.PageSize
        );

        var query = new GetProductsQuery(criteria);

        try
        {
            var result = await mediator.Send(query, ct);

            if (result.Status == ResultStatus.Ok && result.Value is not null)
            {
                dispatcher.Dispatch(new CatalogActions.LoadSuccess(result.Value));
                return;
            }

            if (result.Status == ResultStatus.NotFound)
            {
                var empty = new PaginatedList<ProductDto>(
                    Items: [],
                    Total: 0,
                    PageNumber: p.PageNumber,
                    PageSize: p.PageSize
                );

                dispatcher.Dispatch(new CatalogActions.LoadSuccess(empty));
                return;
            }

            if (result.Status == ResultStatus.Invalid)
            {
                var msg = string.Join("; ",
                    result.ValidationErrors.Select(e => $"{e.Identifier}: {e.ErrorMessage}"));
                dispatcher.Dispatch(new CatalogActions.LoadFailure(msg));
            }

            else
            {
                var fallback = result.Errors?.FirstOrDefault() ?? "Помилка завантаження каталогу.";
                dispatcher.Dispatch(new CatalogActions.LoadFailure(fallback));
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
