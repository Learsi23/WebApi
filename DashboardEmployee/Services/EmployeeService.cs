
using DashboardEmployee.Data;
using DashboardEmployee.Dtos;
using DashboardEmployee.Entities;
using DashboardEmployee.Exceptions;
using DashboardEmployee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace DashboardEmployee.Services;

public class EmployeeService(AppDbContext db, IImageService imageService, ILogger<EmployeeService> logger) : IEmployeeService
{

    // One projection used by every query. EF translates it to SQL, so only the needed columns are read.

    private static readonly Expression<Func<Employee, EmployeeResponse>> ToResponse = e => new EmployeeResponse
    (
        e.EmployeeId,
        e.FullName,
        e.Email,
        e.Salary,
        e.ImageUrl,
        e.DepartmentId,
        e.Department.Name
    );
    public async Task<PagedResult<EmployeeResponse>> GetPagedAsync(EmployeeQueryParameters query, CancellationToken ct = default)
    {
        var employees = db.Employees.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            employees = employees.Where(e => e.FullName.Contains(search) ||
            e.Email.Contains(search));
        }
        if (query.DepartmentId is int departmentId)
            employees = employees.Where(e => e.DepartmentId == departmentId);
        var totalCount = await employees.CountAsync(ct);
        var descending = query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        var ordered = query.SortBy.ToLowerInvariant() switch
        {
            "email" => descending ? employees.OrderByDescending(e => e.Email) : employees.OrderBy(e => e.Email),

            "salary" => descending ? employees.OrderByDescending(e => e.Salary) : employees.OrderBy(e => e.Salary),

            "department" => descending ? employees.OrderByDescending(e => e.Department.Name) : employees.OrderBy(e => e.Department.Name),

            _ => descending ? employees.OrderByDescending(e => e.FullName) : employees.OrderBy(e => e.FullName),
        };
        var items = await ordered
        .ThenBy(e => e.EmployeeId) // tie-breaker: stable order between pages
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .Select(ToResponse)
        .ToListAsync(ct);
        return new PagedResult<EmployeeResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<EmployeeResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Employees
            .AsNoTracking()
            .Where(e => e.EmployeeId == id)
            .Select(ToResponse)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Employee with id {id} was not found.");
    }

    public async Task<EmployeeResponse> CreateAsync(EmployeeRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim();

        await EnsureEmailIsAvailableAsync(email, excludeEmployeeId: null, ct);

        var employee = new Employee
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Salary = request.Salary,
            DepartmentId = request.DepartmentId
        };

        db.Employees.Add(employee);

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created employee {EmployeeId}", employee.EmployeeId);

        return await GetByIdAsync(employee.EmployeeId, ct);
    }

    public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeRequest request, CancellationToken ct = default)
    {
        var employee = await FindTrackedAsync(id, ct);
        var email = request.Email.Trim();

        await EnsureEmailIsAvailableAsync(email, excludeEmployeeId: id, ct);

        employee.FullName = request.FullName.Trim();
        employee.Email = email;
        employee.Salary = request.Salary;
        employee.DepartmentId = request.DepartmentId;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated employee {EmployeeId}", id);
        // Re-read through the projection: the response always carries the NEW department name.
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var employee = await FindTrackedAsync(id, ct);
        var imageUrl = employee.ImageUrl;

        db.Employees.Remove(employee);
        await db.SaveChangesAsync(ct);

        // Only after the database commit succeeded: if SaveChanges fails, the image is still there.

        imageService.DeleteImageFile(imageUrl);
        logger.LogInformation("Deleted employee {EmployeeId}", id);

    }

    public async Task<EmployeeResponse> UpdateImageAsync(int id, IFormFile image, CancellationToken ct = default)
    {
        var employee = await FindTrackedAsync(id, ct);
        var oldImageUrl = employee.ImageUrl;

        var newImageUrl = await imageService.SaveImageAsync(image, ct);
        employee.ImageUrl = newImageUrl;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // The database was not updated, so the new file would be an orphan.
            imageService.DeleteImageFile(newImageUrl);
            throw;
        }

        imageService.DeleteImageFile(oldImageUrl);

        return await GetByIdAsync(id, ct);
    }



    public async Task RemoveImageAsync(int id, CancellationToken ct = default)
    {
        var employee = await FindTrackedAsync(id, ct);
        var oldImageUrl = employee.ImageUrl;

        employee.ImageUrl = null;

        await db.SaveChangesAsync(ct);
        imageService.DeleteImageFile(oldImageUrl);
    }




    private async Task<Employee> FindTrackedAsync(int id, CancellationToken ct)
    {
        return await db.Employees.FindAsync([id], ct)
            ?? throw new NotFoundException($"Employee with id {id} was not found.");
    }

    private async Task EnsureEmailIsAvailableAsync(string email, int? excludeEmployeeId, CancellationToken ct)
    {
        var taken = await db.Employees.AnyAsync(e => e.Email == email && e.EmployeeId != excludeEmployeeId, ct);
        if (taken)
            throw new ConflictException($"An employee with email '{email}' already exists.");
    }

}