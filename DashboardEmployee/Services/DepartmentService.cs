using DashboardEmployee.Data;
using DashboardEmployee.Dtos;
using DashboardEmployee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DashboardEmployee.Services
{
    public class DepartmentService(AppDbContext db) : IDepartmentService
    {
        public async Task<IEnumerable<GetDepartmentResponse>> GetAllAsync(CancellationToken ct)
        {
            var departments = await db.Departments
              .AsNoTracking()
              .Select(e => new GetDepartmentResponse(e.DepartmentId, e.Name))
              .ToListAsync(ct);

            return departments;
        }
    }
}
