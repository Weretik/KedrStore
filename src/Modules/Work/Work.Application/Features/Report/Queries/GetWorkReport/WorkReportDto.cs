using Work.Application.Features.Activity.Shared;
using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Report.Queries.GetWorkReport;

/// <summary>R-7: what one staff member finished, what is still open, and what they changed in the period.</summary>
public sealed record WorkReportDto(
    Guid UserId,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<WorkTaskDto> DoneTasks,
    IReadOnlyList<WorkTaskDto> OpenTasks,
    IReadOnlyList<WorkActivityDto> Activity,
    bool ActivityTruncated);
