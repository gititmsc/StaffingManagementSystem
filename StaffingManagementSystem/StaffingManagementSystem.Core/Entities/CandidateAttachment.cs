namespace StaffingManagementSystem.Core.Entities
{
    /// <summary>
    /// A file (resume, ID proof, certificate, etc.) attached to a candidate profile
    /// (RMS SRS 3.3.6). The physical file lives on local disk on the API server; this
    /// row tracks its metadata and the relative path used to locate it.
    /// </summary>
    public class CandidateAttachment
    {
        public Guid Id { get; set; }

        public Guid CandidateId { get; set; }

        /// <summary>Original file name as uploaded by the user, shown in the UI.</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>Path (relative to the configured storage root) where the file is stored on disk.</summary>
        public string StoredPath { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public Guid UploadedByUserId { get; set; }

        public DateTime UploadedAtUtc { get; set; }
    }
}
