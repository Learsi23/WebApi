using DashboardEmployee.Services;
using DashboardEmployee.Services.Interfaces;

namespace DashboardEmployee.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IImageService, ImageService>();


            return services;
        }
    }
}
