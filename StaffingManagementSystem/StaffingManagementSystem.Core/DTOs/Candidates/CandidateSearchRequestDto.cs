namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>
    /// Query parameters for GET /api/reports/search and /api/reports/search/export
    /// (RMS SRS 3.4.1 Candidate Search / 3.4.2 Standard Reports).
    /// </summary>
    public class CandidateSearchRequestDto
    {
        /// <summary>Skill names to filter by (case-insensitive). Empty/omitted means no skill filter.</summary>
        public List<string>? Skills { get; set; }

        /// <summary>"AND" (candidate must have every listed skill) or "OR" (any one is enough). Default "OR".</summary>
        public string SkillMatchMode { get; set; } = "OR";

        public decimal? MinExperience { get; set; }

        public decimal? MaxExperience { get; set; }

        /// <summary>Matches any current or past employer name (contains, case-insensitive).</summary>
        public string? Company { get; set; }

        /// <summary>Matches the candidate's current job title (contains, case-insensitive).</summary>
        public string? Designation { get; set; }

        public string? Location { get; set; }

        /// <summary>One of: New, Available, InProcess, Placed, OnHold, Blacklisted.</summary>
        public string? Status { get; set; }

        /// <summary>One of: Experience, Name, Created. Default "Created".</summary>
        public string SortBy { get; set; } = "Created";

        public bool SortDescending { get; set; } = true;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
