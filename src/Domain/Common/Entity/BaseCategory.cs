namespace Domain.Common.Entity;

public abstract class BaseCategory<TId, TSelf> : BaseEntity<TId>
    where TSelf : BaseCategory<TId, TSelf>
    where TId : struct
{
    #region Properties & Constructors
    public string Name { get; private set; } = null!;
    public LTree Path { get; private set; }

    protected BaseCategory() { }
    #endregion

    #region Setters/Validation
    protected void SetCategoryId(TId id) => Id = Guard.Against.Default(id, nameof(id));
    protected void SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.OutOfRange(name.Length, nameof(name), 1, 100);
        Name = name.Trim();
    }
    protected void SetPath(LTree path) => Path = Guard.Against.Null(path, nameof(path));
    #endregion

    #region ProductCategory API
    public void Rename(string name) => SetName(name);
    public void Repath(LTree newPath) => SetPath(newPath);

    public void ReparentTo(LTree newParentPath)
    {
        GuardReparentInvariant(newParentPath);

        if (IsSameParent(newParentPath)) return;

        var selfSegment  = GetSelfSegment();
        Path = Concat(newParentPath, selfSegment);
    }
    public void ReparentTo(TSelf newParent)
    {
        Guard.Against.Null(newParent, nameof(newParent));
        ReparentTo(newParent.Path);
    }
    #endregion

    #region Internal rules
    protected void GuardReparentInvariant(LTree newParentPath)
    {
        Guard.Against.Null(newParentPath, nameof(newParentPath));

        if (newParentPath == Path)
            Guard.Against.InvalidInput(
                newParentPath, nameof(newParentPath),
                _ => true, "You cannot set a node as its own parent.");

        if (newParentPath.IsDescendantOf(Path))
            Guard.Against.InvalidInput(
                newParentPath, nameof(newParentPath),
                _ => true, "You cannot move a node under its own descendant (cycle in the tree).");
    }
    protected bool IsSameParent(LTree newParentPath)
        => TryGetParentPath(out var currentParent) && currentParent == newParentPath;

    protected static LTree Concat(LTree parent, string segment) => new($"{parent}.{segment}");
    protected string GetSelfSegment() => Path.Subpath(-1).ToString();

    public bool TryGetParentPath(out LTree parentPath)
    {
        if (IsRoot)
        {
            parentPath = default;
            return false;
        }
        parentPath = Path.Subpath(0, Path.NLevel - 1);
        return true;
    }
    public bool IsRoot => Path.NLevel <= 1;
    #endregion
}
