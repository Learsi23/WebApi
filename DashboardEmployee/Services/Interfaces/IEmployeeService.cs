using DashboardEmployee.Dtos;

namespace DashboardEmployee.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<GetEmployeeResponse>> GetAllAsync(CancellationToken ct = default);
        Task<GetEmployeeResponse> GetByIdAsync(int id, CancellationToken ct = default);
        Task<GetEmployeeResponse> AddAsync(CreateEmployeeeRequest request, CancellationToken ct = default);
        Task<GetEmployeeResponse> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }
}
