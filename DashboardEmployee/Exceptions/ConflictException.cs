namespace DashboardEmployee.Exceptions
{
    public sealed class ConflictException(string message) : AppException(message)
    {
        public override int StatusCode => StatusCodes.Status409Conflict;
        public override string Title => "Conflict";
    }

}
