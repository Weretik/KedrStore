namespace Application.Common.Mapping
{
    public static class MappingHelper
    {
        public static MapperConfiguration CreateMapperConfiguration(Assembly assembly)
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(assembly);
            });
        }
        public static IMapper CreateMapper(Assembly assembly)
        {
            return CreateMapperConfiguration(assembly).CreateMapper();
        }
        public static void ValidateMapperConfiguration(Assembly assembly)
        {
            var config = CreateMapperConfiguration(assembly);
            config.AssertConfigurationIsValid();
        }
    }
}
