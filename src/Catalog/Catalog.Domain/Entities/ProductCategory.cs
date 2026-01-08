using Catalog.Domain.Enumerations;
using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

public class ProductCategory : BaseEntity<ProductCategoryId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public ProductCategoryId? ParentId { get; private set; }
    public CategoryPath  Path { get; private set; }
    #endregion

    #region Constructors
    private ProductCategory() { }
    private ProductCategory(ProductCategoryId id, string name, string slug, CategoryPath path, ProductCategoryId? parentId = null)
    {
        SetCategoryId(id);
        SetName(name);
        SetSlug(slug);
        SetPath(path);
        SetParentId(parentId);
    }
    #endregion

    #region Factories
    public static ProductCategory Create(ProductCategoryId id, string name, string slug, CategoryPath path, ProductCategoryId? parentId = null)
        => new(id, name, slug,path, parentId);

    public void Update(string name, string slug, CategoryPath path, ProductCategoryId? parentId = null)
    {
        SetName(name);
        SetSlug(slug);
        SetPath(path);
        SetParentId(parentId);
    }
    #endregion

    #region Setters/Validation
    private void SetCategoryId(ProductCategoryId id)
    {
        if (id.Value <= 0) throw new DomainException(CategoryErrors.IdMustBePositive());
        Id = id;
    }
    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException(CategoryErrors.NameIsRequired());
        if (name.Length is < 1 or > 100) throw new DomainException(CategoryErrors.NameLengthInvalid(name.Length));
        Name = name.Trim();
    }

    private void SetSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) throw new DomainException(CategoryErrors.NameIsRequired());
        Slug = slug.Trim();
    }
    private void SetPath(CategoryPath path)
    {
        if(path.Value.Length == 0) throw new DomainException(CategoryErrors.PathIsRequired());
        Path = path;
    }
    private void SetParentId(ProductCategoryId? parentId)
    {
        ParentId = parentId;
    }
    #endregion

    #region ProductCategory API
    public void Rename(string name) => SetName(name);
    public void Repath(CategoryPath newPath) => SetPath(newPath);
    #endregion

    #region Internal rules
    protected void GuardReparentInvariant(CategoryPath newParentPath)
    {
        if (newParentPath.Value == Path.Value)
            throw new DomainException(CategoryErrors.CannotSetSelfAsParent(Path.Value));

        if (IsDescendantOf(newParentPath, Path))
            throw new DomainException(CategoryErrors.CannotMoveUnderDescendant(
                Path.Value, newParentPath.Value)
            );
    }
    protected bool IsSameParent(CategoryPath newParentPath)
        => TryGetParentPath(out var currentParent) && currentParent.Value == newParentPath.Value;

    protected static CategoryPath Concat(CategoryPath parent, string segment) =>
        CategoryPath.From($"{parent.Value}.{segment}");

    protected string GetSelfSegment()
    {
        var s = Path.Value;
        var idx = s.LastIndexOf('.');
        return idx < 0 ? s : s[(idx + 1)..];
    }

    private bool TryGetParentPath(out CategoryPath parentPath)
    {
        if (IsRoot)
        {
            parentPath = CategoryPath.None;
            return false;
        }

        var s = Path.Value;
        var idx = s.LastIndexOf('.');
        parentPath = CategoryPath.From(s[..idx]);
        return true;
    }

    public bool IsRoot => SegmentCount(Path) <= 1;

    private static bool IsDescendantOf(CategoryPath candidateChild, CategoryPath candidateAncestor)
    {
        var anc = candidateAncestor.Value;
        var child = candidateChild.Value;
        return child.Length > anc.Length &&
               child.StartsWith(anc, StringComparison.Ordinal) &&
               child[anc.Length] == '.';
    }

    private static int SegmentCount(CategoryPath path)
        => 1 + path.Value.Count(ch => ch == '.');
    #endregion
}
