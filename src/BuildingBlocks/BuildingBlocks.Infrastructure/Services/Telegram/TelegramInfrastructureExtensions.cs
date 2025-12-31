using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace BuildingBlocks.Infrastructure.Services;

public static class иTelegramInfrastructureExtensions
{
    public static IServiceCollection AddTelegramInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TelegramOptions>(configuration.GetSection("Telegram"));

        services.AddHttpClient("telegram")
            .AddTypedClient<ITelegramBotClient>((http, sp) =>
            {
                var opts = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
                if (string.IsNullOrWhiteSpace(opts.BotToken))
                    throw new InvalidOperationException("Telegram:BotToken is not set (ENV Telegram__BotToken).");
                return new TelegramBotClient(new TelegramBotClientOptions(opts.BotToken), http);
            });

        return services;
    }
}
