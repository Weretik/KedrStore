using Application.Catalog.GetCategories;

namespace Presentation.Shared.States.Category;

public sealed class CategoryStore(IDispatcher dispatcher) : ICategoryStore
{
    public void SetProductTypeId(int? productTypeId)
        => dispatcher.Dispatch(new CategoryAction.SetProductTypeId(productTypeId));

    public void Load()
        => dispatcher.Dispatch(new CategoryAction.Load());

    public void LoadFailure(string error)
        => dispatcher.Dispatch(new CategoryAction.LoadFailure(error));

    public void LoadSuccess(IReadOnlyList<CategoryTreeDto> queryResult)
        => dispatcher.Dispatch(new CategoryAction.LoadSuccess(queryResult));

}
