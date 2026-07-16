using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    public class CandidateExperienceInputDto
    {
        [Required(ErrorMessage = "Company name is required.")]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job title is required.")]
        [MaxLength(150)]
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>One of: FullTime, Contract, Internship.</summary>
        public string? EmploymentType { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }
    }
}
