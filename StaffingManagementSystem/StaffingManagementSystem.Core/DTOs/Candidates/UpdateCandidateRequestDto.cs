using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>Payload for PUT /api/candidates/{id}.</summary>
    public class UpdateCandidateRequestDto : IValidatableObject
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>Short professional headline, e.g. "10+ Years Salesforce Developer".</summary>
        [MaxLength(200)]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(30)]
        [RegularExpression(@"^\+?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Enter a valid phone number.")]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(200)]
        public string? CurrentLocation { get; set; }

        public DateTime? DateOfBirth { get; set; }

        /// <summary>One of: Male, Female, Other, PreferNotToSay.</summary>
        [MaxLength(30)]
        public string? Gender { get; set; }

        [MaxLength(300)]
        [Url(ErrorMessage = "Enter a valid URL.")]
        public string? LinkedInUrl { get; set; }

        /// <summary>One of: New, Available, InProcess, Placed, OnHold, Blacklisted.</summary>
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;

        /// <summary>One of: Referral, JobPortal, LinkedIn, WalkIn, Agency, Website, Other.</summary>
        public string? Source { get; set; }

        /// <summary>Required free-text description of the source when <see cref="Source"/> is "Other".</summary>
        [MaxLength(200)]
        public string? OtherSourceText { get; set; }

        /// <summary>Reassigns the owning recruiter when provided; keeps the existing owner when null.</summary>
        public Guid? OwnerRecruiterId { get; set; }

        /// <summary>Admin-only field. Ignored (not persisted) if submitted by a Recruiter.</summary>
        [Range(0, double.MaxValue, ErrorMessage = "Cost to company must be zero or greater.")]
        public decimal? CostToCompany { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost to vendor must be zero or greater.")]
        public decimal? CostToVendor { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Current salary must be zero or greater.")]
        public decimal? CurrentSalary { get; set; }

        public List<CandidateSkillInputDto> Skills { get; set; } = new();

        public List<CandidateExperienceInputDto> Experience { get; set; } = new();

        public List<CandidateEducationInputDto> Education { get; set; } = new();

        public List<CandidateProjectInputDto> Projects { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.Equals(Source, "Other", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(OtherSourceText))
            {
                yield return new ValidationResult(
                    "Please specify the source when 'Other' is selected.",
                    new[] { nameof(OtherSourceText) });
            }
        }
    }
}
