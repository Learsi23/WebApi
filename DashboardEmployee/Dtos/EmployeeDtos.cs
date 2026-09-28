namespace DashboardEmployee.Dtos
{
    #region request
    /// <summary>Body for POST /api/employees and PUT /api/employees/{id}.</summary>
    public sealed record EmployeeRequest(string FullName, string Email, decimal Salary, int DepartmentId);


    #endregion
    #region response
    public sealed record EmployeeResponse(int Id, string FullName, string Email, decimal Salary, string? ImageUrl, int DepartmentId, string DepartmentName);
    #endregion

    /// <summary>Query string for GET /api/employees (search, filter, sort, paging).</summary>
    public sealed class EmployeeQueryParameters
    {
        public string? Search { get; init; }
        public int? DepartmentId { get; init; }
        public string SortBy { get; init; } = "name";
        public string SortDirection { get; init; } = "asc";
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

}
