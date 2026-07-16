namespace StaffingManagementSystem.Core.Configuration
{
    /// <summary>
    /// Strongly typed binding of the "Email" configuration section, used to send
    /// transactional email (password reset links, notifications, etc.) via SMTP.
    /// </summary>
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string SmtpHost { get; set; } = string.Empty;

        public int SmtpPort { get; set; } = 587;

        public string SmtpUsername { get; set; } = string.Empty;

        public string SmtpPassword { get; set; } = string.Empty;

        public string FromAddress { get; set; } = string.Empty;

        public string FromName { get; set; } = string.Empty;

        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// When true, every outgoing email is redirected to <see cref="TestToEmailAddress"/>
        /// instead of the real recipient, with the intended recipient noted in the subject.
        /// Used in non-production environments to avoid emailing real candidates/users.
        /// </summary>
        public bool EnableTestMode { get; set; }

        public string? TestToEmailAddress { get; set; }
    }
}
