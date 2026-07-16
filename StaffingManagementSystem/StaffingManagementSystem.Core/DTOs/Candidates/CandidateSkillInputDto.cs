using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    public class CandidateSkillInputDto
    {
        [Required(ErrorMessage = "Skill name is required.")]
        [MaxLength(150)]
        public string SkillName { get; set; } = string.Empty;

        /// <summary>One of: Beginner, Intermediate, Expert.</summary>
        public string? Proficiency { get; set; }

        [Range(0, 60, ErrorMessage = "Years of experience must be between 0 and 60.")]
        public decimal? YearsOfExperience { get; set; }
    }
}
