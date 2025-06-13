namespace Application.Extensions
{
    public static class MappingServiceCollection
    {
        public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services, Assembly assembly)
        {
            services.AddAutoMapper(assembly);
            return services;
        }
        public static IServiceCollection AddAutoMapperProfiles<T>(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(T).Assembly);
            return services;
        }
    }
}
