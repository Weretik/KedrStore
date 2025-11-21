using Application.Catalog.GetCategories;

namespace Presentation.Shared.States.Category;

public sealed class CategoryEffects(
    IState<CategoryState> state,
    ICategoryStore store,
    IMediator mediator,
    ILogger<CategoryEffects> logger)
{
    private CancellationTokenSource? _categoryCts;

    [EffectMethod(typeof(CategoryAction.Load))]
    public async Task OnLoad(IDispatcher dispatcher)
    {
        _categoryCts?.Cancel();
        _categoryCts = new CancellationTokenSource();

        var cancellationToken = _categoryCts.Token;
        var categoryState = state.Value;

        var query = new GetCategoriesQuery(categoryState.Filter);

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
                    IReadOnlyList<CategoryTreeDto> empty = [];
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
                    var fallback = result.Errors?.FirstOrDefault() ?? "Помилка завантаження категорій.";
                    store.LoadFailure(fallback);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Category load canceled");
        }
        catch (Exception ex)
        {
            store.LoadFailure(ex.Message);
        }

    }

    [EffectMethod(typeof(CategoryAction.SetFilter))]
    public Task OnSetFilter(IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }

    [EffectMethod(typeof(CategoryAction.SetProductTypeId))]
    public Task OnSetProductTypeId(IDispatcher dispatcher)
    {
        store.Load();
        return Task.CompletedTask;
    }

}
