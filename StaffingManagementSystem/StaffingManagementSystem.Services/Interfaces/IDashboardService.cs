using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Dashboard;

namespace StaffingManagementSystem.Services.Interfaces
{
    /// <summary>Business logic for the role-aware Dashboard summary widgets.</summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Returns a summary with only the fields relevant to <paramref name="actorRole"/>
        /// populated — the rest are left null.
        /// </summary>
        Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(string actorRole, Guid actorUserId);
    }
}
