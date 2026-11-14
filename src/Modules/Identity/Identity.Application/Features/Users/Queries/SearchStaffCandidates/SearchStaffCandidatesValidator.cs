using FluentValidation;

namespace Identity.Application.Features.Users.Queries.SearchStaffCandidates;

public sealed class SearchStaffCandidatesValidator : AbstractValidator<SearchStaffCandidatesQuery>
{
    public const int MinimumLength = 2;

    public SearchStaffCandidatesValidator()
    {
        RuleFor(x => x.Search)
            .Must(search => search is not null && search.Trim().Length >= MinimumLength)
            .WithMessage($"Для пошуку введіть щонайменше {MinimumLength} символи.")
            .MaximumLength(256);
    }
}
