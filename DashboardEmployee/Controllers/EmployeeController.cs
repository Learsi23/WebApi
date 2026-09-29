using DashboardEmployee.Dtos;
using DashboardEmployee.Services;
using DashboardEmployee.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DashboardEmployee.Controllers
{
    [ApiController]
    [Route("api/employees")]
    [Produces("application/json")]
    public class EmployeeController(IEmployeeService employeeService, IValidator<EmployeeRequest> requestValidator, IValidator<EmployeeQueryParameters> queryValidator) : ApiControllerBase
    {
        [HttpGet]
        [ProducesResponseType<PagedResult<EmployeeResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<EmployeeResponse>>> GetAll([FromQuery] EmployeeQueryParameters query, CancellationToken ct)
        {
            var validation = await queryValidator.ValidateAsync(query, ct);

            if (!validation.IsValid)
                return ValidationProblem(validation);

            return Ok(await employeeService.GetPagedAsync(query, ct));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmployeeResponse>> GetById(int id, CancellationToken ct)
        {
            return Ok(await employeeService.GetByIdAsync(id, ct));
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeResponse>> CreateEmployee(EmployeeRequest request, CancellationToken ct)
        {
            var validation = await requestValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return ValidationProblem(validation);

            var employee = await employeeService.CreateAsync(request, ct);
            // 201 + Location header pointing to GET /api/employees/{id}
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmployeeResponse>> Update(int id, EmployeeRequest request, CancellationToken ct)
        {
            var validation = await requestValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return ValidationProblem(validation);

            return Ok(await employeeService.UpdateAsync(id, request, ct));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEmployee(int id, CancellationToken ct)
        {
            await employeeService.DeleteAsync(id, ct);
            return NoContent();
        }

        [HttpPut("{id:int}/image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(ImageService.MaxFileSizeBytes + 64 * 1024)] // file + multipart overhead
        [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmployeeResponse>> UploadImage(int id, [Required] IFormFile image, CancellationToken ct)
        {
            return Ok(await employeeService.UpdateImageAsync(id, image, ct));
        }
        [HttpDelete("{id:int}/image")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveImage(int id, CancellationToken ct)
        {
            await employeeService.RemoveImageAsync(id, ct);
            return NoContent();
        }

    }
}