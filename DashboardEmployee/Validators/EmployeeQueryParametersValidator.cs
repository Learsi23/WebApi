// Import the namespace containing Data Transfer Objects (DTOs) like EmployeeQueryParameters.
using DashboardEmployee.Dtos;

// Import the FluentValidation framework library.
using FluentValidation;

namespace DashboardEmployee.Validators
{
    // Define a public validator class for HTTP query string parameters (search, pagination, sorting).
    public class EmployeeQueryParametersValidator : AbstractValidator<EmployeeQueryParameters>
    {
        // Whitelist of valid database column names allowed for sorting to prevent SQL Injection or unknown field errors.
        private static readonly string[] AllowedSortFields = ["name", "email", "salary", "department"];

        // Whitelist of valid sorting directions allowed by the API.
        private static readonly string[] AllowedSortDirections = ["asc", "desc"];

        // Constructor where validation rules are defined.
        public EmployeeQueryParametersValidator()
        {
            // Rule for the optional search input filter.
            RuleFor(x => x.Search)
                .MaximumLength(100); // Prevents extremely long search strings to protect database query performance.

            // Rule for the 'SortBy' query parameter.
            RuleFor(x => x.SortBy)
                .Must(value => AllowedSortFields.Contains(value, StringComparer.OrdinalIgnoreCase)) // Ensures SortBy matches one of the allowed fields (case-insensitive).
                .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}."); // Returns a clear error message listing allowed fields if invalid.

            // Rule for the 'SortDirection' query parameter.
            RuleFor(x => x.SortDirection)
                .Must(value => AllowedSortDirections.Contains(value, StringComparer.OrdinalIgnoreCase)) // Ensures value is either 'asc' or 'desc' (case-insensitive).
                .WithMessage("SortDirection must be 'asc' or 'desc'."); // Custom error message for invalid sort direction.

            // Rule for the page number in pagination.
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1); // Page number must be 1 or higher (disallows 0 or negative numbers).

            // Rule for the number of items per page.
            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100); // Ensures page size is between 1 and 100 (prevents requests for millions of records at once).
        }
    }
}