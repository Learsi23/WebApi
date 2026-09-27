namespace DashboardEmployee.Extensions
{
    /// <summary>
    /// Base class for expected errors. Each subclass knows which HTTP status it maps to,
    /// so GlobalExceptionHandler can turn it into a ProblemDetails response.
    /// </summary>

        public abstract class AppException(string message) : Exception(message)
        {
            public abstract int StatusCode { get; }
            public abstract string Title { get; }
        }
    
}
