namespace StaffingManagementSystem.Core.Configuration
{
    /// <summary>
    /// Strongly typed binding of the "FileStorage" configuration section, used by
    /// candidate attachment upload/download (RMS SRS 3.3.6 — resume/document attachments).
    /// Files are stored on local disk on the API server per the agreed Phase 1 approach.
    /// </summary>
    public class FileStorageSettings
    {
        public const string SectionName = "FileStorage";

        /// <summary>
        /// Root folder for stored candidate attachments. If relative, it is resolved against
        /// the API application's base directory.
        /// </summary>
        public string RootPath { get; set; } = "App_Data/CandidateAttachments";

        /// <summary>Maximum accepted upload size, in bytes. Defaults to 10 MB.</summary>
        public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

        /// <summary>Allowed file extensions (lower-case, including the leading dot).</summary>
        public string[] AllowedExtensions { get; set; } =
        {
            ".pdf", ".doc", ".docx", ".rtf", ".txt", ".jpg", ".jpeg", ".png",
        };
    }
}
