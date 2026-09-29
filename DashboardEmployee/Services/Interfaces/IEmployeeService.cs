using DashboardEmployee.Dtos;

namespace DashboardEmployee.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<PagedResult<EmployeeResponse>> GetPagedAsync(EmployeeQueryParameters query,CancellationToken ct = default);
        Task<EmployeeResponse> GetByIdAsync(int id, CancellationToken ct = default);
        Task<EmployeeResponse> CreateAsync(EmployeeRequest request, CancellationToken ct = default);
        Task<EmployeeResponse> UpdateAsync(int id, EmployeeRequest request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        /// <summary>
        /// /  Image    
        /// </summary>
        /// <param name="id"></param>
        /// <param name="image"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EmployeeResponse> UpdateImageAsync(int id, IFormFile image, CancellationToken ct = default);
        Task RemoveImageAsync(int id, CancellationToken ct = default);
    }
}
