using AutoMapper;
using System.Reflection;

namespace Application.Common.Mapping
{
    /// <summary>
    /// Вспомогательный класс для работы с маппингами
    /// </summary>
    public static class MappingHelper
    {
        /// <summary>
        /// Создает конфигурацию маппера на основе всех профилей в указанной сборке
        /// </summary>
        /// <param name="assembly">Сборка, содержащая профили маппинга</param>
        /// <returns>Конфигурация маппинга</returns>
        public static MapperConfiguration CreateMapperConfiguration(Assembly assembly)
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(assembly);
            });
        }

        /// <summary>
        /// Создает экземпляр маппера на основе всех профилей в указанной сборке
        /// </summary>
        /// <param name="assembly">Сборка, содержащая профили маппинга</param>
        /// <returns>Экземпляр маппера</returns>
        public static IMapper CreateMapper(Assembly assembly)
        {
            return CreateMapperConfiguration(assembly).CreateMapper();
        }

        /// <summary>
        /// Проверяет конфигурацию маппинга на наличие ошибок
        /// </summary>
        /// <param name="assembly">Сборка, содержащая профили маппинга</param>
        public static void ValidateMapperConfiguration(Assembly assembly)
        {
            var config = CreateMapperConfiguration(assembly);
            config.AssertConfigurationIsValid();
        }
    }
}
