using AutoMapper;

namespace Application.Common.Mapping
{
    /// <summary>
    /// Интерфейс для классов, которые должны маппиться с типом T
    /// </summary>
    /// <typeparam name="T">Тип, с которым выполняется маппинг</typeparam>
    public interface IMapWith<T>
    {
        /// <summary>
        /// Настраивает маппинг между текущим типом и типом T
        /// </summary>
        /// <param name="profile">Профиль AutoMapper</param>
        void Mapping(Profile profile);
    }
}
