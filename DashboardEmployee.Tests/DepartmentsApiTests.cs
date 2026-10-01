using DashboardEmployee.Dtos;
using DashboardEmployee.Entities;
using System.Net;
using System.Net.Http.Json;
using static DashboardEmployee.Tests.ApiFactory;
namespace DashboardEmployee.Tests;

[Collection(ApiCollection.Name)]
public sealed class DepartmentsApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    [Fact]
    public async Task Create_ThenDelete_EmptyDepartment()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/departments", new
        DepartmentRequest($"Dept {Guid.NewGuid():N}"));
        var department = await createResponse.Content.ReadFromJsonAsync<DepartmentResponse>();
        var deleteResponse = await _client.DeleteAsync($"/api/departments/{department!.Id}");
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
    [Fact]
    public async Task Create_WithDuplicateName_Returns409()
    {
        var response = await _client.PostAsJsonAsync("/api/departments", new
        DepartmentRequest("Finance"));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    [Fact]
    public async Task Delete_DepartmentWithEmployees_Returns409()
    {
        var response = await _client.DeleteAsync("/api/departments/1"); // seeded department with employees
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DashboardStats_ReturnsTotals()
    {
        var stats = await _client.GetFromJsonAsync<DashboardStatsResponse>("/api/dashboard/stats");
        Assert.NotNull(stats);
        Assert.True(stats.TotalEmployees >= 6);
        Assert.Equal(stats.TotalEmployees, stats.Departments.Sum(d => d.EmployeeCount));
    }
}
