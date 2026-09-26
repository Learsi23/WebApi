namespace DashboardEmployee.Extensions;

public static class CorsExtension
{
    public const string AllowSpecificOrigins = "_allowSpecificOrigins";
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services,
    IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>()
        ?? throw new InvalidOperationException("'AllowedOrigins' is missing in configuration.");
        services.AddCors(options =>
        {
            options.AddPolicy(AllowSpecificOrigins, policy =>
            {
                policy.WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod();
            });
        });
        return services;
    }
}