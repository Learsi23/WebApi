using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace DashboardEmployee.Controllers
{
    /// <summary>Shared helpers for all API controllers.</summary>
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Returns a 400 ValidationProblemDetails with the same shape ASP.NET Core uses for its own
        /// model-binding errors: { "errors": { "fullName": ["..."] } }.
        /// </summary>
        /// 
        protected ActionResult ValidationProblem(ValidationResult result)
        {
            var errors = new ModelStateDictionary();
            // Keys in camelCase ("fullName") so they match the JSON property names the client sent.
            foreach (var error in result.Errors)
                errors.AddModelError(JsonNamingPolicy.CamelCase.ConvertName(error.PropertyName),
                error.ErrorMessage);
            return ValidationProblem(errors);
        }
    }
}
