using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    public class CandidateProjectInputDto
    {
        [Required(ErrorMessage = "Project name is required.")]
        [MaxLength(200)]
        public string ProjectName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Role { get; set; }

        [MaxLength(100)]
        public string? DurationText { get; set; }

        [MaxLength(500)]
        public string? TechnologiesUsed { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }
    }
}
