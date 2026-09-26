namespace DashboardEmployee.Extensions
{
    public static class CorsExtension
    {

        public static readonly string AllowSpecificOrigins = "_allowSpecificOrigins";

        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("AllowOrigins").Get<string[]>()
                 ?? ["http://localhost:3000", "https://localhost:3000"];

            services.AddCors(opt =>
            {
                opt.AddPolicy(name: AllowSpecificOrigins, policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Optional: required if using cookies/sessions
                });
            });

            return services;
        }

    }
}
