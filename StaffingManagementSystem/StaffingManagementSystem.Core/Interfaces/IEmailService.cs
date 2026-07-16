namespace StaffingManagementSystem.Core.Interfaces
{
    /// <summary>
    /// Sends transactional email on behalf of the Staffing Management System.
    /// Implemented in the Infrastructure layer.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a "reset your password" email containing a link back to the web app's
        /// reset-password page.
        /// </summary>
        Task SendPasswordResetEmailAsync(
            string toEmail,
            string recipientName,
            string resetLink,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a "welcome, set up your password" email to a user account that was just
        /// created by an administrator. Uses the same set-a-password link as the reset flow.
        /// </summary>
        Task SendAccountSetupEmailAsync(
            string toEmail,
            string recipientName,
            string setupLink,
            CancellationToken cancellationToken = default);
    }
}
