using DashboardEmployee.Services;
using DashboardEmployee.Services.Interfaces;
using FluentValidation;

namespace DashboardEmployee.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IDashboardService, DashboardService>();

            // Registers every AbstractValidator<T> in this project as IValidator<T>.
            services.AddValidatorsFromAssemblyContaining<Program>();

            return services;
        }
    }
}
