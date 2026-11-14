using Work.Domain.Tasks;
namespace Work.Api.Endpoints.Tasks.Shared;

/// <summary>Body of creating and of editing an own task.</summary>
/// <param name="DueDate">Day the task should be done by; null means no deadline.</param>
/// <param name="Steps">The whole list of steps in order; null or empty means none.</param>
public sealed record SaveWorkTaskRequest(
    string Title, string? Details, DateOnly? DueDate = null, IReadOnlyList<WorkTaskStepInput>? Steps = null);
