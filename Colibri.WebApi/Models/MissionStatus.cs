namespace Colibri.WebApi.Models;

/// <summary>
/// Статус калибровки датчиков
/// </summary>
public class MissionStatus
{
    public bool Completed { get; set; }
    public bool InAir { get; set; }
    public long Timestamp { get; set; }
}
