using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colibri.Data.Entity;

namespace Colibri.Data.Services.Abstracts;

public interface ITelemetryService
{
    Task<Telemetry> SaveTelemetryAsync(Telemetry telemetry);
    Task<Telemetry> GetLatestTelemetryAsync();
    Task<List<Telemetry>> GetTelemetryByPeriodAsync(DateTime from, DateTime to);
    Task<object> GetTelemetryStatsAsync(DateTime from, DateTime to);
}
