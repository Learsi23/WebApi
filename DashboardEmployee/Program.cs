using DashboardEmployee.Extensions;
using DashboardEmployee.Infrastructure;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseConfiguration(builder.Configuration);
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


// Always English messages. Otherwise FluentValidation follows the OS language (Swedish on this PC).
ValidatorOptions.Global.LanguageManager.Enabled = false;

var app = builder.Build();

// FIRST middleware, right after Build():
app.UseExceptionHandler(); // catches exceptions from everything below 
app.UseStatusCodePages(); // empty 404/405 responses become ProblemDetails too

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsExtension.AllowSpecificOrigins);
app.UseAuthorization();
app.MapControllers();

app.Run();