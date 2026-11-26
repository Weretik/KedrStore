using BuildingBlocks.Application.Outbox;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Application.Features.QuickOrders.ToOneC;

namespace Sales.Infrastructure.DependencyInjection;

internal static class SiteOrdersToOneCRegistrationExtensions
{
    /// <summary>Spec sales/009, R-010: every site order becomes a 1C order on the retail counterparty from the settings.</summary>
    public static IServiceCollection AddSiteOrdersToOneC(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SiteOrderOneCOptions>()
            .Bind(configuration.GetSection(SiteOrderOneCOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.CounterpartyId),
                "Sales:SiteOrders:CounterpartyId must name the 1C counterparty of site orders.")
            .ValidateOnStart();
        services.AddOutboxHandler<QuickOrderToOneC, CreateOneCOrderFromQuickOrderHandler>(QuickOrderToOneC.TypeName);
        return services;
    }
}
