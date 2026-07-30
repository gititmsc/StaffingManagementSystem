namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>Query parameters for GET /api/candidates (the Candidate Master list screen).</summary>
    public class CandidateListRequestDto
    {
        /// <summary>Matches full name, email, current company or any skill (contains, case-insensitive).</summary>
        public string? Search { get; set; }

        /// <summary>One of: New, Available, InProcess, Placed, OnHold, Blacklisted.</summary>
        public string? Status { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }
}
