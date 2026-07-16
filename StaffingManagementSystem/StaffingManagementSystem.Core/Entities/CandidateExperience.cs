using StaffingManagementSystem.Core.Enums;

namespace StaffingManagementSystem.Core.Entities
{
    /// <summary>A past or current employer entry on a candidate profile (RMS SRS Appendix A.3).</summary>
    public class CandidateExperience
    {
        public Guid Id { get; set; }

        public Guid CandidateId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public EmploymentType? EmploymentType { get; set; }

        public DateTime StartDate { get; set; }

        /// <summary>Null when <see cref="IsCurrent"/> is true.</summary>
        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        public string? Location { get; set; }

        public string? Description { get; set; }
    }
}
