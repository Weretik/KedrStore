namespace Work.Application.Features.Tasks.Common;

/// <summary>Shape of task input shared by the create and update validators.</summary>
internal static class WorkTaskInputRules
{
    public static void Title<T>(IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().Must(value => value.Trim().Length > 0).WithMessage("Title is required.")
            .MaximumLength(WorkTask.TitleMaxLength);

    public static void Details<T>(IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(WorkTask.DetailsMaxLength);

    /// <summary>R-9: up to 30 steps, each with text up to 200 characters.</summary>
    public static void Steps<T>(IRuleBuilder<T, IReadOnlyList<WorkTaskStepInput>?> rule) =>
        rule.Must(steps => steps is null || steps.Count <= WorkTask.StepsMaxCount)
            .WithMessage($"A task may have at most {WorkTask.StepsMaxCount} steps.")
            .Must(steps => steps is null || steps.All(step =>
                !string.IsNullOrWhiteSpace(step.Title) && step.Title.Trim().Length <= WorkTaskStep.TitleMaxLength))
            .WithMessage($"Each step needs text of at most {WorkTaskStep.TitleMaxLength} characters.");
}
