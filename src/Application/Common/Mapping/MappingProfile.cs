namespace Application.Common.Mapping
{
    /// <summary>
    /// Базовый профиль маппинга, автоматически применяющий маппинги из классов, реализующих IMapWith
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Применяет маппинги из всех типов в сборке, реализующих IMapWith
        /// </summary>
        /// <param name="assembly">Сборка для сканирования</param>
        private void ApplyMappingsFromAssembly(Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(type => type.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                              i.GetGenericTypeDefinition() == typeof(IMapWith<>)))
                .ToList();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);
                var methodInfo = type.GetMethod("Mapping") ??
                                 type.GetInterface("IMapWith`1")?.
                                     GetMethod("Mapping");

                methodInfo?.Invoke(instance, new object[] { this });
            }
        }
    }
}
