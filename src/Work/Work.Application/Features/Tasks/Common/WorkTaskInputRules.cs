namespace Work.Application.Features.Tasks.Common;

/// <summary>Shape of task input shared by the create and update validators.</summary>
internal static class WorkTaskInputRules
{
    public static void Title<T>(IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().Must(value => value.Trim().Length > 0).WithMessage("Title is required.")
            .MaximumLength(WorkTask.TitleMaxLength);

    public static void Details<T>(IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(WorkTask.DetailsMaxLength);
}
