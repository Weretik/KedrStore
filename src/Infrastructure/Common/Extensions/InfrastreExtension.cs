using Application.Catalog.CreateQuickOrder;
using Application.Catalog.ImportCatalogFromXml;
using Application.Catalog.Shared;
using Application.Identity.Shared;
using Infrastructure.Catalog;
using Infrastructure.Catalog.Import;
using Infrastructure.Catalog.Notifications;
using Infrastructure.Catalog.Repositories;
using Infrastructure.Common.Contracts;
using Infrastructure.Common.Migrator;
using Infrastructure.Identity;
using Infrastructure.Identity.Contracts;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Persistence;
using Infrastructure.Identity.Security;
using Infrastructure.Identity.Seeders;

namespace Infrastructure.Common.Extensions;

public static class InfrastreExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Подключение Catalog БД
        //var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<CatalogDbContext>(
            options => options.UseNpgsql(connectionString));
        // services.AddPooledDbContextFactory<CatalogDbContext>(o => o.UseNpgsql(connectionString),lifetime: ServiceLifetime.Scoped);
        services.AddDbContextFactory<CatalogDbContext>(
            options => options.UseNpgsql(connectionString),
            lifetime: ServiceLifetime.Scoped
        );

        // Подключение Identity БД
        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Регистрация Identity
        services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        // Регистрация Репозиториев
        services.AddScoped(typeof(ICatalogRepository<>), typeof(CatalogEfRepository<>));
        services.AddScoped(typeof(ICatalogReadRepository<>), typeof(CatalogReadEfRepository<>));

        services.AddScoped(typeof(IAppIdentityRepository<>), typeof(AppIdentityEfRepository<>));
        services.AddScoped(typeof(IAppIdentityReadRepository<>), typeof(AppIdentityEfRepository<>));

        // Migrations
        services.AddScoped<IDatabaseMigrator, DbMigrator<CatalogDbContext>>();
        services.AddScoped<IDatabaseMigrator, DbMigrator<AppIdentityDbContext>>();

        // Регистрация сидеров Identity
        services.AddScoped<ISeeder, RoleSeeder>();
        services.AddScoped<IIdentitySeeder, RoleSeeder>();

        services.AddScoped<ISeeder, IdentitySeeder>();
        services.AddScoped<IIdentitySeeder, IdentitySeeder>();

        //Регистрация Fake Services
        services.AddScoped<IPermissionService, FakePermissionService>();
        services.AddScoped<ICurrentUserService, FakeCurrentUserService>();

        //Services
        services.AddScoped<IDomainEventContext, EfDomainEventContext>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        services.AddScoped<ICatalogXmlParser, CatalogXmlParser>();

        //Telegram
        services.Configure<TelegramOptions>(configuration.GetSection("Telegram"));
        services.AddHttpClient("telegram")
            .AddTypedClient<ITelegramBotClient>((http, sp) =>
            {
                var opts = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;

                if (string.IsNullOrWhiteSpace(opts.BotToken))
                    throw new InvalidOperationException("Telegram:BotToken is not set (ENV Telegram__BotToken).");

                return new TelegramBotClient(new TelegramBotClientOptions(opts.BotToken), http);
            });
        services.AddScoped<ITelegramNotifier, TelegramBotNotifier>();

        return services;
    }
}
