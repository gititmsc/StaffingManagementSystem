namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    public class CandidateNoteDto
    {
        public Guid Id { get; set; }

        public string Note { get; set; } = string.Empty;

        public string? CreatedByName { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
