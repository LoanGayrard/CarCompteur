using CarCompteur.Data;
using CarCompteur.Domain;
using CarCompteur.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CarCompteur.Services;

public class ParkingService(ParkingDbContext db) : IParkingService
{
    private readonly ParkingDbContext _db = db;

    private static readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<int> AddCarAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            _db.ParkingEvents.Add(new ParkingEvent
            {
                TimestampUtc = DateTime.UtcNow,
                Type = ParkingEventType.In
            });

            await _db.SaveChangesAsync(ct);
            return await GetCurrentCountAsync(ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<int> RemoveCarAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var current = await GetCurrentCountAsync(ct);
            if (current <= 0)
                throw new InvalidOperationException("Parking vide: impossible de retirer une voiture.");

            _db.ParkingEvents.Add(new ParkingEvent
            {
                TimestampUtc = DateTime.UtcNow,
                Type = ParkingEventType.Out
            });

            await _db.SaveChangesAsync(ct);
            return current - 1;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<int> GetCurrentCountAsync(CancellationToken ct = default)
    {
        // Occupation = IN - OUT
        var inCount = await _db.ParkingEvents.CountAsync(e => e.Type == ParkingEventType.In, ct);
        var outCount = await _db.ParkingEvents.CountAsync(e => e.Type == ParkingEventType.Out, ct);
        return inCount - outCount;
    }

    public async Task<DailyStatsResponse> GetDailyStatsAsync(DateOnly date, CancellationToken ct = default)
    {
        // Ici "jour" = UTC (cohérent car on stocke TimestampUtc)
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);

        var inBefore = await _db.ParkingEvents.CountAsync(e =>
            e.Type == ParkingEventType.In && e.TimestampUtc < start, ct);

        var outBefore = await _db.ParkingEvents.CountAsync(e =>
            e.Type == ParkingEventType.Out && e.TimestampUtc < start, ct);

        var startOfDay = inBefore - outBefore;

        var entries = await _db.ParkingEvents.CountAsync(e =>
            e.Type == ParkingEventType.In && e.TimestampUtc >= start && e.TimestampUtc < end, ct);

        var exits = await _db.ParkingEvents.CountAsync(e =>
            e.Type == ParkingEventType.Out && e.TimestampUtc >= start && e.TimestampUtc < end, ct);

        var net = entries - exits;
        var endOfDay = startOfDay + net;

        return new DailyStatsResponse(date, startOfDay, entries, exits, net, endOfDay);
    }
}
