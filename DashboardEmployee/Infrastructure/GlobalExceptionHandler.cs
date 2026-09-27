using DashboardEmployee.Entities;
using DashboardEmployee.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DashboardEmployee.Infrastructure;

/// <summary>
/// Turns every unhandled exception into a ProblemDetails (RFC 9457) response.
/// Clients never see a stack trace; the full error goes to the log instead.
/// </summary>
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService,ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    // SQL Server error numbers: 2601/2627 = unique index violation, 547 = foreign key violation.
    private static readonly int[] UniqueViolationErrors = [2601, 2627];
    private const int ForeignKeyViolationError = 547;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,Exception exception, CancellationToken ct)
    {
        var (statusCode, title, detail) = exception switch
        {
            AppException appException =>
                (appException.StatusCode, appException.Title, appException.Message),

            DbUpdateException { InnerException: SqlException sql } when UniqueViolationErrors.Contains(sql.Number) =>
                (StatusCodes.Status409Conflict, "Conflict", "A record with the same unique value already exists."),

            DbUpdateException { InnerException: SqlException { Number: ForeignKeyViolationError } } =>
                (StatusCodes.Status409Conflict, "Conflict", "The operation conflicts with related data."),

            _ =>
                (StatusCodes.Status500InternalServerError, "An unexpected error occurred", null)
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning("Request failed with {StatusCode}: {Detail}", statusCode, detail);
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            }
        });
    }
}