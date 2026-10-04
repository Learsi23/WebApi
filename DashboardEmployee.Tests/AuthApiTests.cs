using DashboardEmployee.Data;
using DashboardEmployee.Dtos;
using System.Net;
using System.Net.Http.Json;
using static DashboardEmployee.Tests.ApiFactory;

namespace DashboardEmployee.Tests
{
    [Collection(ApiCollection.Name)]
    public sealed class AuthApiTests(ApiFactory factory)
    {

        [Fact]
        public async Task AnonymousRequest_Returns401()
        {
            var anonymous = factory.CreateClient();
            var response = await anonymous.GetAsync("/api/employees");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            var anonymous = factory.CreateClient();
            var response = await anonymous.PostAsJsonAsync("/api/auth/login", new
            LoginRequest(IdentitySeeder.ViewerEmail, "wrong-password"));
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Me_ReturnsEmailAndRoles()
        {
            var me = await factory.AdminClient.GetFromJsonAsync<CurrentUserResponse>("/api/auth/me");
            Assert.Equal(IdentitySeeder.AdminEmail, me!.Email);
            Assert.Contains("Admin", me.Roles);
        }

        [Fact]
        public async Task Viewer_CanRead_ButCannotWrite()
        {
            var readResponse = await factory.ViewerClient.GetAsync("/api/employees");
            var writeResponse = await factory.ViewerClient.PostAsJsonAsync("/api/departments", new
            DepartmentRequest("Viewer Dept"));
            Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, writeResponse.StatusCode);
        }
    }
}
