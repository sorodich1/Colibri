namespace Colibri.WebApi.Models;

public class GeoPoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; } = 10; // Высота по умолчанию
}
