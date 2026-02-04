using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Orders.Commands.CreateQuickOrder;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Notifications;
using Catalog.Infrastructure.Repositories;

namespace Catalog.Infrastructure.DependencyInjection;

public static class CatalogTelegramNotifierExtension
{
    public static IServiceCollection AddTelegramNotifierServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITelegramNotifier, TelegramBotNotifier>();

        return services;
    }
}
