using StaffingManagementSystem.Core.Enums;

namespace StaffingManagementSystem.Core.Entities
{
    /// <summary>
    /// The core candidate profile — the reusable talent-database record described in the
    /// RMS Requirements Specification, Section 3.3 / Appendix A.
    /// </summary>
    public class Candidate
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? CurrentLocation { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public CandidateStatus Status { get; set; } = CandidateStatus.New;

        public CandidateSource? Source { get; set; }

        /// <summary>Recruiter who owns/added this candidate (RMS SRS 3.3.7).</summary>
        public Guid OwnerRecruiterId { get; set; }

        /// <summary>
        /// Auto-calculated from <see cref="Experience"/> whenever the candidate is saved, so
        /// experience-wise search/reports (RMS SRS 3.4) don't need to recompute it per query.
        /// </summary>
        public decimal TotalExperienceYears { get; set; }

        /// <summary>Soft-delete flag — hidden from search/list views but retained for history.</summary>
        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public List<CandidateSkill> Skills { get; set; } = new();

        public List<CandidateExperience> Experience { get; set; } = new();

        public List<CandidateEducation> Education { get; set; } = new();

        public List<CandidateProject> Projects { get; set; } = new();

        public List<CandidateNote> Notes { get; set; } = new();

        public List<CandidateAttachment> Attachments { get; set; } = new();
    }
}
