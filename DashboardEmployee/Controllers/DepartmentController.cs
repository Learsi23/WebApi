using DashboardEmployee.Data;
using DashboardEmployee.Dtos;
using DashboardEmployee.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DashboardEmployee.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DepartmentController(IDepartmentService dService) : ControllerBase
    {

        [HttpGet]

        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var department = await dService.GetAllAsync(ct);
            return Ok(department);
        }
    }
}
