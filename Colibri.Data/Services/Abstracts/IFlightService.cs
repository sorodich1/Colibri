using Colibri.Data.Entity;
using System.Threading.Tasks;

namespace Colibri.Data.Services.Abstracts
{
    /// <summary>
    /// Интерфейс для сервиса управления полетами дронов.
    /// Предоставляет методы для добавления и обработки путевых точек, маршрутов и связанных данных.
    /// </summary>
    public interface IFlightService
    {
        /// <summary>
        /// Добавляет новую путевую точку (Waypoint) в систему.
        /// Используется для сохранения координат и информации о точке маршрута.
        /// </summary>
        /// <param name="waypoint">бъект путевой точки, содержащий координаты и дополнительные параметры маршрута.</param>
        /// <returns>Задача, представляющая асинхронную операцию добавления путевой точки. Завершается после успешного сохранения или при возникновении ошибки.</returns>
        Task AddFlightWaipoints(Waypoint waypoint);
        /// <summary>
        /// Добавляет новую регистрацию события в систему.
        /// </summary>
        /// <param name="eventReg">Объект, содержащий информацию о регистрации события, которую необходимо добавить.</param>
        /// <returns>Задача, представляющая асинхронную операцию добавления регистрации события</returns>
        Task AddEventRegistration(EventRegistration eventReg);
    }
}
