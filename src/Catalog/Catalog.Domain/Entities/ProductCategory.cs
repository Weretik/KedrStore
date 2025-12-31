using Catalog.Domain.Enumerations;
using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

public class ProductCategory : BaseEntity<ProductCategoryId>, IAggregateRoot
{
    #region Properties
    public string Name { get; private set; } = null!;
    public CategoryPath  Path { get; private set; }
    public ProductType ProductType { get; private set; }
    #endregion

    #region Constructors
    private ProductCategory() { }
    private ProductCategory(ProductCategoryId id, string name, CategoryPath path, ProductType productType)
    {
        SetCategoryId(id);
        SetName(name);
        SetPath(path);
        SetProductType(productType);
    }
    #endregion

    #region Factories
    public static ProductCategory Create(ProductCategoryId id, string name, CategoryPath path, ProductType productType)
        => new(id, name, path, productType);
    public static ProductCategory CreateRoot(ProductCategoryId id, string name, ProductType productType)
        => new(id, name, CategoryPath.Root(id.ToString()), productType);
    public static ProductCategory CreateChild(ProductCategoryId id, string name, ProductCategory parent, ProductType productType)
        => new(id, name, parent.Path.Append(id.ToString()), productType);
    #endregion

    #region Setters/Validation
    protected void SetCategoryId(ProductCategoryId id)
    {
        if (id.Value <= 0) throw new DomainException(CategoryErrors.IdMustBePositive());
        Id = id;
    }
    protected void SetName(string name)
    {

        if (string.IsNullOrWhiteSpace(name)) throw new DomainException(CategoryErrors.NameIsRequired());
        if (name.Length is < 1 or > 100) throw new DomainException(CategoryErrors.NameLengthInvalid(name.Length));
        Name = name.Trim();
    }
    protected void SetPath(CategoryPath path)
    {
        if(path.Value.Length == 0) throw new DomainException(CategoryErrors.PathIsRequired());
        Path = path;
    }
    private void SetProductType(ProductType productType)
    {
        if (productType == 0) throw new DomainException(CategoryErrors.ProductTypeInvalid());
        ProductType = productType;
    }
    #endregion

    #region ProductCategory API
    public void Rename(string name) => SetName(name);
    public void Repath(CategoryPath newPath) => SetPath(newPath);

    public void ReparentTo(CategoryPath newParentPath)
    {
        GuardReparentInvariant(newParentPath);
        if (IsSameParent(newParentPath)) return;

        var selfSegment  = GetSelfSegment();
        Path = Concat(newParentPath, selfSegment);
    }
    public void ReparentTo(ProductCategory newParent)
    {
        ReparentTo(newParent.Path);
    }
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

    public bool TryGetParentPath(out CategoryPath parentPath)
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
