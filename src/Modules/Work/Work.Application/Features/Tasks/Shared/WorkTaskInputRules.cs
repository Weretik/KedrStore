namespace Work.Application.Features.Tasks.Shared;

/// <summary>Shape of task input shared by the create and update validators.</summary>
internal static class WorkTaskInputRules
{
    public static void Title<T>(IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().Must(value => value.Trim().Length > 0).WithMessage("Вкажіть заголовок.")
            .MaximumLength(WorkTask.TitleMaxLength);

    public static void Details<T>(IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(WorkTask.DetailsMaxLength);

    /// <summary>R-9: up to 30 steps, each with text up to 200 characters.</summary>
    public static void Steps<T>(IRuleBuilder<T, IReadOnlyList<WorkTaskStepInput>?> rule) =>
        rule.Must(steps => steps is null || steps.Count <= WorkTask.StepsMaxCount)
            .WithMessage($"У завданні не більше {WorkTask.StepsMaxCount} кроків.")
            .Must(steps => steps is null || steps.All(step =>
                !string.IsNullOrWhiteSpace(step.Title) && step.Title.Trim().Length <= WorkTaskStep.TitleMaxLength))
            .WithMessage($"Кожному кроку потрібен текст до {WorkTaskStep.TitleMaxLength} символів.");
}
