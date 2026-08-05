namespace StaffingManagementSystem.Core.DTOs.Dashboard
{
    /// <summary>
    /// Lightweight candidate row used in dashboard "recent activity" mini-lists (recently
    /// approved/rejected, recent registrations, recently added system-wide).
    /// </summary>
    public class DashboardCandidateDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        /// <summary>Contextual per list: CreatedAtUtc, ApprovedAtUtc or RejectedAtUtc.</summary>
        public DateTime EventAtUtc { get; set; }

        public string? OwnerRecruiterName { get; set; }
    }
}
