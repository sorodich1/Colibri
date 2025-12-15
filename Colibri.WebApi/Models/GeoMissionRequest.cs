using System;
using System.Collections.Generic;

namespace Colibri.WebApi.Models;

public class GeoMissionRequest
{
    public List<GeoPoint> Waypoints { get; set; }
}
