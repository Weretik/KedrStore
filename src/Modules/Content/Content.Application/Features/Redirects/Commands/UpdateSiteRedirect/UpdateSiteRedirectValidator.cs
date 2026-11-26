using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.UpdateSiteRedirect;

public sealed class UpdateSiteRedirectValidator : AbstractValidator<UpdateSiteRedirectCommand>
{
    public UpdateSiteRedirectValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        this.AddressRules(x => x.From, x => x.To);
    }
}
