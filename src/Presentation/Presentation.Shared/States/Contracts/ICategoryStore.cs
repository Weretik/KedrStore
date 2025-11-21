using Application.Catalog.GetCategories;

namespace Presentation.Shared.States;

public interface ICategoryStore
{
    void SetProductTypeId(int? productTypeId);

    void Load();
    void LoadFailure(string error);
    void LoadSuccess(IReadOnlyList<CategoryTreeDto> queryResult);
}
