using Work.Application.Features.Activity.Common;
using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Report.GetReport;

/// <summary>R-7: what one staff member finished, what is still open, and what they changed in the period.</summary>
public sealed record WorkReportDto(
    Guid UserId,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<WorkTaskDto> DoneTasks,
    IReadOnlyList<WorkTaskDto> OpenTasks,
    IReadOnlyList<WorkActivityDto> Activity,
    bool ActivityTruncated);
