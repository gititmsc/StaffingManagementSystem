namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    public class CandidateSkillDto
    {
        public Guid Id { get; set; }

        public string SkillName { get; set; } = string.Empty;

        public string? Proficiency { get; set; }

        public decimal? YearsOfExperience { get; set; }
    }
}
