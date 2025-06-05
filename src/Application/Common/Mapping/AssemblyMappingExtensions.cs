using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Mapping
{
    /// <summary>
    /// Расширения для регистрации маппингов в DI-контейнере
    /// </summary>
    public static class AssemblyMappingExtensions
    {
        /// <summary>
        /// Регистрирует AutoMapper с профилями из указанной сборки
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="assembly">Сборка, содержащая профили маппинга</param>
        /// <returns>Коллекция сервисов для цепочки вызовов</returns>
        public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services, Assembly assembly)
        {
            services.AddAutoMapper(assembly);
            return services;
        }

        /// <summary>
        /// Регистрирует AutoMapper с профилями из сборки, содержащей указанный тип
        /// </summary>
        /// <typeparam name="T">Тип, сборка которого содержит профили маппинга</typeparam>
        /// <param name="services">Коллекция сервисов</param>
        /// <returns>Коллекция сервисов для цепочки вызовов</returns>
        public static IServiceCollection AddAutoMapperProfiles<T>(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(T).Assembly);
            return services;
        }
    }
}
