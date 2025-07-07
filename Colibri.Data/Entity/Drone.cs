using Colibri.Data.Helpers;

namespace Colibri.Data.Entity
{
    /// <summary>
    /// Сущность дрона
    /// </summary>
    public class Drone : AudiTable
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Серийный номер
        /// </summary>
        public string Serial { get; set; }
        /// <summary>
        /// Модель
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// Изготовитель
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// Год выпуска
        /// </summary>
        public int ReleaseYear { get; set; }
        /// <summary>
        /// Полезная нагрузка
        /// </summary>
        public decimal Weight { get; set; }
        /// <summary>
        /// Максимальная скорость
        /// </summary>
        public decimal MaxRange { get; set; }
        /// <summary>
        /// Максимальное время полёта
        /// </summary>
        public decimal MaxFlightTime { get; set; }
        /// <summary>
        /// Наличие камеры
        /// </summary>
        public string CameraResolution { get; set; }
        /// <summary>
        /// Наличие GPS
        /// </summary>
        public bool Gps { get; set; }
    }
}
