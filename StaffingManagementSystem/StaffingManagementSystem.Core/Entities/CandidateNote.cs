namespace StaffingManagementSystem.Core.Entities
{
    /// <summary>A timestamped, attributed free-text note on a candidate profile (RMS SRS 3.3.1).</summary>
    public class CandidateNote
    {
        public Guid Id { get; set; }

        public Guid CandidateId { get; set; }

        public string Note { get; set; } = string.Empty;

        public Guid CreatedByUserId { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
