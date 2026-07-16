namespace StaffingManagementSystem.Core.Entities
{
    /// <summary>A project entry on a candidate profile (RMS SRS Appendix A.4).</summary>
    public class CandidateProject
    {
        public Guid Id { get; set; }

        public Guid CandidateId { get; set; }

        /// <summary>Optional link to the <see cref="CandidateExperience"/> the project was done under.</summary>
        public Guid? ExperienceId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string? Role { get; set; }

        public string? DurationText { get; set; }

        /// <summary>Free-text, comma-separated technology list.</summary>
        public string? TechnologiesUsed { get; set; }

        public string? Description { get; set; }
    }
}
