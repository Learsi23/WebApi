namespace DashboardEmployee.Exceptions
{
    public sealed class NotFoundException(string message) : AppException(message)
    {
        public override int StatusCode => StatusCodes.Status404NotFound;
        public override string Title => "Resource not found";
    }
}
