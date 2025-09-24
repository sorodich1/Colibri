using System.Collections.Generic;

namespace Colibri.GetDirection.Data
{
    /// <summary>
    /// Представляет географическую точку с координатами широты и долготы
    /// </summary>
    public class GeoPoint
    {
        /// <summary>
        /// Широта точки в градусах
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// Долгота точки в градусах
        /// </summary>
        public double Longitude { get; set; }
    }

    /// <summary>
    /// Элемент миссии - отдельная точка или команда в плане полета
    /// </summary>
    public class MissionItem
    {
        /// <summary>
        /// Высота над уровнем моря относительно рельефа (может быть null)
        /// </summary>
        public object AMSLAltAboveTerrain { get; set; }
        /// <summary>
        /// Высота точки в метрах
        /// </summary>
        public double Altitude { get; set; }
        /// <summary>
        /// Режим высоты (0-абсолютная, 1-относительная и т.д.)
        /// </summary>
        public int AltitudeMode { get; set; }
        /// <summary>
        /// Автоматически переходить к следующей точке после выполнения
        /// </summary>
        public bool AutoContine { get; set; }
        /// <summary>
        /// Код команды MAVLink
        /// </summary>
        public int Command { get; set; }
        /// <summary>
        /// Идентификатор точки для прыжка (для циклов)
        /// </summary>
        public int DoJumpId { get; set; }
        /// <summary>
        /// Система координат (0-глобальная, 1-локальная и т.д.)
        /// </summary>
        public int Frame { get; set; }
        /// <summary>
        /// Параметры команды (зависит от типа команды)
        /// </summary>
        public List<object> Params { get; set; }
        /// <summary>
        ///  Тип элемента миссии
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// Полная миссия - план полета для БПЛА
    /// </summary>
    public class Mission
    {
        /// <summary>
        /// Крейсерская скорость в м/с
        /// </summary>
        public double CruiseSpeed { get; set; }
        /// <summary>
        /// Тип прошивки автопилота
        /// </summary>
        public int FirmwareType { get; set; }
        /// <summary>
        /// Глобальный режим высоты для плана
        /// </summary>
        public int GlobalPlanAltitudeMode { get; set; }
        /// <summary>
        ///  Скорость в режиме зависания в м/с
        /// </summary>
        public double HoverSpeed { get; set; }
        /// <summary>
        /// Список элементов миссии (точек маршрута)
        /// </summary>
        public List<MissionItem> Items { get; set; }
        /// <summary>
        /// Планируемая домашняя позиция [широта, долгота, высота]
        /// </summary>
        public List<double> PlannedHomePosition { get; set; }
        /// <summary>
        /// Тип транспортного средства (1-самолет, 2-вертолет, 3-мультикоптер и т.д.)
        /// </summary>
        public int VehicleType { get; set; }
        /// <summary>
        /// Версия формата миссии
        /// </summary>
        public int Version { get; set; }
    }

    /// <summary>
    /// Модель плана полета - корневой контейнер для данных миссии
    /// </summary>
    public class PlanModel
    {
        /// <summary>
        /// Тип файла (например, "Plan")
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// Название наземной станции управления
        /// </summary>
        public string GroundStation { get; set; }
        /// <summary>
        /// Данные миссии
        /// </summary>
        public Mission Mission { get; set; }
        /// <summary>
        /// Версия формата плана
        /// </summary>
        public int Version { get; set; }
    }
}
