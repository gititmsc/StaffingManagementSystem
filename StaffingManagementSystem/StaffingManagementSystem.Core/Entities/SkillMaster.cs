namespace StaffingManagementSystem.Core.Entities
{
    /// <summary>
    /// Shared skill lookup list (RMS SRS 3.3.2) — keeps skill naming consistent across
    /// candidates so skill-wise search actually groups matching skills together.
    /// </summary>
    public class SkillMaster
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Category { get; set; }
    }
}
