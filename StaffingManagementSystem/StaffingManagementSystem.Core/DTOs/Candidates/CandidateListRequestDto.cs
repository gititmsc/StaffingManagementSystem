namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>Query parameters for GET /api/candidates (the Candidate Master list screen).</summary>
    public class CandidateListRequestDto
    {
        /// <summary>Matches full name, email, current company or any skill (contains, case-insensitive).</summary>
        public string? Search { get; set; }

        /// <summary>One of: New, Available, InProcess, Placed, OnHold, Blacklisted.</summary>
        public string? Status { get; set; }

        /// <summary>When set, only candidates owned by this recruiter are returned.</summary>
        public Guid? OwnerRecruiterId { get; set; }

        /// <summary>One of: Name, Email, Experience, Status, Added. Defaults to Added.</summary>
        public string? SortBy { get; set; }

        /// <summary>Defaults to true (newest/highest first) to match the pre-sorting default order.</summary>
        public bool SortDescending { get; set; } = true;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }
}
