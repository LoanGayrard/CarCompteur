using CarCompteur.Dtos;

namespace CarCompteur.Services;

public interface IParkingService
{
    Task<int> AddCarAsync(CancellationToken ct = default);
    Task<int> RemoveCarAsync(CancellationToken ct = default);
    Task<int> GetCurrentCountAsync(CancellationToken ct = default);
    Task<DailyStatsResponse> GetDailyStatsAsync(DateOnly date, CancellationToken ct = default);
}
