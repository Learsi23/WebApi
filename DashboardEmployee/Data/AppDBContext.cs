using DashboardEmployee.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DashboardEmployee.Data;

// IdentityDbContext adds the ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, ...).
public partial class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automáticamente EmployeeConfiguration, DepartmentConfiguration, etc.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}