namespace DashboardEmployee.Extensions
{
    public sealed class BadRequestException(string message) : AppException(message)
    {
        public override int StatusCode => StatusCodes.Status400BadRequest;
        public override string Title => "Bad request";
    }
}
