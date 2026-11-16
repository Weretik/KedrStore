using Sales.Application.Features.Orders.OneCDelivery.Send;

namespace Sales.Infrastructure.DependencyInjection;

internal static class OneCSendingWindowRegistrationExtensions
{
    /// <summary>The Kyiv window in which orders go to 1C (platform spec 006, R-011); defaults 07:00–20:00.</summary>
    public static IServiceCollection AddOneCSendingWindow(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OneCSendingWindowOptions>()
            .Bind(configuration.GetSection(OneCSendingWindowOptions.SectionName))
            .Validate(options => options.SendingWindowStart >= TimeSpan.Zero && options.SendingWindowStart < TimeSpan.FromDays(1),
                "Sales:OneCOrderSync:SendingWindowStart must be within one day.")
            .Validate(options => options.SendingWindowEnd > options.SendingWindowStart && options.SendingWindowEnd <= TimeSpan.FromDays(1),
                "Sales:OneCOrderSync:SendingWindowEnd must be after SendingWindowStart and within one day.")
            .ValidateOnStart();
        services.AddSingleton<OneCSendingWindow>();
        return services;
    }
}
