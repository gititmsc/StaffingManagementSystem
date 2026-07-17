namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>A single grouped-count row (skill, experience band or company) for report summaries.</summary>
    public class NameCountDto
    {
        public string Name { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}
