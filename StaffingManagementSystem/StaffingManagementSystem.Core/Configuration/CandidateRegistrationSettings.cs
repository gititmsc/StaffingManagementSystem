namespace StaffingManagementSystem.Core.Configuration
{
    /// <summary>
    /// Strongly typed binding of the "CandidateRegistration" configuration section — the
    /// resume-upload rules for the public, no-login candidate self-registration form. Kept
    /// separate from <see cref="FileStorageSettings"/> (the internal, authenticated
    /// attachment-upload rules) since the two are allowed to diverge intentionally.
    /// </summary>
    public class CandidateRegistrationSettings
    {
        public const string SectionName = "CandidateRegistration";

        /// <summary>Maximum accepted resume upload size, in bytes. Defaults to 50 MB.</summary>
        public long MaxResumeSizeBytes { get; set; } = 50 * 1024 * 1024;

        /// <summary>Allowed resume file extensions (lower-case, including the leading dot).</summary>
        public string[] AllowedResumeExtensions { get; set; } = { ".pdf", ".doc", ".docx" };
    }
}
