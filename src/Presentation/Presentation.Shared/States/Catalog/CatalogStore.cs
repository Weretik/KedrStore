namespace Presentation.Shared.States.Catalog;

public sealed class CatalogStore(
    IState<CatalogState> state, IDispatcher dispatcher)
    : ICatalogStore
{
    public void SetSearchTerm(string? value)
        => Apply(p => {
            var v = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            if (v == p.SearchTerm) return p;
            return p with
            {
                SearchTerm = v, PageNumber = 1
            };
        });

    public void SetCategoryId(int? id)
        => Apply(p =>
            p.CategoryId == id ? p : p with
            {
                CategoryId = id, PageNumber = 1
            });

    public void SetSort(string? sort)
        => Apply(p => {
            var s = string.IsNullOrWhiteSpace(sort) ? null : sort.Trim();
            if (s == p.Sort) return p;
            return p with
            {
                Sort = s, PageNumber = 1
            };
        });

    public void SetPrice(decimal? min, decimal? max)
        => Apply(p => (p.MinPrice, p.MaxPrice) == (min, max) ? p : p with
        {
            MinPrice = min, MaxPrice = max, PageNumber = 1
        });


    public void SetManufacturer(string? manufacturer)
        => Apply(p => {
            var m = string.IsNullOrWhiteSpace(manufacturer) ? null : manufacturer.Trim();
            if (m == p.Manufacturer) return p;
            return p with
            {
                Manufacturer = m, PageNumber = 1
            };
        });

    public void SetPageSize(int size)
        => Apply(p => {
            var sz = size < 12 ? 12 : size;
            if (sz == p.PageSize) return p;
            return p with
            {
                PageSize = sz, PageNumber = 1
            };
        });

    public void GoToPage(int page)
        => dispatcher.Dispatch(new CatalogActions.SetPageNumber(page));

    public void Reset()
        => dispatcher.Dispatch(new CatalogActions.ResetParams());

    public void Reload()
        => dispatcher.Dispatch(new CatalogActions.Load());

    private void Apply(Func<CatalogParams, CatalogParams> mutate)
    {
        var current = state.Value.Params;
        var next = mutate(current);

        if (next == current) return;
        dispatcher.Dispatch(new CatalogActions.SetParams(next));
    }
}
