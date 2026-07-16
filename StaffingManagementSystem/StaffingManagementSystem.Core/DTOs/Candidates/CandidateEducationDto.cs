namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    public class CandidateEducationDto
    {
        public Guid Id { get; set; }

        public string Degree { get; set; } = string.Empty;

        public string Institution { get; set; } = string.Empty;

        public string? FieldOfStudy { get; set; }

        public int? StartYear { get; set; }

        public int? EndYear { get; set; }

        public bool IsExpected { get; set; }

        public string? Grade { get; set; }
    }
}
