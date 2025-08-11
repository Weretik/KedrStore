namespace Application.Common.Mapping
{
    public static class MappingServiceCollection
    {
        public static IServiceCollection AddAutoMapperProfiles<TMarker>(
            this IServiceCollection services)
        {
            return services.AddAutoMapper((sp, cfg) => { }, typeof(TMarker));
        }

        public static IServiceCollection AddAutoMapperProfiles(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            return services.AddAutoMapper((sp, cfg) => { }, assemblies);
        }
    }
}
