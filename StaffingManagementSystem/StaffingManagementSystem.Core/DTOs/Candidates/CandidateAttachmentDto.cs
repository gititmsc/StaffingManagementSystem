namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>Row shape returned by GET /api/candidates/{id}/attachments.</summary>
    public class CandidateAttachmentDto
    {
        public Guid Id { get; set; }

        /// <summary>"Resume" or "Other".</summary>
        public string Type { get; set; } = "Other";

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public string? UploadedByName { get; set; }

        public DateTime UploadedAtUtc { get; set; }
    }
}
