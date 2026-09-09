using Sales.Infrastructure.Integrations.OneC.Options;

namespace Sales.Infrastructure.DependencyInjection;

internal static class OneCOrderSyncOptionsRegistrationExtensions
{
    public static IServiceCollection AddOneCOrderSyncConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<OneCOrderSyncOptions>()
            .Bind(configuration.GetSection(OneCOrderSyncOptions.SectionName))
            .Validate(options => options.BatchSize > 0,
                "Sales:OneCOrderSync:BatchSize must be greater than zero.")
            .Validate(options => options.InterCallDelay >= TimeSpan.Zero,
                "Sales:OneCOrderSync:InterCallDelay cannot be negative.")
            .Validate(options => options.MaximumAttempts > 0,
                "Sales:OneCOrderSync:MaximumAttempts must be greater than zero.")
            .Validate(
                options => options.SendingWindowStart >= TimeSpan.Zero &&
                           options.SendingWindowStart < TimeSpan.FromDays(1),
                "Sales:OneCOrderSync:SendingWindowStart must be within one day.")
            .Validate(
                options => options.SendingWindowEnd > options.SendingWindowStart &&
                           options.SendingWindowEnd <= TimeSpan.FromDays(1),
                "Sales:OneCOrderSync:SendingWindowEnd must be after SendingWindowStart and within one day.")
            .Validate(options => options.StaleSentRecoveryDelay > TimeSpan.Zero,
                "Sales:OneCOrderSync:StaleSentRecoveryDelay must be greater than zero.")
            .ValidateOnStart();

        return services;
    }
}
