using DashboardEmployee.Dtos;
using DashboardEmployee.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DashboardEmployee.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeeController(IEmployeeService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetALL(CancellationToken ct)
        {
            var employees = await service.GetAllAsync(ct);
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var employee = await service.GetByIdAsync(id, ct);
            if (employee is null)
                return NotFound(new { message = $"Employee with Id {id} was not found." });

            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeeRequest request, CancellationToken ct)
        {
            var result = await service.AddAsync(request, ct);

            // Fix: Point to GetById and bind route param as id = result.Id
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                new { message = "Employee created successfully.", result }
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
        {
            var result = await service.UpdateAsync(id, request, ct);

            if (result is null)
                return NotFound(new { message = $"Employee with Id {id} was not found." });

            return Ok(new { message = "Employee updated successfully.", result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id, CancellationToken ct)
        {
            var deleted = await service.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound(new { message = $"Employee with Id {id} was not found." });

            return NoContent();
        }
    }
}