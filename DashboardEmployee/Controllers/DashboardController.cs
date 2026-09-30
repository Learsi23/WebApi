using DashboardEmployee.Dtos;
using DashboardEmployee.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DashboardEmployee.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Produces("application/json")]
    public class DashboardController(IDashboardService dashboardService) : ApiControllerBase
    {
        [HttpGet("stats")]
        [ProducesResponseType<DashboardStatsResponse>(StatusCodes.Status200OK)]
        public async Task<ActionResult<DashboardStatsResponse>> GetStats(CancellationToken ct)
        {
            return Ok(await dashboardService.GetStatsAsync(ct));
        }
    }
}
