
using DashboardEmployee.Data;
using DashboardEmployee.Dtos;
using DashboardEmployee.Entities;
using DashboardEmployee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DashboardEmployee.Services
{
    public class EmployeeService(AppDbContext db, IImageService serviceImage) : IEmployeeService
    {

        public async Task<IEnumerable<GetEmployeeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await db.Employees
                    .AsNoTracking()
                    .Select(e => new GetEmployeeResponse(
                        e.EmployeeId,
                        e.FullName,
                        e.Email,
                        e.Salary,
                        e.ImageUrl!,
                        e.Department.Name,
                        e.DepartmentId
                    ))
                    .ToListAsync(ct);
        }

        public async Task<GetEmployeeResponse> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var employee = await db.Employees
                .AsNoTracking()
                .Where(e => e.EmployeeId == id)
                .Select(e => new GetEmployeeResponse(
                        e.EmployeeId,
                        e.FullName,
                        e.Email,
                        e.Salary,
                        e.ImageUrl!,
                        e.Department.Name,
                        e.DepartmentId
                    ))
                .FirstOrDefaultAsync(ct);

            return employee!;


        }

        public async Task<GetEmployeeResponse> AddAsync(CreateEmployeeeRequest request, CancellationToken ct = default)
        {
            string imageUrl = await serviceImage.SaveImageAsync(request.Image,ct);

            var employee = new Employee
            {
                FullName = request.FullName,
                Email = request.Email,
                Salary = request.Salary,
                ImageUrl = imageUrl,
                DepartmentId = request.DepartmentId
            };

            db.Employees.Add(employee);
            await db.SaveChangesAsync(ct);

            await db.Entry(employee).Reference(e => e.Department).LoadAsync(ct);

            return new GetEmployeeResponse(
                employee.EmployeeId,
                employee.FullName,
                employee.Email,
                employee.Salary,
                employee.ImageUrl!,
                employee.Department.Name,
                employee.DepartmentId
            );

        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var employee = await db.Employees.FindAsync([id], ct);
            if (employee is null) return false;

            serviceImage.DeleteImageFile(employee.ImageUrl);

            db.Employees.Remove(employee);          
            await db.SaveChangesAsync(ct);

            return true;
        }


        public async Task<GetEmployeeResponse> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken ct = default)
        {
            var employee = await db.Employees
                 .Include(e => e.Department)
                 .FirstOrDefaultAsync(e => e.EmployeeId == id, ct);

            if (employee is null)
                return null!;

            employee.FullName = request.FullName;
            employee.Email = request.Email;
            employee.Salary = request.Salary;
            employee.DepartmentId = request.DepartmentId;

            // If a new image is supplied and differs from current image, delete old file and save new one
            if (!string.IsNullOrWhiteSpace(request.Image) && request.Image != employee.ImageUrl)
            {
                // Delete previous physical file if it exists
                serviceImage.DeleteImageFile(employee.ImageUrl);

                // Save new image and update entity property
                employee.ImageUrl = await serviceImage.SaveImageAsync(request.Image, ct);
            }

            await db.SaveChangesAsync(ct);

            return new GetEmployeeResponse(
                employee.EmployeeId,
                employee.FullName,
                employee.Email,
                employee.Salary,
                employee.ImageUrl!,
                employee.Department.Name,
                employee.DepartmentId
            );
        }

    
    }
}