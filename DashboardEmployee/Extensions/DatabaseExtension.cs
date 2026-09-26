using DashboardEmployee.Data;
using DashboardEmployee.Services;
using DashboardEmployee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DashboardEmployee.Extensions
{
    public static class DatabaseExtension
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SqlServerConnection");

            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(connectionString);
            });

            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IImageService, ImageService>();

            return services;
        }
    }
}
