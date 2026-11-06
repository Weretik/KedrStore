namespace Work.Domain.Tasks;

/// <summary>
/// A staff member's own task (spec work/001, R-1…R-4). The owner moves it between the board columns and writes
/// the result when it is done; an administrator may create one for anybody.
/// </summary>
public sealed class WorkTask : IAggregateRoot
{
    public const int TitleMaxLength = 200;
    public const int DetailsMaxLength = 4000;
    public const int ResultMaxLength = 2000;
    public const int StepsMaxCount = 30;

    private readonly List<WorkTaskStep> _steps = [];

    public WorkTaskId Id { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Details { get; private set; }
    public WorkTaskStatus Status { get; private set; }
    public string? Result { get; private set; }
    /// <summary>R-10: the day the task should be done by (Kyiv calendar); none means no deadline.</summary>
    public DateOnly? DueDate { get; private set; }
    public IReadOnlyList<WorkTaskStep> Steps => _steps;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private WorkTask() { }

    public static WorkTask Create(
        Guid ownerUserId,
        Guid createdByUserId,
        string title,
        string? details,
        DateOnly? dueDate,
        IReadOnlyList<WorkTaskStepInput>? steps,
        DateTimeOffset now)
    {
        if (ownerUserId == Guid.Empty || createdByUserId == Guid.Empty)
            throw new DomainException(WorkErrors.UserRequired());

        var task = new WorkTask
        {
            OwnerUserId = ownerUserId,
            CreatedByUserId = createdByUserId,
            Title = NormalizeTitle(title),
            Details = NormalizeOptional(details, nameof(Details), DetailsMaxLength),
            DueDate = dueDate,
            Status = WorkTaskStatus.Planned,
            CreatedAt = now,
            UpdatedAt = now
        };
        task.ReplaceSteps(steps);
        return task;
    }

    public void Edit(string title, string? details, DateOnly? dueDate, IReadOnlyList<WorkTaskStepInput>? steps,
        DateTimeOffset now)
    {
        Title = NormalizeTitle(title);
        Details = NormalizeOptional(details, nameof(Details), DetailsMaxLength);
        DueDate = dueDate;
        ReplaceSteps(steps);
        UpdatedAt = now;
    }

    /// <summary>R-9: ticks or unticks one step by its position; false when there is no such step.</summary>
    public bool SetStepDone(int index, bool isDone, DateTimeOffset now)
    {
        if (index < 0 || index >= _steps.Count)
            return false;
        _steps[index].SetDone(isDone);
        UpdatedAt = now;
        return true;
    }

    /// <summary>R-10: an open task whose due day has passed in Kyiv.</summary>
    public bool IsOverdue(DateOnly today) => Status != WorkTaskStatus.Done && DueDate is { } due && due < today;

    /// <summary>R-3: start time is set once, completion keeps the result, leaving «Готово» clears both.</summary>
    public void ChangeStatus(WorkTaskStatus status, string? result, DateTimeOffset now)
    {
        if (status != WorkTaskStatus.Planned)
            StartedAt ??= now;

        if (status == WorkTaskStatus.Done)
        {
            if (Status != WorkTaskStatus.Done)
                CompletedAt = now;
            Result = NormalizeOptional(result, nameof(Result), ResultMaxLength);
        }
        else
        {
            CompletedAt = null;
            Result = null;
        }

        Status = status;
        UpdatedAt = now;
    }

    public bool IsCreatedByOwner => CreatedByUserId == OwnerUserId;

    private void ReplaceSteps(IReadOnlyList<WorkTaskStepInput>? steps)
    {
        steps ??= [];
        if (steps.Count > StepsMaxCount)
            throw new DomainException(WorkErrors.TooManySteps(StepsMaxCount, steps.Count));
        var created = steps.Select(step => WorkTaskStep.Create(step.Title, step.IsDone)).ToList();
        _steps.Clear();
        _steps.AddRange(created);
    }

    private static string NormalizeTitle(string? title)
    {
        var normalized = title?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
            throw new DomainException(WorkErrors.Required(nameof(Title)));
        if (normalized.Length > TitleMaxLength)
            throw new DomainException(WorkErrors.TooLong(nameof(Title), TitleMaxLength, normalized.Length));
        return normalized;
    }

    private static string? NormalizeOptional(string? value, string field, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized))
            return null;
        if (normalized.Length > maxLength)
            throw new DomainException(WorkErrors.TooLong(field, maxLength, normalized.Length));
        return normalized;
    }
}
