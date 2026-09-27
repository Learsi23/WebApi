using DashboardEmployee.Dtos;

namespace DashboardEmployee.Services.Interfaces
{
    public interface IDepartmentService
    {

        Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(CancellationToken ct = default);
        Task<DepartmentResponse> GetByIdAsync(int id, CancellationToken ct = default);
        Task<DepartmentResponse> CreateAsync(DepartmentRequest request, CancellationToken ct = default);
        Task<DepartmentResponse> UpdateAsync(int id, DepartmentRequest request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

    }
}
