namespace Infrastructure.Shared.Extensions;

public static class InfrastreServiceCollection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Подключение Catalog БД
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICatalogDbContext>(provider =>
            provider.GetRequiredService<CatalogDbContext>());

        services.AddScoped<ICatalogUnitOfWork, CatalogUnitOfWork>();

        // Подключение Identity БД
        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

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

        //Регистрация Fake Service
        services.AddScoped<IPermissionService, FakePermissionService>();
        services.AddScoped<ICurrentUserService, FakeCurrentUserService>();

        return services;
    }
}
