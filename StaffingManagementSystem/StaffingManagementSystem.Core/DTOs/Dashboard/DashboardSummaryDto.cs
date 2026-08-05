using StaffingManagementSystem.Core.DTOs.Candidates;

namespace StaffingManagementSystem.Core.DTOs.Dashboard
{
    /// <summary>
    /// Role-aware dashboard summary returned by GET /api/dashboard/summary. Only the fields
    /// relevant to the caller's role are populated — the rest are left null, matching the
    /// per-role field-nulling convention already used for CandidateDetailDto's cost fields.
    /// </summary>
    public class DashboardSummaryDto
    {
        // ---------- Admin ----------

        /// <summary>New(includes Approved)/Available/InProcess/Placed/OnHold/Blacklisted, in that order.</summary>
        public List<NameCountDto>? StatusCounts { get; set; }

        public int? PendingApprovalsCount { get; set; }

        /// <summary>Top 5 by ApprovedAtUtc, most recent first.</summary>
        public List<DashboardCandidateDto>? RecentlyApproved { get; set; }

        /// <summary>Top 5 by RejectedAtUtc, most recent first.</summary>
        public List<DashboardCandidateDto>? RecentlyRejected { get; set; }

        /// <summary>Recruiter full name -> count of candidates they currently own.</summary>
        public List<NameCountDto>? RecruiterWorkload { get; set; }

        /// <summary>Top 8 skills across the candidate database by candidate count.</summary>
        public List<NameCountDto>? TopSkills { get; set; }

        /// <summary>Self-registered candidates (OwnerRecruiterId is null) created in the last 7 days.</summary>
        public List<DashboardCandidateDto>? RecentRegistrations { get; set; }

        // ---------- Recruiter ----------

        public int? MyCandidatesCount { get; set; }

        public List<NameCountDto>? MyCandidatesByStatus { get; set; }

        public int? MyInProcessCount { get; set; }

        /// <summary>Every visible candidate (any owner) created in the last 7 days.</summary>
        public List<DashboardCandidateDto>? RecentlyAddedSystemWide { get; set; }

        // ---------- Viewer (also uses StatusCounts above) ----------

        public int? TotalVisibleCandidates { get; set; }
    }
}
