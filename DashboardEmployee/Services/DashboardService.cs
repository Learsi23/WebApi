using DashboardEmployee.Data;
using DashboardEmployee.Dtos;
using DashboardEmployee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DashboardEmployee.Services
{
    public class DashboardService(AppDbContext db) : IDashboardService
    {
        public async Task<DashboardStatsResponse> GetStatsAsync(CancellationToken ct = default)
        {
            // Step 1 (SQL): count and sum per department in ONE query.
            // The (decimal?) cast matters: SUM over zero rows is NULL in SQL.

            var totals = await db.Departments
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .Select(d => new
                {
                    d.DepartmentId,
                    d.Name,
                    EmployeeCount = d.Employees.Count,
                    TotalSalary = d.Employees.Sum(e => (decimal?)e.Salary) ?? 0
                })
                .ToListAsync(ct);

            var highestSalary = await db.Employees.MaxAsync(e => (decimal?)e.Salary, ct) ?? 0;
            var lowestSalary = await db.Employees.MinAsync(e => (decimal?)e.Salary, ct) ?? 0;

            // Step 2 (C#): averages and grand totals from data already in memory.

            var departments = totals
                .Select(d => new DepartmentStatsResponse(
                d.DepartmentId,
                d.Name,
                d.EmployeeCount,
                d.TotalSalary,
                Average(d.TotalSalary, d.EmployeeCount)))
                .ToList();

            var totalEmployees = departments.Sum(d => d.EmployeeCount);
            var totalPayroll = departments.Sum(d => d.TotalSalary);

            return new DashboardStatsResponse(
                totalEmployees,
                departments.Count,
                totalPayroll,
                Average(totalPayroll, totalEmployees),
                highestSalary,
                lowestSalary,
                departments);

        }
        private static decimal Average(decimal total, int count) => count == 0 ? 0 : Math.Round(total / count, 2);
    }
}
