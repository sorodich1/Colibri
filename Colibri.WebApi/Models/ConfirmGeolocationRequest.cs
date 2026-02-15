using System;

namespace Colibri.WebApi.Models;

public class ConfirmGeolocationRequest
{
    public GeoPoint GeoPoint { get; set; }
    public int OrderId { get; set; }
}
