using Colibri.Data.Helpers;

namespace Colibri.Data.Entity
{
    /// <summary>
    /// Гео-точки маршрута
    /// </summary>
    public class Waypoint : AudiTable
    {
        /// <summary>
        /// Идентификатор маршрута
        /// </summary>
        public int DroneRouteId { get; set; }
        /// <summary>
        /// Широта
        /// </summary>
        public decimal Latitude { get; set; }
        /// <summary>
        /// Долгота
        /// </summary>
        public decimal Longitude { get; set; }
        /// <summary>
        /// Преодаление точки маршрута
        /// </summary>
        public bool IsRoute { get; set; }
    }
}
