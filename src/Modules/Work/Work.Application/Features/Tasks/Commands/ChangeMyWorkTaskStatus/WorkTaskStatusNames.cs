namespace Work.Application.Features.Tasks.Commands.ChangeMyWorkTaskStatus;

/// <summary>The API spells a board column by its enum name; numbers are not accepted.</summary>
internal static class WorkTaskStatusNames
{
    public static bool TryParse(string? value, out WorkTaskStatus status)
    {
        status = default;
        return value is not null
               && Enum.GetNames<WorkTaskStatus>().Contains(value, StringComparer.Ordinal)
               && Enum.TryParse(value, out status);
    }
}
