using Catalog.Domain.Categories;

namespace Catalog.Infrastructure.Persistence.Categories;

public static class PathConverter
{
    public static readonly ValueConverter<CategoryPath, LTree> Convert =
    new(
        path => (LTree)path.Value,
        value => CategoryPath.From(value)
    );
}
