using System;
using System.Collections.Generic;

namespace Colibri.WebApi.Models;

public class TelemetryEntry
{
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double RelativeAltitude { get; set; }
        public double BatteryVoltage { get; set; }
        public double BatteryPercentage { get; set; }
        public bool Gyro { get; set; }
        public bool Accel { get; set; }
        public bool Mag { get; set; }
        public string GpsStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Дополнительные вычисляемые свойства
        public string Coordinates => $"{Latitude:F6}, {Longitude:F6}";
        public string BatteryInfo => $"{BatteryPercentage:F1}% ({BatteryVoltage:F2}V)";
        public string SensorStatus => GetSensorStatus();
        
        private string GetSensorStatus()
        {
            var sensors = new List<string>();
            if (Gyro) sensors.Add("Гиро");
            if (Accel) sensors.Add("Акселерометр");
            if (Mag) sensors.Add("Магнитометр");
            return sensors.Count > 0 ? string.Join(", ", sensors) : "Нет активных";
        }
}
