namespace StaffingManagementSystem.Core.Configuration
{
    /// <summary>
    /// Strongly typed binding of the root-level "FrontendBaseUrl" configuration value,
    /// used to build links back to the web app (e.g. password reset links) from the API.
    /// </summary>
    public class AppUrlSettings
    {
        public string FrontendBaseUrl { get; set; } = string.Empty;
    }
}
