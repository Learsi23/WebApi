namespace DashboardEmployee.Dtos
{
    public sealed record DashboardStatsResponse(
        int TotalEmployees,
        int TotalDepartments,
        decimal TotalPayroll,
        decimal AverageSalary,
        decimal HighestSalary,
        decimal LowestSalary,
        IReadOnlyList<DepartmentStatsResponse> Departments);

    public sealed record DepartmentStatsResponse(
        int Id,
        string Name,
        int EmployeeCount,
        decimal TotalSalary,
        decimal AverageSalary);
}
