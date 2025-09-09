using Colibri.GetDirection.Helpers;

namespace Colibri.WebApi.Models
{
    /// <summary>
    /// Представляет ответ, содержащий информацию о маршруте, включая начальную и конечную точки.
    /// </summary>
    public class RouteResponse
    {
        /// <summary>
        /// Начальная точка маршрута.
        /// </summary>
        public Point Start { get; set; }
        /// <summary>
        /// Конечная точка маршрута.
        /// </summary>
        public Point End { get; set; }
    }
}
