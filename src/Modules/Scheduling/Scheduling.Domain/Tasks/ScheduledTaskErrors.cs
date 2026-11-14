namespace Scheduling.Domain.Tasks;

public static class ScheduledTaskErrors
{
    public const int MaxMessageLength = 500;

    public static DomainError AlreadyRunning { get; } =
        DomainError.Conflict("Scheduling.Task.AlreadyRunning", "Задача вже виконується.");

    public static DomainError AlreadyRequested { get; } =
        DomainError.Conflict("Scheduling.Task.AlreadyRequested", "Запуск уже заплановано.");

    public static DomainError StaleRun { get; } =
        DomainError.Conflict("Scheduling.Task.StaleRun", "Цей запуск задачі вже не поточний.");

    public static DomainError NextRunInPast { get; } = new("Scheduling.Task.NextRunInPast", "Наступний запуск не може бути в минулому.", Field: "nextRunAt");

    /// <summary>A message kept for the admin: the first line, at most <see cref="MaxMessageLength"/> characters.</summary>
    public static string Shorten(string message)
    {
        var text = string.IsNullOrWhiteSpace(message) ? "Невідома помилка." : message.Trim();
        return text.Length <= MaxMessageLength ? text : text[..(MaxMessageLength - 1)] + "…";
    }
}
