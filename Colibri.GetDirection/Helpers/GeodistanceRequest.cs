using System.Collections.Generic;

namespace Colibri.GetDirection.Helpers
{
    public class GeodistanceRequest
    {
        /// <summary>
        /// Серийный номер
        /// </summary>
        public int Serial { get; set; }
        /// <summary>
        /// Начальная точка
        /// </summary>
        public Point Start { get; set; }
        /// <summary>
        /// Конечная точка
        /// </summary>
        public Point End { get; set; }
        /// <summary>
        /// Расстояние между точками
        /// </summary>
        public double Distance { get; set; }
        /// <summary>
        /// Маршрут
        /// </summary>
        public string Route { get; set; }
        /// <summary>
        /// Список точек маршрута
        /// </summary>
        public List<Point> RoutePoints { get; set; }
    }
}
