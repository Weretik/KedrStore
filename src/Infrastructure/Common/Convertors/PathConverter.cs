using Domain.Common.ValueObject;

namespace Infrastructure.Common.Convertors;

public static class PathConverter
{
    public static readonly ValueConverter<CategoryPath, LTree> Convert =
    new(
        path => (LTree)path.Value,
        ltree => CategoryPath.From(ltree)
    );
}
