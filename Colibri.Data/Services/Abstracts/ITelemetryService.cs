using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colibri.Data.Entity;

namespace Colibri.Data.Services.Abstracts;

public interface ITelemetryService
{
    Task<List<Telemetry>> GetTelemetriesAsync(int page = 1, int pageSize = 50, DateTime? fromDate = null, DateTime? toDate = null, string search = null, bool? gpsStatus = null);
        
    Task<Telemetry> GetTelemetryByIdAsync(int id);
        
    Task<int> GetTotalCountAsync(DateTime? fromDate = null, DateTime? toDate = null, string search = null, bool? gpsStatus = null);
        
    Task DeleteTelemetryAsync(int id);
        
    Task DeleteTelemetriesAsync(List<int> telemetryIds);
        
    Task ClearOldTelemetriesAsync(DateTime olderThan);
        
    Task<List<string>> GetGpsStatusesAsync();
    Task<bool> AddTelemetryAsync(Telemetry telemetry);
}
