namespace Colibri.WebApi.Models;

public class FlightCommand
{
    public bool ShouldTakeoff { get; set; }
    public float AltitudeMeters { get; set; }
    public string CommandId { get; set; }
    public bool Valid { get; set; }
}
