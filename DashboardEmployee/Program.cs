using DashboardEmployee.Data;
using DashboardEmployee.Extensions;
using DashboardEmployee.Infrastructure;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddAuth();
builder.Services.AddApplicationServices();
builder.Services.AddCorsConfiguration(builder.Configuration);

builder.Services.AddControllers(opt =>
{
    // FluentValidation is the only source of validation messages.
    // Without this, MVC adds a hidden [Required] to every non-nullable string.
    opt.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks (endast en registrering behövs)
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

// Always English messages. Otherwise FluentValidation follows the OS language (Swedish on this PC).
ValidatorOptions.Global.LanguageManager.Enabled = false;

var app = builder.Build();

// FIRST middleware, right after Build():
app.UseExceptionHandler(); // catches exceptions from everything below
app.UseStatusCodePages();   // empty 404/405 responses become ProblemDetails too

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseCors(CorsExtension.AllowSpecificOrigins);
app.UseAuthentication(); // who are you? (reads the auth cookie)
app.UseAuthorization();  // are you allowed? ([Authorize], fallback policy)
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health"); // Exponerar health check på /health
if (app.Environment.IsDevelopment())
{
    await IdentitySeeder.SeedAsync(app.Services);
}

app.Run();