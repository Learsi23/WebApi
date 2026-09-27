namespace DashboardEmployee.Dtos
{
    #region request
    /// <summary>Body for POST /api/departments and PUT /api/departments/{id}.</summary>
    public sealed record DepartmentRequest(string Name);
    #endregion
    #region response
    public sealed record DepartmentResponse(int Id, string Name, int EmployeeCount, DateTimeOffset CreatedAt);
    #endregion
}
