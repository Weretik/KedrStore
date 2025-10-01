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

        CategoryId? categoryId = parameters.CategoryId.HasValue ? new CategoryId(parameters.CategoryId.Value) : null;
        PageRequest pageRequest = new(parameters.PageNumber, parameters.PageSize);
        ProductsFilter criteria = new(
            SearchTerm: parameters.SearchTerm,
            CategoryId: categoryId,
            MinPrice: parameters.MinPrice,
            MaxPrice: parameters.MaxPrice,
            Manufacturer: parameters.Manufacturer
        );

        var query = new GetProductsQuery(criteria, pageRequest, parameters.Sort);

        try
        {
            var result = await mediator.Send(query, cancellationToken);

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
                    pageRequest
                );

                dispatcher.Dispatch(new CatalogActions.LoadSuccess(empty));
                return;
            }

            if (result.Status == ResultStatus.Invalid)
            {
                var msg = string.Join("; ",
                    result.ValidationErrors.Select(error => $"{error.Identifier}: {error.ErrorMessage}"));
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
