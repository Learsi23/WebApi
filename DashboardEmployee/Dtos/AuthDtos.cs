namespace DashboardEmployee.Dtos
{
    public sealed record LoginRequest(string Email, string Password);

    public sealed record CurrentUserResponse(string Email, IReadOnlyList<string> Roles);
}
