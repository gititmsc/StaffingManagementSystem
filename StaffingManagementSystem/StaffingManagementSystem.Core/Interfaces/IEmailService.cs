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

        /// <summary>
        /// Notifies an Admin that a new candidate submitted the public self-registration form
        /// and is awaiting approval. Links back to the Candidate Approvals screen rather than
        /// attaching/linking the resume directly — the Admin downloads it from there once
        /// signed in.
        /// </summary>
        Task SendCandidateRegistrationAdminNotificationAsync(
            string toEmail,
            string adminName,
            string candidateName,
            string candidateEmail,
            string? candidateMobile,
            decimal experienceYears,
            string primarySkills,
            DateTime registeredAtUtc,
            string approvalsDeepLink,
            CancellationToken cancellationToken = default);

        /// <summary>Sent to the candidate immediately after they submit the registration form.</summary>
        Task SendCandidateRegistrationConfirmationEmailAsync(
            string toEmail,
            string candidateName,
            CancellationToken cancellationToken = default);

        /// <summary>Sent to the candidate when an Admin rejects their pending registration.</summary>
        Task SendCandidateRejectionEmailAsync(
            string toEmail,
            string candidateName,
            string rejectionComment,
            CancellationToken cancellationToken = default);

        /// <summary>Sent to the candidate when an Admin approves their pending registration.</summary>
        Task SendCandidateApprovalEmailAsync(
            string toEmail,
            string candidateName,
            CancellationToken cancellationToken = default);
    }
}
