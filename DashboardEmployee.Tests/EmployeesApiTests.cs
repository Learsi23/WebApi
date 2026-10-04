using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DashboardEmployee.Dtos;
using Microsoft.AspNetCore.Mvc;
using static DashboardEmployee.Tests.ApiFactory;

namespace DashboardEmployee.Tests
{
    [Collection(ApiCollection.Name)]
    public sealed class EmployeesApiTests(ApiFactory factory)
    {
        // Using the authenticated Admin client to bypass [Authorize(Roles = Roles.Admin)] restrictions
        private readonly HttpClient _client = factory.AdminClient;

        // Every test uses its own email, so tests never collide on the unique index.
        private static EmployeeRequest NewEmployee(int departmentId = 1) =>
            new("Test Person", $"test-{Guid.NewGuid():N}@company.se", 40000, departmentId);

        private async Task<EmployeeResponse> CreateEmployeeAsync(EmployeeRequest? request = null)
        {
            var response = await _client.PostAsJsonAsync("/api/employees", request ?? NewEmployee());
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<EmployeeResponse>())!;
        }

        [Fact]
        public async Task GetAll_ReturnsPagedResult()
        {
            var result = await _client.GetFromJsonAsync<PagedResult<EmployeeResponse>>("/api/employees?page=1&pageSize=2");
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.True(result.TotalCount >= 6); // the six seeded employees
        }

        [Fact]
        public async Task GetAll_WithInvalidQuery_Returns400()
        {
            // GET request with invalid pagination parameters
            var response = await _client.GetAsync("/api/employees?page=-1&pageSize=0");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithInvalidData_Returns400WithFieldErrors()
        {
            var response = await _client.PostAsJsonAsync("/api/employees", new EmployeeRequest("", "not-an-email", -100, 999));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.Contains("fullName", problem!.Errors.Keys);
            Assert.Contains("email", problem.Errors.Keys);
            Assert.Contains("salary", problem.Errors.Keys);
            Assert.Contains("departmentId", problem.Errors.Keys); // department 999 does not exist
        }

        [Fact]
        public async Task Create_WithDuplicateEmail_Returns409()
        {
            var existing = await CreateEmployeeAsync();
            var response = await _client.PostAsJsonAsync("/api/employees", NewEmployee() with
            {
                Email = existing.Email
            });
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Update_ChangingDepartment_ReturnsNewDepartmentName()
        {
            // Regression test for the old NullReferenceException when the department changed.
            var employee = await CreateEmployeeAsync(NewEmployee(departmentId: 1));
            var request = new EmployeeRequest(employee.FullName, employee.Email, employee.Salary, DepartmentId: 2);

            var response = await _client.PutAsJsonAsync($"/api/employees/{employee.Id}", request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updated = await response.Content.ReadFromJsonAsync<EmployeeResponse>();
            Assert.Equal(2, updated!.DepartmentId);
            Assert.Equal("Human Resources", updated.DepartmentName);
        }

        [Fact]
        public async Task GetById_WhenMissing_Returns404ProblemDetails()
        {
            var response = await _client.GetAsync("/api/employees/999999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        }

        [Fact]
        public async Task Delete_RemovesEmployee()
        {
            var employee = await CreateEmployeeAsync();
            var deleteResponse = await _client.DeleteAsync($"/api/employees/{employee.Id}");
            var getResponse = await _client.GetAsync($"/api/employees/{employee.Id}");

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task UploadImage_WithRealPng_SavesFile()
        {
            var employee = await CreateEmployeeAsync();
            byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

            var response = await _client.PutAsync($"/api/employees/{employee.Id}/image", ImageContent(png, "photo.png"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updated = await response.Content.ReadFromJsonAsync<EmployeeResponse>();
            Assert.StartsWith("/uploads/employees/", updated!.ImageUrl);
            Assert.True(File.Exists(Path.Combine(factory.WebRootPath, updated.ImageUrl!.TrimStart('/'))));
        }

        [Fact]
        public async Task UploadImage_WithFakeImage_Returns400()
        {
            var employee = await CreateEmployeeAsync();
            var notAnImage = "<?php echo 'hacked'; ?>"u8.ToArray();

            var response = await _client.PutAsync($"/api/employees/{employee.Id}/image", ImageContent(notAnImage, "evil.png"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static MultipartFormDataContent ImageContent(byte[] bytes, string fileName)
        {
            var file = new ByteArrayContent(bytes);
            file.Headers.ContentType = new MediaTypeHeaderValue("image/png"); // The client can lie; the API checks the bytes
            return new MultipartFormDataContent { { file, "image", fileName } };
        }
    }
}