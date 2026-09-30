using DashboardEmployee.Dtos;

namespace DashboardEmployee.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsResponse> GetStatsAsync(CancellationToken ct = default);
    }
}
