namespace Work.Application.Features.Report.GetReport;

/// <summary>
/// The report of <paramref name="UserId"/>, or of the signed-in staff member when it is <c>null</c>. Only the admin
/// endpoint passes another user's ID. Dates are Kyiv calendar days, both inclusive.
/// </summary>
public sealed record GetWorkReportQuery(Guid? UserId, DateOnly From, DateOnly To) : IQuery<Result<WorkReportDto>>;
