namespace Work.Domain;

public static class WorkErrors
{
    public static DomainError Required(string field) =>
        new($"Work.{field}.Required", $"{field} is required");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Work.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static DomainError TooManySteps(int maxCount, int count) =>
        new("Work.Steps.TooMany", $"A task may have at most {maxCount} steps. Actual: {count}");

    public static DomainError UserRequired() =>
        new("Work.User.Required", "A task and an activity entry need a user");
}
