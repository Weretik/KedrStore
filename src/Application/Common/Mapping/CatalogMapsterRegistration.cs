namespace Application.Common.Mapping;

public static class MapsterRegistration
{
    public static void RegisterAll(TypeAdapterConfig config, TimeProvider time)
    {
        CatalogMapping.Register(config, time);
    }
}
