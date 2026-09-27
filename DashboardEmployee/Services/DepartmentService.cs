using DashboardEmployee.Data;
using DashboardEmployee.Dtos;
using DashboardEmployee.Entities;
using DashboardEmployee.Exceptions;
using DashboardEmployee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DashboardEmployee.Services;

public sealed class DepartmentService(AppDbContext db, ILogger<DepartmentService> logger) : IDepartmentService
{
    // Expression tree that defines how to map a Department entity to a DepartmentResponse DTO.
    // Making it static and readonly compiles it once, allowing EF Core to reuse the exact same 
    // expression across different queries without reallocation overhead.
    private static readonly Expression<Func<Department, DepartmentResponse>> ToResponse = d
        => new DepartmentResponse(d.DepartmentId, d.Name, d.Employees.Count, d.CreatedAt);

    public async Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Departments
            .AsNoTracking()
            .OrderBy(o => o.Name)
            // Passing the expression to .Select() translates the projection into SQL.
            // EF Core executes a SELECT with only the required columns and a COUNT() subquery,
            // avoiding fetching unnecessary entity properties or entire child collections into memory.
            .Select(ToResponse)
            .ToListAsync(ct);
    }

    public async Task<DepartmentResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Departments
            .AsNoTracking()
            .Where(d => d.DepartmentId == id)
            .Select(ToResponse)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Department with id {id} was not found.");
    }

    public async Task<DepartmentResponse> CreateAsync(DepartmentRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        await EnsureNameIsAvailableAsync(name, excludeDepartmentId: null, ct);

        var department = new Department { Name = name };
        db.Departments.Add(department);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created department {DepartmentId}", department.DepartmentId);

        return await GetByIdAsync(department.DepartmentId, ct);
    }

    public async Task<DepartmentResponse> UpdateAsync(int id, DepartmentRequest request, CancellationToken ct = default)
    {
        var department = await FindTrackedAsync(id, ct);

        var name = request.Name.Trim();

        await EnsureNameIsAvailableAsync(name, excludeDepartmentId: id, ct);

        department.Name = name;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated department {DepartmentId}", id);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var department = await FindTrackedAsync(id, ct);

        var hasEmployees = await db.Employees.AnyAsync(e => e.DepartmentId == id, ct);

        if (hasEmployees)
            throw new ConflictException("The department still has employees. Move or delete them first.");

        db.Departments.Remove(department);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deleted department {DepartmentId}", id);
    }

    // Helper method that fetches a tracked entity by ID for mutation operations (Update/Delete).
    // Replaces boilerplate "FindAsync + null-check + throw NotFoundException" in every method.
    private async Task<Department> FindTrackedAsync(int id, CancellationToken ct)
    {
        return await db.Departments.FindAsync([id], ct)
            ?? throw new NotFoundException($"Department with id {id} was not found.");
    }

    // Encapsulates unique name validation logic for both Creation and Update.
    // When creating, excludeDepartmentId is null.
    // When updating, excludeDepartmentId ignores the current record so it doesn't conflict with itself.
    // Replaces redundant AnyAsync queries in CreateAsync and UpdateAsync methods.
    private async Task EnsureNameIsAvailableAsync(string name, int? excludeDepartmentId, CancellationToken ct)
    {
        var taken = await db.Departments.AnyAsync(d => d.Name == name && d.DepartmentId != excludeDepartmentId, ct);
        if (taken)
            throw new ConflictException($"A department named '{name}' already exists.");
    }
}

/* 
 =========================================================================================
 EXPLANATION OF PRIVATE HELPER METHODS (FindTrackedAsync & EnsureNameIsAvailableAsync)
 =========================================================================================

 These private helper methods implement the DRY (Don't Repeat Yourself) principle by centralizing 
 repetitive database queries and exception handling logic across service operations.

 -----------------------------------------------------------------------------------------
 1. FindTrackedAsync(int id, CancellationToken ct)
 -----------------------------------------------------------------------------------------
 PURPOSE:
 - Retrieves a tracked entity from Entity Framework's Change Tracker so it can be mutated (updated or deleted).
 - Automatically validates existence and throws a domain-specific `NotFoundException` if the record isn't found.

 WHAT IT REPLACES:
 Without this helper, both `UpdateAsync` and `DeleteAsync` would require duplicate code:
 
   // ❌ Repeated boilerplate in every method:
   var department = await db.Departments.FindAsync([id], ct);
   if (department is null)
   {
       throw new NotFoundException($"Department with id {id} was not found.");
   }

 With this helper, it reduces to a single clean line:
 
   // ✅ Clean & reusable:
   var department = await FindTrackedAsync(id, ct);

 -----------------------------------------------------------------------------------------
 2. EnsureNameIsAvailableAsync(string name, int? excludeDepartmentId, CancellationToken ct)
 -----------------------------------------------------------------------------------------
 PURPOSE:
 - Encapsulates uniqueness validation for entity names.
 - Supports both creation (excludeDepartmentId = null) and updates (excludeDepartmentId = id).
 - Throws a `ConflictException` if a record with the same name already exists.

 WHAT IT REPLACES:
 Without this helper, `CreateAsync` and `UpdateAsync` would require separate, duplicate EF Core queries:

   // ❌ Manual check during Update:
   var taken = await db.Departments.AnyAsync(d => d.Name == name && d.DepartmentId != id, ct);
   if (taken)
   {
       throw new ConflictException($"A department named '{name}' already exists.");
   }

 With this helper, both methods share a single implementation:

   // ✅ Reusable call for Create:
   await EnsureNameIsAvailableAsync(name, excludeDepartmentId: null, ct);

   // ✅ Reusable call for Update (ignores current entity ID to avoid self-conflict):
   await EnsureNameIsAvailableAsync(name, excludeDepartmentId: id, ct);
 =========================================================================================
*/