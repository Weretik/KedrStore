using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.CreateSiteRedirect;

public sealed class CreateSiteRedirectValidator : AbstractValidator<CreateSiteRedirectCommand>
{
    public CreateSiteRedirectValidator() => this.AddressRules(x => x.From, x => x.To);
}
