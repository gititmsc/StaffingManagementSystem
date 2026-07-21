namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>Full candidate profile returned by GET /api/candidates/{id}.</summary>
    public class CandidateDetailDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? Title { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? CurrentLocation { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? LinkedInUrl { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Source { get; set; }

        public string? OtherSourceText { get; set; }

        public Guid OwnerRecruiterId { get; set; }

        public string? OwnerRecruiterName { get; set; }

        public decimal TotalExperienceYears { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public List<CandidateSkillDto> Skills { get; set; } = new();

        public List<CandidateExperienceDto> Experience { get; set; } = new();

        public List<CandidateEducationDto> Education { get; set; } = new();

        public List<CandidateProjectDto> Projects { get; set; } = new();

        public List<CandidateNoteDto> Notes { get; set; } = new();
    }
}
