

using DashboardEmployee.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DashboardEmployee.Tests
{
    public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private const string ConnectionString = "Server=LEAR\\SQLEXPRESS;Database=EmployeeDB_Tests;Trusted_Connection=True;TrustServerCertificate=True;";
        // Uploaded images go to a temp folder instead of the project's wwwroot.
        public string WebRootPath { get; } = Path.Combine(Path.GetTempPath(), "dashboard-employeetests", Guid.NewGuid().ToString("N"));

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:SqlServerConnection", ConnectionString);
            builder.UseSetting("AllowedOrigins:0", "http://localhost:3000");
            builder.UseWebRoot(WebRootPath);
        }

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(WebRootPath);
            // Fresh database with the real migrations (and their seed data) for every test run.
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {

            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.EnsureDeletedAsync();
            }
            if (Directory.Exists(WebRootPath))
                Directory.Delete(WebRootPath, recursive: true);
            await base.DisposeAsync();
        }
        /// <summary>All test classes in this collection share ONE ApiFactory (one server, one database).</summary>

        [CollectionDefinition(Name)]
        public sealed class ApiCollection : ICollectionFixture<ApiFactory>
        {
            public const string Name = "Api";
        }

    }
}
