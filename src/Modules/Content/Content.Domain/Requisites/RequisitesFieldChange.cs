namespace Content.Domain.Requisites;

/// <summary>One field of the requisites that an update changed.</summary>
public sealed class RequisitesFieldChange
{
    public string Field { get; private set; } = null!;
    public string OldValue { get; private set; } = null!;
    public string NewValue { get; private set; } = null!;

    private RequisitesFieldChange() { }

    public RequisitesFieldChange(string field, string oldValue, string newValue)
    {
        Field = field;
        OldValue = oldValue;
        NewValue = newValue;
    }
}
