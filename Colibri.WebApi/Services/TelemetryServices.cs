
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;

namespace Colibri.WebApi.Services;

public class TelemetryServices : ITelemetryServices
{
    private static readonly List<TelemetryData> _telemetryDatas = [];
    private const int MAX_HISTORY = 1000;

    public async Task<List<TelemetryData>> GetRecentTelemetryAsync(int count = 10)
    {
        try
        {
            
        }
        catch (Exception ex)
        {
            // throw new InvalidOperationException("Error processing telemetry data", ex);
        }
    }

    public async Task<TelemetryResponse> ProcessTelemetryAsync(TelemetryData telemetryData)
    {
        try
        {
            if (!IsValidTelemetry(telemetryData))
            {
                return new TelemetryResponse
                {
                    Message = "Invalid telemetry data",
                    Success = false
                };
            }
                 return new TelemetryResponse
                {
                    Message = "Invalid telemetry data",
                    Success = false
                };
        }
        catch (Exception ex)
        {
            return new TelemetryResponse
            {
                Message = "Error processing telemetry data",
                Success = false
            };

            throw new InvalidOperationException("Error processing telemetry data", ex);
        }
    }

    private bool IsValidTelemetry(TelemetryData data)
    {
        if (data.Latitude < -90 || data.Latitude > 90)
            return false;
        if (data.Longitude < -180 || data.Longitude > 180)
            return false;
        if (data.BatteryPercentage < 0 || data.BatteryPercentage > 100)
            return false;
        if (data.BatteryVoltage < 0 || data.BatteryVoltage > 50)
            return false;

        return true;         
    }
}
