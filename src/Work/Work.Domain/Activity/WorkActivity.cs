namespace Work.Domain.Activity;

/// <summary>
/// One change a staff member made through an admin endpoint (spec work/001, R-5, R-6). Written automatically;
/// a repeat of the same change within <see cref="MergeWindow"/> updates this entry instead of adding a row.
/// </summary>
public sealed class WorkActivity : IAggregateRoot
{
    public const int UserEmailMaxLength = 256;
    public const int AreaMaxLength = 64;
    public const int OperationMaxLength = 100;
    public const int MethodMaxLength = 10;
    public const int TargetMaxLength = 300;

    public static readonly TimeSpan MergeWindow = TimeSpan.FromMinutes(10);

    public WorkActivityId Id { get; private set; }
    public Guid UserId { get; private set; }
    public string UserEmail { get; private set; } = string.Empty;
    public string Area { get; private set; } = null!;
    public string Operation { get; private set; } = null!;
    public string Method { get; private set; } = null!;
    public string Target { get; private set; } = string.Empty;
    public int Count { get; private set; }
    public DateTimeOffset FirstAt { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private WorkActivity() { }

    public static DomainResult<WorkActivity> Record(Guid userId, string? userEmail, string area, string operation,
        string method, string? target, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
            return WorkErrors.UserRequired();
        if (string.IsNullOrWhiteSpace(area))
            return WorkErrors.Required(nameof(Area));
        if (string.IsNullOrWhiteSpace(operation))
            return WorkErrors.Required(nameof(Operation));
        if (string.IsNullOrWhiteSpace(method))
            return WorkErrors.Required(nameof(Method));

        return new WorkActivity
        {
            UserId = userId,
            UserEmail = Cut(userEmail, UserEmailMaxLength),
            Area = Cut(area, AreaMaxLength),
            Operation = Cut(operation, OperationMaxLength),
            Method = Cut(method, MethodMaxLength).ToUpperInvariant(),
            Target = Cut(target, TargetMaxLength),
            Count = 1,
            FirstAt = now,
            OccurredAt = now
        };
    }

    /// <summary>R-6: the same user, area, operation and target shortly after this entry.</summary>
    public bool CanMerge(WorkActivity next) =>
        next.UserId == UserId
        && next.Area == Area
        && next.Operation == Operation
        && next.Method == Method
        && next.Target == Target
        && next.OccurredAt >= OccurredAt
        && next.OccurredAt - OccurredAt <= MergeWindow;

    public void Repeat(DateTimeOffset now)
    {
        Count++;
        OccurredAt = now;
    }

    private static string Cut(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length > maxLength ? normalized[..maxLength] : normalized;
    }
}
