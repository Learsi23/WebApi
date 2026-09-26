using DashboardEmployee.Data;
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


            return services;
        }
    }
}
