using Catalog.Domain.ValueObjects;

namespace Catalog.Infrastructure.Configurations;

public static class PathConverter
{
    public static readonly ValueConverter<CategoryPath, LTree> Convert =
    new(
        path => (LTree)path.Value,
        value => CategoryPath.From(value)
    );
}
