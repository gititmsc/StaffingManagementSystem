using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>Payload for POST /api/candidates.</summary>
    public class CreateCandidateRequestDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(200)]
        public string? CurrentLocation { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(30)]
        public string? Gender { get; set; }

        /// <summary>One of: New, Available, InProcess, Placed, OnHold, Blacklisted.</summary>
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;

        /// <summary>One of: Referral, JobPortal, LinkedIn, WalkIn, Agency, Website, Other.</summary>
        public string? Source { get; set; }

        public List<CandidateSkillInputDto> Skills { get; set; } = new();

        public List<CandidateExperienceInputDto> Experience { get; set; } = new();

        public List<CandidateEducationInputDto> Education { get; set; } = new();

        public List<CandidateProjectInputDto> Projects { get; set; } = new();
    }
}
