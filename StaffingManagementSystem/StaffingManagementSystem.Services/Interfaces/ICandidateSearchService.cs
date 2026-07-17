using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;

namespace StaffingManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Business logic for advanced candidate search and the Phase 1 standard reports
    /// (RMS SRS 3.4 Search &amp; Reporting).
    /// </summary>
    public interface ICandidateSearchService
    {
        /// <summary>Filtered, sorted, paginated candidate search.</summary>
        Task<ApiResponse<CandidateSearchResultDto>> SearchAsync(CandidateSearchRequestDto request);

        /// <summary>Every matching candidate, filtered and sorted but not paginated — used for CSV export.</summary>
        Task<List<CandidateListItemDto>> SearchAllAsync(CandidateSearchRequestDto request);

        /// <summary>Grouped counts backing the skill-wise, experience-wise and company-wise report views.</summary>
        Task<ApiResponse<CandidateReportSummaryDto>> GetReportSummaryAsync();
    }
}
