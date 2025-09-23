
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colibri.Data.Entity;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;

namespace Colibri.WebApi.Services;

public class TelemetryServices(ILoggerService logger) : ITelemetryServices
{
    private static readonly List<TelemetryData> _telemetryDatas = [];
    private readonly ILoggerService _logger = logger;
    private const int MAX_HISTORY = 1000;

    public async Task<List<TelemetryData>> GetRecentTelemetryAsync(int count = 10)
    {
        return await Task.FromResult(_telemetryDatas
                                     .OrderByDescending(t => t.Timestamp)
                                     .Take(count)
                                     .ToList());
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

            var calibration = ParseCalibrationStatus(telemetryData.CalibrationStatus);

                Telemetry data = new()
                {
                    Altitude = telemetryData.Altitude,
                    Latitude = telemetryData.Latitude,
                    Longitude = telemetryData.Longitude,
                    BatteryPercentage = telemetryData.BatteryPercentage,
                    BatteryVoltage = telemetryData.BatteryVoltage,
                    GpsStatus = telemetryData.GpsStatus,
                    RelativeAltitude = telemetryData.RelativeAltitude,
                    Accel = calibration.Accelerometer,
                    Gyro = calibration.Gyro,
                    Mag = calibration.Magnetometer
                };

                await SaveToHistoryAsync(telemetryData);

                await _logger.AddTelemetryAsync(data);

                return new TelemetryResponse
                {
                    Message = "Telemetry received successfully",
                    Success = true
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

    public CalibrationStatus ParseCalibrationStatus(string status)
    {
        var part = status.Split(' ');

        return new CalibrationStatus
        {
            Gyro = part[0].Contains("OK"),
            Accelerometer = part[1].Contains("OK"),
            Magnetometer = part[2].Contains("OK")
        };
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

    private async Task SaveToHistoryAsync(TelemetryData data)
    {
        await Task.Run(() => 
        {
            lock(_telemetryDatas)
            {
                _telemetryDatas.Add(data);

                if(_telemetryDatas.Count > MAX_HISTORY)
                {
                    _telemetryDatas.RemoveRange(0, _telemetryDatas.Count - MAX_HISTORY);
                }
            }
        });
    }
}
