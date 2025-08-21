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
        /// Широта стартовая
        /// </summary>
        public decimal LatitudeStart { get; set; }
        /// <summary>
        /// Долгота стартовая
        /// </summary>
        public decimal LongitudeStart { get; set; }
        /// <summary>
        /// Широта конечная
        /// </summary>
        public decimal LatitudeEnd { get; set; }
        /// <summary>
        /// Долгота конечная
        /// </summary>
        public decimal LongitudeEnd { get; set; }
        /// <summary>
        /// Дистанция
        /// </summary>
        public decimal Distance { get; set; }
        /// <summary>
        /// Файл маршрута
        /// </summary>
        public decimal RouteFile { get; set; }
        /// <summary>
        /// Преодаление точки маршрута
        /// </summary>
        public bool IsRoute { get; set; }
    }
}
