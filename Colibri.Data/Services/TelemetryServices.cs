using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colibri.Data.Context;
using Colibri.Data.Entity;
using Colibri.Data.Services.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace Colibri.Data.Services;

public class TelemetryServices(AppDbContext context) : ITelemetryServices
{
    private readonly AppDbContext _context = context;

    public async Task ClearOldTelemetriesAsync(DateTime olderThan)
    {
        var oldTelemetries = await _context.Telemetries
            .Where(t => !t.IsDeleted && t.CreatedAt < olderThan)
            .ToListAsync();

        foreach (var telemetry in oldTelemetries)
        {
            telemetry.IsDeleted = true;
            telemetry.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteTelemetriesAsync(List<int> telemetryIds)
    {
        if (telemetryIds == null || telemetryIds.Count == 0)
            return;

        var telemetriesToDelete = await _context.Telemetries
            .Where(t => telemetryIds.Contains(t.Id))
            .ToListAsync();
            
        if (telemetriesToDelete.Count > 0)
        {
            foreach (var telemetry in telemetriesToDelete)
            {
                telemetry.IsDeleted = true;
                telemetry.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteTelemetryAsync(int id)
    {
        var telemetry = await _context.Telemetries.FindAsync(id);
        if (telemetry != null)
        {
            telemetry.IsDeleted = true;
            telemetry.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetGpsStatusesAsync()
    {
        return await _context.Telemetries
            .Where(t => !t.IsDeleted && t.GpsStatus != null)
            .Select(t => t.GpsStatus)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task<List<Telemetry>> GetTelemetriesAsync(int page = 1, int pageSize = 50, DateTime? fromDate = null, DateTime? toDate = null, string search = null, bool? gpsStatus = null)
    {
        var query = _context.Telemetries
                .Where(t => !t.IsDeleted)
                .AsQueryable();

                    // Применяем фильтры
            if (fromDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= toDate.Value);
            }

            if (gpsStatus.HasValue)
            {
                query = query.Where(t => t.Gyro == gpsStatus.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => 
                    t.GpsStatus != null && t.GpsStatus.Contains(search));
            }

            // Сортировка и пагинация
            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
    }

    public async Task<Telemetry> GetTelemetryByIdAsync(int id)
    {
        return await _context.Telemetries
                .Where(t => !t.IsDeleted && t.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
    }

    public async Task<int> GetTotalCountAsync(DateTime? fromDate = null, DateTime? toDate = null, string search = null, bool? gpsStatus = null)
    {
            var query = _context.Telemetries
                .Where(t => !t.IsDeleted)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= toDate.Value);
            }

            if (gpsStatus.HasValue)
            {
                query = query.Where(t => t.Gyro == gpsStatus.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => 
                    t.GpsStatus != null && t.GpsStatus.Contains(search));
            }

            return await query.CountAsync();
    }
}
