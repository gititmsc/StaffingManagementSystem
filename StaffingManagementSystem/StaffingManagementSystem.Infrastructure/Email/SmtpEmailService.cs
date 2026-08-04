using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StaffingManagementSystem.Core.Configuration;
using StaffingManagementSystem.Core.Interfaces;

namespace StaffingManagementSystem.Infrastructure.Email
{
    /// <summary>
    /// Sends transactional email over SMTP using the configured <see cref="EmailSettings"/>.
    /// When <see cref="EmailSettings.EnableTestMode"/> is on, every email is redirected to
    /// <see cref="EmailSettings.TestToEmailAddress"/> instead of the real recipient — used to
    /// avoid emailing real users from non-production environments.
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly AppUrlSettings _appUrlSettings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(
            IOptions<EmailSettings> options,
            IOptions<AppUrlSettings> appUrlOptions,
            ILogger<SmtpEmailService> logger)
        {
            _settings = options.Value;
            _appUrlSettings = appUrlOptions.Value;
            _logger = logger;
        }

        public Task SendPasswordResetEmailAsync(
            string toEmail,
            string recipientName,
            string resetLink,
            CancellationToken cancellationToken = default)
        {
            const string title = "Reset your password";

            var body =
                $"<p>Hi {WebUtility.HtmlEncode(recipientName)},</p>" +
                "<p>We received a request to reset your Staffing Management System password. " +
                "This link is valid for 60 minutes and can only be used once.</p>" +
                $"<p><a href=\"{WebUtility.HtmlEncode(resetLink)}\" style=\"display:inline-block;margin-top:6px;padding:10px 20px;background:#163a63;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;\">Reset your password</a></p>" +
                "<p>If you didn't request this, you can safely ignore this email — your password will not be changed.</p>";

            return SendAsync(toEmail, title, BuildEmailHtml(title, body), cancellationToken);
        }

        public Task SendAccountSetupEmailAsync(
            string toEmail,
            string recipientName,
            string setupLink,
            CancellationToken cancellationToken = default)
        {
            const string title = "Welcome — set up your password";

            var body =
                $"<p>Hi {WebUtility.HtmlEncode(recipientName)},</p>" +
                "<p>An administrator has created an account for you on the Staffing Management System. " +
                "Set your password to get started. This link is valid for 60 minutes and can only be used once.</p>" +
                $"<p><a href=\"{WebUtility.HtmlEncode(setupLink)}\" style=\"display:inline-block;margin-top:6px;padding:10px 20px;background:#163a63;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;\">Set your password</a></p>" +
                "<p>If you weren't expecting this, you can safely ignore this email.</p>";

            return SendAsync(toEmail, title, BuildEmailHtml(title, body), cancellationToken);
        }

        public Task SendCandidateRegistrationAdminNotificationAsync(
            string toEmail,
            string adminName,
            string candidateName,
            string candidateEmail,
            string? candidateMobile,
            decimal experienceYears,
            string primarySkills,
            DateTime registeredAtUtc,
            string approvalsDeepLink,
            CancellationToken cancellationToken = default)
        {
            const string title = "New candidate awaiting approval";

            var body =
                $"<p>Hi {WebUtility.HtmlEncode(adminName)},</p>" +
                "<p>A new candidate has submitted the self-registration form and is awaiting your approval:</p>" +
                "<table style=\"width:100%;border-collapse:collapse;margin:12px 0;\">" +
                $"{EmailField("Candidate Name", candidateName)}" +
                $"{EmailField("Email Address", candidateEmail)}" +
                $"{EmailField("Mobile Number", candidateMobile ?? "—")}" +
                $"{EmailField("Experience", $"{experienceYears:0.#} years")}" +
                $"{EmailField("Primary Skills", string.IsNullOrWhiteSpace(primarySkills) ? "—" : primarySkills)}" +
                $"{EmailField("Registration Date", registeredAtUtc.ToString("yyyy-MM-dd HH:mm") + " UTC")}" +
                "</table>" +
                $"<p><a href=\"{WebUtility.HtmlEncode(approvalsDeepLink)}\" style=\"display:inline-block;margin-top:6px;padding:10px 20px;background:#163a63;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;\">Review in Candidate Approvals</a></p>" +
                "<p>Sign in to download the resume and approve or reject this application.</p>";

            return SendAsync(toEmail, title, BuildEmailHtml(title, body), cancellationToken);
        }

        public Task SendCandidateRegistrationConfirmationEmailAsync(
            string toEmail,
            string candidateName,
            CancellationToken cancellationToken = default)
        {
            const string title = "We've received your application";

            var body =
                $"<p>Hi {WebUtility.HtmlEncode(candidateName)},</p>" +
                "<p>Thank you for applying. We've received your profile and resume, and our recruitment " +
                "team will review your application shortly.</p>" +
                "<p>We'll be in touch if there's a suitable opportunity for you.</p>";

            return SendAsync(toEmail, title, BuildEmailHtml(title, body), cancellationToken);
        }

        public Task SendCandidateRejectionEmailAsync(
            string toEmail,
            string candidateName,
            string rejectionComment,
            CancellationToken cancellationToken = default)
        {
            const string title = "Update on your application";

            var body =
                $"<p>Hi {WebUtility.HtmlEncode(candidateName)},</p>" +
                "<p>Thank you for taking the time to apply and for your interest in joining us.</p>" +
                "<p>After careful review, we regret to let you know that your application has not been " +
                "shortlisted at this time.</p>" +
                $"<blockquote style=\"margin:16px 0;padding:10px 16px;border-left:3px solid #163a63;background:#f5f7fa;\">" +
                $"{WebUtility.HtmlEncode(rejectionComment)}</blockquote>" +
                "<p>We encourage you to apply again in the future as new opportunities become available.</p>" +
                "<p>We wish you the very best in your job search.</p>";

            return SendAsync(toEmail, title, BuildEmailHtml(title, body), cancellationToken);
        }

        private static string EmailField(string label, string value) =>
            "<tr>" +
            $"<td style=\"padding:4px 12px 4px 0;color:#5b6b7f;white-space:nowrap;\"><strong>{WebUtility.HtmlEncode(label)}</strong></td>" +
            $"<td style=\"padding:4px 0;\">{WebUtility.HtmlEncode(value)}</td>" +
            "</tr>";

        /// <summary>
        /// Wraps a raw HTML body fragment in the shared header/footer chrome used by every
        /// system email — company logo, name and email title in the header; address, contact,
        /// website and a disclaimer in the footer — so every outgoing email looks consistent.
        /// </summary>
        private string BuildEmailHtml(string title, string bodyHtml)
        {
            var logoUrl = string.IsNullOrWhiteSpace(_appUrlSettings.FrontendBaseUrl)
                ? null
                : $"{_appUrlSettings.FrontendBaseUrl.TrimEnd('/')}/logo.png";

            var logoHtml = logoUrl is null
                ? string.Empty
                : $"<img src=\"{WebUtility.HtmlEncode(logoUrl)}\" alt=\"Staffing Management System\" height=\"32\" style=\"display:block;margin-bottom:8px;\" />";

            return $"""
                <!doctype html>
                <html>
                <body style="margin:0;padding:0;background:#eef1f5;font-family:Segoe UI,Arial,sans-serif;color:#25313f;">
                  <table role="presentation" style="width:100%;background:#eef1f5;padding:24px 0;">
                    <tr><td align="center">
                      <table role="presentation" style="width:100%;max-width:560px;background:#ffffff;border-radius:8px;overflow:hidden;">
                        <tr><td style="background:#163a63;color:#ffffff;padding:20px 28px;">
                          {logoHtml}
                          <div style="font-size:15px;font-weight:600;">Staffing Management System</div>
                          <div style="font-size:13px;opacity:0.85;margin-top:2px;">{WebUtility.HtmlEncode(title)}</div>
                        </td></tr>
                        <tr><td style="padding:24px 28px;font-size:14px;line-height:1.6;">
                          {bodyHtml}
                        </td></tr>
                        <tr><td style="padding:16px 28px;border-top:1px solid #e5e9ef;font-size:12px;color:#8a97a8;">
                          <div>Staffing Management System &middot; ITMusketeers Consultancy Services</div>
                          <div>This is an automated message — please do not reply directly to this email.</div>
                        </td></tr>
                      </table>
                    </td></tr>
                  </table>
                </body>
                </html>
                """;
        }

        private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
        {
            var actualRecipient = _settings.EnableTestMode && !string.IsNullOrWhiteSpace(_settings.TestToEmailAddress)
                ? _settings.TestToEmailAddress!
                : toEmail;

            var effectiveSubject = _settings.EnableTestMode
                ? $"[TEST — intended for {toEmail}] {subject}"
                : subject;

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = effectiveSubject,
                Body = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(actualRecipient);

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
                EnableSsl = _settings.EnableSsl,
            };

            try
            {
                await client.SendMailAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                // Never let a failed email send take down the request that triggered it —
                // callers treat email delivery as best-effort.
                _logger.LogError(ex, "Failed to send email to {Recipient}", actualRecipient);
                throw;
            }
        }
    }
}
