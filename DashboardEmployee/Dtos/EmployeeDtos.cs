namespace DashboardEmployee.Dtos
{
    #region request
    public record CreateEmployeeeRequest(string FullName, string Email, decimal Salary, string Image, int DepartmentId);
    public record UpdateEmployeeRequest(int Id, string FullName, string Email, decimal Salary, string? Image, int DepartmentId);
    #endregion
    #region response
    public record GetEmployeeResponse(int Id, string FullName, string Email, decimal Salary, string Image, string DepartmentName, int DepartmentId);
    #endregion
}
