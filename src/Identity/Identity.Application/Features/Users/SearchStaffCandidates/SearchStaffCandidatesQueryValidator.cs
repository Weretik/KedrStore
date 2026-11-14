using FluentValidation;

namespace Identity.Application.Features.Users.SearchStaffCandidates;

public sealed class SearchStaffCandidatesQueryValidator : AbstractValidator<SearchStaffCandidatesQuery>
{
    public const int MinimumLength = 2;

    public SearchStaffCandidatesQueryValidator()
    {
        RuleFor(x => x.Search)
            .Must(search => search is not null && search.Trim().Length >= MinimumLength)
            .WithMessage($"Для пошуку введіть щонайменше {MinimumLength} символи.")
            .MaximumLength(256);
    }
}
