using Microsoft.OpenApi;

namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class SwaggerRegistrationExtensions
{
    private const string BearerSecurityScheme = "bearer";

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(BearerSecurityScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "Opaque",
                Description = "Paste the accessToken returned by POST /api/auth/session/login."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerSecurityScheme, document)] = []
            });
        });

        return services;
    }
}
