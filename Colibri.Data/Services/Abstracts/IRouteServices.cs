using Colibri.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Colibri.Data.Services.Abstracts
{
    /// <summary>
    /// Интерфейс для управления маршрутизацией дронов.
    /// </summary>
    public interface IRouteServices
    {
        /// <summary>
        /// Сохраняет информацию о дроне.
        /// </summary>
        /// <param name="drone">Объект дрона, который должен быть сохранен.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если дрон был успешно сохранен; в противном случае <c>false</c>.</returns>
        Task<bool> SaveDroneAsync(Drone drone);
        /// <summary>
        /// Сохраняет информацию о маршруте дрона.
        /// </summary>
        /// <param name="droneRoute">Объект маршрута дрона, который должен быть сохранен.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если маршрут был успешно сохранен; в противном случае <c>false</c></returns>
        Task<bool> SaveRouteDroneAsync(DroneRoute droneRoute);
        /// <summary>
        /// Сохраняет информацию о путевой точке.
        /// </summary>
        /// <param name="waypoint">Объект путевой точки, который должен быть сохранен.</param>
        /// <returns>Асинхронная задача, возвращающая <c>true</c>, если путевая точка была успешно сохранена; в противном случае <c>false</c>.</returns>
        Task<bool> SaveWaypointAsync(Waypoint waypoint);
        /// <summary>
        /// Получает список всех дронов.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов дронов.</returns>
        Task<List<Drone>> GetDroneAsync();
        /// <summary>
        /// Получает список всех маршрутов дронов.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов маршрутов дронов.</returns>
        Task<List<DroneRoute>> GetRouteDroneAsync();
        /// <summary>
        /// Получает список всех путевых точек.
        /// </summary>
        /// <returns>Асинхронная задача, возвращающая список объектов путевых точек.</returns>
        Task<List<Waypoint>> GetWaypointAsync();
        /// <summary>
        /// Получает информацию о дроне по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор дрона.</param>
        /// <returns>Асинхронная задача, возвращающая объект дрона с указанным идентификатором или <c>null</c>, если дрон не найден.</returns>
        Task<Drone> GetDroneByIdAsync(int id);
        /// <summary>
        /// Получает информацию о дроне по его серийному номеру.
        /// </summary>
        /// <param name="serial">Серийный номер дрона.</param>
        /// <returns>Асинхронная задача, возвращающая объект дрона с указанным серийным номером или <c>null</c>, если дрон не найден.</returns>
        Task<Drone> GetDroneBySerialAsync(string serial);
        /// <summary>
        /// Получает информацию о маршруте дрона по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор маршрута дрона.</param>
        /// <returns>Асинхронная задача, возвращающая объект маршрута дрона с указанным идентификатором или <c>null</c>, если маршрут не найден</returns>
        Task<DroneRoute> GetRouteDroneByIdAsync(int id);
        /// <summary>
        /// Получает информацию о путевой точке по ее уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор путевой точки.</param>
        /// <returns>Асинхронная задача, возвращающая объект путевой точки с указанным идентификатором или <c>null</c>, если путевая точка не найдена.</returns>
        Task<Waypoint> GetWaypointByIdAsync(int id);
    }
}
