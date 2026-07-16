namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    public class CandidateExperienceDto
    {
        public Guid Id { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string? EmploymentType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        public string? Location { get; set; }

        public string? Description { get; set; }
    }
}
