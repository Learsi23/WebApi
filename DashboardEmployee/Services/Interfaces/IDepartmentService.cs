using DashboardEmployee.Dtos;

namespace DashboardEmployee.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<GetDepartmentResponse>> GetAllAsync(CancellationToken ct= default);
    }
}
