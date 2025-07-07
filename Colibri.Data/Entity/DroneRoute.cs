using Colibri.Data.Helpers;

namespace Colibri.Data.Entity
{
    /// <summary>
    /// Маршрут дрона
    /// </summary>
    public class DroneRoute : AudiTable
    {
        /// <summary>
        /// Идентификатор дрона
        /// </summary>
        public int DroneId { get; set; }
        /// <summary>
        /// Начало маршрута - широта
        /// </summary>
        public decimal StartLatitude { get; set; }
        /// <summary>
        /// Начало маршрута - долгота
        /// </summary>
        public decimal StartLongitude { get; set; }
        /// <summary>
        /// конец маршрута - широта
        /// </summary>
        public decimal StopLatitude { get; set; }
        /// <summary>
        /// Конец маршрута - долгота
        /// </summary>
        public decimal StopLongitude { get; set; }
        /// <summary>
        /// Расстояние
        /// </summary>
        public decimal Distance { get; set; }
        /// <summary>
        /// Название маршрута
        /// </summary>
        public string Route { get; set; }
    }
}
