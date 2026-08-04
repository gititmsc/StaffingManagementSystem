namespace StaffingManagementSystem.Core.Configuration
{
    /// <summary>
    /// Strongly typed binding of the "Recaptcha" configuration section, used to verify
    /// Google reCAPTCHA v2 tokens submitted by the public candidate registration form.
    /// </summary>
    public class RecaptchaSettings
    {
        public const string SectionName = "Recaptcha";

        /// <summary>Public site key, embedded in the registration page's reCAPTCHA widget.</summary>
        public string SiteKey { get; set; } = string.Empty;

        /// <summary>
        /// Secret key used to verify tokens server-side against Google's siteverify endpoint.
        /// While this is left as the placeholder value, verification is skipped (with a
        /// logged warning) so local/dev testing isn't blocked before real keys are issued.
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        public const string PlaceholderValue = "REPLACE_WITH_RECAPTCHA_SECRET_KEY";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(SecretKey) && SecretKey != PlaceholderValue;
    }
}
