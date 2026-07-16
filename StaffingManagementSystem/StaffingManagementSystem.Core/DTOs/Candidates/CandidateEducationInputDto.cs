using System.ComponentModel.DataAnnotations;

namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    public class CandidateEducationInputDto
    {
        [Required(ErrorMessage = "Degree/qualification is required.")]
        [MaxLength(150)]
        public string Degree { get; set; } = string.Empty;

        [Required(ErrorMessage = "Institution is required.")]
        [MaxLength(200)]
        public string Institution { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? FieldOfStudy { get; set; }

        public int? StartYear { get; set; }

        public int? EndYear { get; set; }

        public bool IsExpected { get; set; }

        [MaxLength(50)]
        public string? Grade { get; set; }
    }
}
