

using DashboardEmployee.Data;
using DashboardEmployee.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace DashboardEmployee.Tests
{
    public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public const string AdminPassword = "Test-Admin-Pass-1";
        public const string ViewerPassword = "Test-Viewer-Pass-1";
        private const string ConnectionString = "Server=LEAR\\SQLEXPRESS;Database=EmployeeDB_Tests;Trusted_Connection=True;TrustServerCertificate=True;";

        /// <summary>Logged in as admin@company.se (role Admin). Its cookie container keeps the auth cookie.</summary>
        public HttpClient AdminClient { get; private set; } = null!;
        /// <summary>Logged in as viewer@company.se (role Viewer).</summary>
        public HttpClient ViewerClient { get; private set; } = null!;

        // Uploaded images go to a temp folder instead of the project's wwwroot.
        public string WebRootPath { get; } = Path.Combine(Path.GetTempPath(), "dashboard-employeetests", Guid.NewGuid().ToString("N"));

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:SqlServerConnection", ConnectionString);
            builder.UseSetting("AllowedOrigins:0", "http://localhost:5173");
            builder.UseSetting("SeedUser:AdminPassword", AdminPassword);
            builder.UseSetting("SeedUser:ViewerPassword", ViewerPassword);
            builder.UseWebRoot(WebRootPath);
        }

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(WebRootPath);
            // Fresh database with the real migrations (and their seed data) for every test run.
            using (var scope = Services.CreateScope()) 
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.EnsureDeletedAsync();
                await db.Database.MigrateAsync();
            };

            await IdentitySeeder.SeedAsync(Services);

            AdminClient = await CreateLoggedInClientAsync(IdentitySeeder.AdminEmail, AdminPassword);
            ViewerClient = await CreateLoggedInClientAsync(IdentitySeeder.ViewerEmail, ViewerPassword);

        }

        private async Task<HttpClient> CreateLoggedInClientAsync(string email, string password)
        {
            var client = CreateClient(); // handles cookies like a browser

            var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email,password));
            response.EnsureSuccessStatusCode();
            return client;
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
