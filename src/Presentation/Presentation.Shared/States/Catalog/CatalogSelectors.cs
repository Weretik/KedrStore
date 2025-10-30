using Application.Catalog.GetProducts;
using Application.Catalog.Shared.DTOs;

namespace Presentation.Shared.States.Catalog;

public static class CatalogSelectors
{
    public static CatalogParams Params(CatalogState state) => state.Params;
    public static bool IsLoading(CatalogState state) => state.IsLoading;
    public static string? Error(CatalogState state) => state.Error;
    public static PaginationResult<ProductDto>? PageList(CatalogState state) => state.PageList;


    public static IReadOnlyList<ProductDto> Items(CatalogState state)
        => state.PageList?.Items ?? [];

    public static int TotalPages(CatalogState state)
        => state.PageList?.TotalPages ?? 0;

    public static bool HasError(CatalogState state)
        => state.Error is not null;

    public static bool HasPageList(CatalogState state)
        => state.PageList is not null;

    public static bool IsEmpty(CatalogState state)
        => state.PageList is not null && state.PageList.Total == 0;
}
