namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    public class CandidateProjectDto
    {
        public Guid Id { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string? Role { get; set; }

        public string? DurationText { get; set; }

        public string? TechnologiesUsed { get; set; }

        public string? Description { get; set; }
    }
}
