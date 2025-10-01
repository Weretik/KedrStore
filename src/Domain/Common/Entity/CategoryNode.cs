namespace Domain.Common.Entity;

public class CategoryNode<TId, TSelf> : BaseEntity<TId>, IAggregateRoot
    where TSelf : CategoryNode<TId, TSelf>
    where TId : struct
{
    #region Properties & Constructors
    public  string Name { get; private set; } = null!;
    public TId? ParentId { get; private set; }
    private readonly List<TSelf> _children = [];
    public IReadOnlyCollection<TSelf> Children => _children.AsReadOnly();
    public LTree Path { get; private set; }

    protected CategoryNode() { }
    #endregion

    #region Setters/Validation
    protected void SetName(string name) => Name = Guard.Against.NullOrWhiteSpace(name, nameof(name)).Trim();
    protected void SetParent(TId? parentId) => ParentId = parentId;
    protected void SetPath(LTree path) => Path = Guard.Against.Null(path, nameof(path));
    #endregion

    #region Local helpers
    private bool HasDescendantLocal(TId candidateId)
    {
        var stack = new Stack<TSelf>(_children);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (EqualityComparer<TId>.Default.Equals(node.Id, candidateId)) return true;

            foreach (var child in node._children)
                stack.Push(child);
        }
        return false;
    }
    #endregion

    #region Domain operations
    public void Rename(string name) => SetName(name);
    public void Repath(LTree newPath) => SetPath(newPath);

    public void AddChild(TSelf child)
    {
        child = Guard.Against.Null(child, nameof(child));

        Guard.Against.InvalidInput(child, nameof(child),
            c => EqualityComparer<TId>.Default.Equals(c.Id, Id),
            "Cannot add node as a child of itself.");

        Guard.Against.InvalidInput(child.Id!, nameof(child.Id),
            id => _children.Any(c => EqualityComparer<TId>.Default.Equals(c.Id, id)),
            "Child already added.");

        Guard.Against.InvalidInput(child, nameof(child),
            c => c.ParentId is not null && !EqualityComparer<TId>.Default.Equals(c.ParentId.Value, Id),
            "Child belongs to another parent. Detach or move it first.");

        Guard.Against.InvalidInput(child, nameof(child),
            c => c.HasDescendantLocal(Id),
            "Cycle detected: cannot add ancestor as child.");

        _children.Add(child);
        child.SetParent(Id);
    }
    public void RemoveChild(TSelf child)
    {
        child = Guard.Against.Null(child, nameof(child));

        var node = _children.FirstOrDefault(c => EqualityComparer<TId>.Default.Equals(c.Id, child.Id));
        if (node is null) return;

        _children.Remove(node);

        if (EqualityComparer<TId>.Default.Equals(node.ParentId.Value, Id)) node.SetParent(default);
    }
    public void MoveTo(TId? newParentId)
    {
        Guard.Against.InvalidInput(newParentId, nameof(newParentId),
            p => p is not null && EqualityComparer<TId>.Default.Equals(p.Value, Id),
            "Cannot assign node as its own parent.");

        SetParent(newParentId);
    }
    #endregion
}
