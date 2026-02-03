using BuildingBlocks.Application.Contracts;
using BuildingBlocks.Application.Notifications;
using BuildingBlocks.Infrastructure.DomainEvents;
using BuildingBlocks.Infrastructure.Services;
using Catalog.Application.Contracts.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace BuildingBlocks.Infrastructure.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Telegram Service
        services.Configure<TelegramOptions>(configuration.GetSection("Telegram"));
        services.AddHttpClient("telegram")
            .AddTypedClient<ITelegramBotClient>((http, sp) =>
            {
                var opts = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
                if (string.IsNullOrWhiteSpace(opts.BotToken))
                    throw new InvalidOperationException("Telegram:BotToken is not set (ENV Telegram__BotToken).");
                return new TelegramBotClient(new TelegramBotClientOptions(opts.BotToken), http);
            });

        // Domain Events
        services.AddScoped<IDomainEventContext, EfDomainEventContext>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        // JsonConvector
        services.AddScoped<IXmlToJsonConvector, XmlToJsonConvector>();

        //Регистрация Fake Services
        services.AddScoped<IPermissionService, FakePermissionService>();
        services.AddScoped<ICurrentUserService, FakeCurrentUserService>();

        return services;
    }
}
