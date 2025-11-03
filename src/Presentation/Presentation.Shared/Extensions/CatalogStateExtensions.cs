using Presentation.Shared.States.Catalog;

namespace Presentation.Shared.Extensions;

public static class CatalogStateExtensions
{
    public static CatalogState ResetPage(this CatalogState state)
        => state with
        {
            ProductsPagination = state.ProductsPagination with { CurrentPage = 1 }
        };
}
