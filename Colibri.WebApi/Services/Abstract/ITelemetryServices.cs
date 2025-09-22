using System.Collections.Generic;
using System.Threading.Tasks;
using Colibri.WebApi.Models;

namespace Colibri.WebApi.Services.Abstract;

public interface ITelemetryServices
{
    Task<TelemetryResponse> ProcessTelemetryAsync(TelemetryData telemetryData);
    Task<List<TelemetryData>> GetRecentTelemetryAsync(int count = 10);
}
