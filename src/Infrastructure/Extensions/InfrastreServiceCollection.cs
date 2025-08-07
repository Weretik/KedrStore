namespace Infrastructure.Extensions;

public static class InfrastreServiceCollection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Подключение Catalog БД
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICatalogDbContext>(provider =>
            provider.GetRequiredService<CatalogDbContext>());

        services.AddScoped<ICatalogUnitOfWork, CatalogUnitOfWork>();

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

        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromMinutes(30);
        });

        // Регистрация миграторов каталога и идентификации
        services.AddScoped<IDatabaseMigrator, CatalogDbMigrator>();
        services.AddScoped<ICatalogDbMigrator, CatalogDbMigrator>();

        // Регистрация миграторов идентификации
        services.AddScoped<IDatabaseMigrator, AppIdentityDbMigrator>();
        services.AddScoped<IAppIdentityDbMigrator, AppIdentityDbMigrator>();

        // Регистрация Сервисов и Репозиториев
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

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
        services.AddScoped<IDomainEventContext, DomainEventContext>();
        services.AddScoped<ILoggingService, LoggingService>();

        return services;
    }
}
