namespace Infrastructure.Extensions;

public static class InfrastreExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Подключение Catalog БД
        //var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        var connectionString = configuration.GetConnectionString("Default");
        services.AddDbContext<CatalogDbContext>(
            options => options.UseNpgsql(connectionString));

        services.AddDbContextFactory<CatalogDbContext>(
            options => options.UseNpgsql(connectionString),
            lifetime: ServiceLifetime.Scoped
        );

        // services.AddPooledDbContextFactory<CatalogDbContext>(
        // o => o.UseNpgsql(connectionString),
        // lifetime: ServiceLifetime.Scoped);

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

        // Регистрация миграторов каталога и идентификации
        services.AddScoped<IDatabaseMigrator, CatalogDbMigrator>();
        services.AddScoped<ICatalogDbMigrator, CatalogDbMigrator>();

        // Регистрация миграторов идентификации
        services.AddScoped<IDatabaseMigrator, AppIdentityDbMigrator>();
        services.AddScoped<IAppIdentityDbMigrator, AppIdentityDbMigrator>();

        // Регистрация сидеров Identity
        services.AddScoped<ISeeder, RoleSeeder>();
        services.AddScoped<IIdentitySeeder, RoleSeeder>();

        services.AddScoped<ISeeder, IdentitySeeder>();
        services.AddScoped<IIdentitySeeder, IdentitySeeder>();

        // Регистрация сидеров Catalog
        services.AddScoped<ISeeder, CategorySeeder>();
        services.AddScoped<ICatalogSeeder, CategorySeeder>();

        services.AddScoped<ISeeder, ProductSeeder>();
        services.AddScoped<ICatalogSeeder, ProductSeeder>();

        //Регистрация Fake Services
        services.AddScoped<IPermissionService, FakePermissionService>();
        services.AddScoped<ICurrentUserService, FakeCurrentUserService>();

        // Регистрация Services
        services.AddSingleton<IEnvironmentService, EnvironmentService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IDomainEventContext, EfDomainEventContext>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

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
