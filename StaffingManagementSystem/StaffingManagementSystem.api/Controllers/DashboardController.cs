using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Dashboard;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Api.Controllers
{
    /// <summary>
    /// Role-aware Dashboard summary — thin controller, all logic in IDashboardService.
    /// Open to every authenticated role; the response shape differs per role (see
    /// DashboardSummaryDto), not the authorization.
    /// </summary>
    [ApiController]
    [Route("api/dashboard")]
    [Produces("application/json")]
    [Authorize]
    public sealed class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _dashboardService.GetSummaryAsync(GetActingRole(), GetActingUserId());
            return Ok(result);
        }

        private Guid GetActingUserId()
            => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private string GetActingRole() => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
