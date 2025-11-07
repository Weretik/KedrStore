using Domain.Common.ValueObject;

namespace Domain.Common.Entity;

public abstract class BaseCategory<TId, TSelf> : BaseEntity<TId>
    where TSelf : BaseCategory<TId, TSelf>
    where TId : struct
{
    #region Properties & Constructors
    public string Name { get; private set; } = null!;
    public CategoryPath  Path { get; private set; }

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
    protected void SetPath(CategoryPath  path) => Path = Guard.Against.Default(path, nameof(path));
    #endregion

    #region ProductCategory API
    public void Rename(string name) => SetName(name);
    public void Repath(CategoryPath  newPath) => SetPath(newPath);

    public void ReparentTo(CategoryPath newParentPath)
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
    /// <summary>
    /// Ensures reparenting doesn't break tree invariants:
    /// - can't set node as its own parent;
    /// - can't move node under its own descendant (cycle).
    /// </summary>
    protected void GuardReparentInvariant(CategoryPath newParentPath)
    {
        Guard.Against.Null(newParentPath, nameof(newParentPath));

        if (newParentPath.Value == Path.Value)
            Guard.Against.InvalidInput(
                newParentPath, nameof(newParentPath),
                _ => true, "You cannot set a node as its own parent.");

        if (IsDescendantOf(newParentPath, Path))
            Guard.Against.InvalidInput(
                newParentPath, nameof(newParentPath),
                _ => true, "You cannot move a node under its own descendant (cycle in the tree).");
    }
    protected bool IsSameParent(CategoryPath newParentPath)
        => TryGetParentPath(out var currentParent) && currentParent.Value == newParentPath.Value;

    /// <summary>
    /// Combines parent path with child segment, e.g. ("n1.n2", "n3") → "n1.n2.n3".
    /// </summary>
    protected static CategoryPath Concat(CategoryPath parent, string segment) =>
        CategoryPath.From($"{parent.Value}.{segment}");

    /// <summary>
    /// Returns the node’s own segment — substring after the last dot in <see cref="Path"/>.
    /// </summary>
    protected string GetSelfSegment()
    {
        var s = Path.Value;
        var idx = s.LastIndexOf('.');
        return idx < 0 ? s : s[(idx + 1)..];
    }

    /// <summary>
    /// Extracts the parent path if present; returns false for root node.
    /// </summary>
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

    /// <summary>
    /// True if node has only one segment (root node).
    /// </summary>
    public bool IsRoot => SegmentCount(Path) <= 1;


    /// <summary>
    /// Checks if one path is a strict descendant of another:
    /// must start with ancestor.Value + '.' and be longer.
    /// </summary>
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
