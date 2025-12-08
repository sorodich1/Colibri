using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colibri.Data.Context;
using Colibri.Data.Entity;
using Colibri.Data.Services.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace Colibri.Data.Services;

public class TelemetryService(AppDbContext context) : ITelemetryService
{
    private readonly AppDbContext _context = context;

    public async Task<Telemetry> GetLatestTelemetryAsync()
    {
        return await _context.Telemetries
            .Where(t => !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Telemetry>> GetTelemetryByPeriodAsync(DateTime from, DateTime to)
    {
        return await _context.Telemetries
            .Where(t => !t.IsDeleted && t.CreatedAt >= from && t.CreatedAt <= to)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<object> GetTelemetryStatsAsync(DateTime from, DateTime to)
    {
        var telemetryData = await GetTelemetryByPeriodAsync(from, to);
            
        if (!telemetryData.Any())
        {
            return new { Message = "No data for the selected period" };
        }

        return new
        {
            TotalRecords = telemetryData.Count,
            FirstRecord = telemetryData.First().CreatedAt,
            LastRecord = telemetryData.Last().CreatedAt,
            BatteryStats = new
            {
                AverageVoltage = telemetryData.Average(t => t.BatteryVoltage),
                MinVoltage = telemetryData.Min(t => t.BatteryVoltage),
                MaxVoltage = telemetryData.Max(t => t.BatteryVoltage),
                AveragePercentage = telemetryData.Average(t => t.BatteryPercentage)
            },
            PositionStats = new
            {
                AverageAltitude = telemetryData.Average(t => t.Altitude),
                MinAltitude = telemetryData.Min(t => t.Altitude),
                MaxAltitude = telemetryData.Max(t => t.Altitude)
            }
        };
    }

    public async Task<Telemetry> SaveTelemetryAsync(Telemetry telemetry)
    {
        try
        {
            await _context.Telemetries.AddAsync(telemetry);
            await _context.SaveChangesAsync();

            return telemetry;
        }
        catch (Exception ex)
        {
            throw new Exception("Error saving telemetry", ex);
        }
    }
}