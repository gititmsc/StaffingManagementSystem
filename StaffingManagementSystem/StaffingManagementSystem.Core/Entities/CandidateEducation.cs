namespace StaffingManagementSystem.Core.Entities
{
    /// <summary>An education entry on a candidate profile (RMS SRS Appendix A.5).</summary>
    public class CandidateEducation
    {
        public Guid Id { get; set; }

        public Guid CandidateId { get; set; }

        public string Degree { get; set; } = string.Empty;

        public string Institution { get; set; } = string.Empty;

        public string? FieldOfStudy { get; set; }

        public int? StartYear { get; set; }

        public int? EndYear { get; set; }

        /// <summary>True when <see cref="EndYear"/> is an expected/in-progress completion year.</summary>
        public bool IsExpected { get; set; }

        public string? Grade { get; set; }
    }
}
