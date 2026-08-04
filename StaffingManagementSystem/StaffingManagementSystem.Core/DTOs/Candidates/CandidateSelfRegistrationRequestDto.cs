using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>
    /// Payload for POST /api/candidate-registration — the public, no-login candidate
    /// self-registration form. Deliberately smaller than <see cref="CreateCandidateRequestDto"/>:
    /// no Status/OwnerRecruiterId/CostToCompany/etc., since those are internal-only concepts
    /// the candidate never sets directly.
    /// </summary>
    public class CandidateSelfRegistrationRequestDto : IValidatableObject
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required.")]
        [MaxLength(30)]
        [RegularExpression(@"^\+?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Enter a valid mobile number.")]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? CurrentLocation { get; set; }

        [MaxLength(300)]
        [Url(ErrorMessage = "Enter a valid URL.")]
        public string? LinkedInUrl { get; set; }

        /// <summary>Simple skill names — no proficiency/years, kept lightweight for a public form.</summary>
        public List<string> Skills { get; set; } = new();

        /// <summary>Same shape as the internal Add/Edit Candidate form's Work Experience section.</summary>
        public List<CandidateExperienceInputDto> Experience { get; set; } = new();

        /// <summary>Same shape as the internal Add/Edit Candidate form's Education section.</summary>
        public List<CandidateEducationInputDto> Education { get; set; } = new();

        /// <summary>Same shape as the internal Add/Edit Candidate form's Projects section.</summary>
        public List<CandidateProjectInputDto> Projects { get; set; } = new();

        [Required(ErrorMessage = "Please complete the CAPTCHA verification.")]
        public string RecaptchaToken { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Skills.Count == 0 || Skills.All(string.IsNullOrWhiteSpace))
            {
                yield return new ValidationResult(
                    "Please list at least one primary skill.",
                    new[] { nameof(Skills) });
            }
        }
    }
}
