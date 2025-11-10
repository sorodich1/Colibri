using System;

namespace Colibri.WebApi.Models;

public class DroneConnectionResult
{
    public bool Success { get; set; }
    public string DroneUrl { get; set; }
    public string ErrorMessage { get; set; }
    public TimeSpan ResponseTime { get; set; }
}
