using System.Text.Json.Serialization;

public class DroneCommand
{
    [JsonPropertyName("takeoff")]
    public bool Takeoff { get; set; }
    
    [JsonPropertyName("altitude")]
    public float Altitude { get; set; }
}