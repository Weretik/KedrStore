namespace Work.Domain.Tasks;

/// <summary>One step of a task (spec work/001, R-9): a short line the owner ticks off. Kept inside the task.</summary>
public sealed class WorkTaskStep
{
    public const int TitleMaxLength = 200;

    public string Title { get; private set; } = null!;
    public bool IsDone { get; private set; }

    private WorkTaskStep() { }

    internal static DomainResult<WorkTaskStep> Create(string? title, bool isDone)
    {
        var normalized = title?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
            return WorkErrors.Required("Steps");
        if (normalized.Length > TitleMaxLength)
            return WorkErrors.TooLong("Steps", TitleMaxLength, normalized.Length);
        return new WorkTaskStep { Title = normalized, IsDone = isDone };
    }

    internal void SetDone(bool isDone) => IsDone = isDone;
}

/// <summary>A step as the caller sends it: its text and whether it is done.</summary>
public sealed record WorkTaskStepInput(string Title, bool IsDone);
