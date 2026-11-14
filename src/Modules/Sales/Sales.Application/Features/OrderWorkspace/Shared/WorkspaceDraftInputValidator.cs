using Sales.Application.Abstractions.Orders;

namespace Sales.Application.Features.OrderWorkspace.Shared;

/// <summary>A draft's title, note and lines, shared by create and update.</summary>
public sealed class WorkspaceDraftInputValidator : AbstractValidator<WorkspaceDraftInput>
{
    public const int TitleMaxLength = 200;
    public const int NoteMaxLength = 2000;

    public WorkspaceDraftInputValidator()
    {
        RuleFor(draft => draft.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title) && title.Trim().Length <= TitleMaxLength)
            .WithErrorCode("invalid_draft").WithMessage($"Вкажіть назву чернетки до {TitleMaxLength} символів.");
        RuleFor(draft => draft.Note).MaximumLength(NoteMaxLength)
            .WithErrorCode("invalid_draft").WithMessage($"Примітка — не більше {NoteMaxLength} символів.");
        this.AddLineRules(draft => draft.CounterpartyId, draft => draft.Lines);
    }
}
