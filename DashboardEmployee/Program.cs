using DashboardEmployee.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseConfiguration(builder.Configuration);

builder.Services.AddCorsConfiguration(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignora diferencias de mayúsculas/minúsculas entre JSON y DTOs
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });


builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(CorsExtension.AllowSpecificOrigins);
app.MapControllers();

app.Run();
