namespace Colibri.ConnectNetwork.Data
{
    /// <summary>
    /// Представляет данные GPS в формате JSON, содержащие информацию о местоположении и состоянии GPS-устройства.
    /// </summary>
    public class GpsJson
    {
        /// <summary>
        /// Широта (Latitude) в виде строки.
        /// </summary>
        public string Lat { get; set; }
        /// <summary>
        /// Долгота (Longitude) в виде строки.
        /// </summary>
        public string Lon { get; set; }
        /// <summary>
        /// Высота над уровнем моря (Altitude) в виде строки.
        /// </summary>
        public string Alt { get; set; }
        /// <summary>
        /// Скорость движения, измеренная GPS, в виде строки.
        /// </summary>
        public string Speed { get; set; }
        /// <summary>
        /// Курс или направление движения, в виде строки.
        /// </summary>
        public string Course { get; set; }
        /// <summary>
        /// Количество видимых спутников, используемых для определения позиции.
        /// </summary>
        public string Sats { get; set; }
        /// <summary>
        ///  Качество фиксации GPS-сигнала.
        /// </summary>
        public string FixQuality { get; set; }
        /// <summary>
        /// Показатель качества HDOP (Horizontal Dilution of Precision).
        /// </summary>
        public string Hdop { get; set; }
        /// <summary>
        /// Метка времени, связанная с данными GPS, в виде строки.
        /// </summary>
        public string Timestamp { get; set; }
    }
}
