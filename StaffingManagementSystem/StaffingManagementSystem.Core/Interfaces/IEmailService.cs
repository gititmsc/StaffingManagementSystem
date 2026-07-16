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
    }
}
