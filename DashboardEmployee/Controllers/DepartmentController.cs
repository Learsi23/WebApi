using DashboardEmployee.Dtos;
using DashboardEmployee.Infrastructure;
using DashboardEmployee.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashboardEmployee.Controllers
{

    [Route("api/departments")]
    [ApiController]
    [Produces("application/json")]
    public sealed class DepartmentController(IDepartmentService dService, IValidator<DepartmentRequest> requestValidator) : ApiControllerBase
    {

        [HttpGet]
        [ProducesResponseType<IReadOnlyList<DepartmentResponse>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<DepartmentResponse>>> GetAll(CancellationToken ct)
        {
            return Ok(await dService.GetAllAsync(ct));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType<DepartmentResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DepartmentResponse>> GetById(int id, CancellationToken ct)
        {
            return Ok(await dService.GetByIdAsync(id, ct));
        }
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType<DepartmentResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<DepartmentResponse>> Create(DepartmentRequest request,
        CancellationToken ct)
        {
            var validation = await requestValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return ValidationProblem(validation);
            var department = await dService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
        }
        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType<DepartmentResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<DepartmentResponse>> Update(int id, DepartmentRequest request,CancellationToken ct)
        {
            var validation = await requestValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return ValidationProblem(validation);
            return Ok(await dService.UpdateAsync(id, request, ct));
        }
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await dService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}
