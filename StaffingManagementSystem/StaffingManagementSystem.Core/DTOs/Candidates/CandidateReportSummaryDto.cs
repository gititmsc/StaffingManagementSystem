namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>
    /// Grouped counts backing the three Phase 1 standard reports (RMS SRS 3.4.2 /
    /// Section 6): skill-wise, experience-wise and company-wise.
    /// </summary>
    public class CandidateReportSummaryDto
    {
        /// <summary>Top skills by candidate count, descending.</summary>
        public List<NameCountDto> SkillCounts { get; set; } = new();

        /// <summary>Candidate counts bucketed into experience bands (0-2, 2-5, 5-10, 10+ years).</summary>
        public List<NameCountDto> ExperienceBandCounts { get; set; } = new();

        /// <summary>Top employers (current or past) by candidate count, descending.</summary>
        public List<NameCountDto> CompanyCounts { get; set; } = new();
    }
}
