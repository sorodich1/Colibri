using System;

namespace Colibri.WebApi.Models;

public class DroneConfig
{
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public int Priority { get; set; } // Приоритет подключения
        public bool IsActive { get; set; }
}
