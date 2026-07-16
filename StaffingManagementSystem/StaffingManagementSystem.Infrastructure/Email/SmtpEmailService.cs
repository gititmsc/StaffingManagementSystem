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
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> options, ILogger<SmtpEmailService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public Task SendPasswordResetEmailAsync(
            string toEmail,
            string recipientName,
            string resetLink,
            CancellationToken cancellationToken = default)
        {
            const string subject = "Reset your Staffing Management System password";

            var body =
                $"<p>Hi {WebUtility.HtmlEncode(recipientName)},</p>" +
                "<p>We received a request to reset your Staffing Management System password. " +
                "This link is valid for 60 minutes and can only be used once.</p>" +
                $"<p><a href=\"{WebUtility.HtmlEncode(resetLink)}\">Reset your password</a></p>" +
                "<p>If you didn't request this, you can safely ignore this email — your password will not be changed.</p>";

            return SendAsync(toEmail, subject, body, cancellationToken);
        }

        public Task SendAccountSetupEmailAsync(
            string toEmail,
            string recipientName,
            string setupLink,
            CancellationToken cancellationToken = default)
        {
            const string subject = "Welcome to the Staffing Management System — set up your password";

            var body =
                $"<p>Hi {WebUtility.HtmlEncode(recipientName)},</p>" +
                "<p>An administrator has created an account for you on the Staffing Management System. " +
                "Set your password to get started. This link is valid for 60 minutes and can only be used once.</p>" +
                $"<p><a href=\"{WebUtility.HtmlEncode(setupLink)}\">Set your password</a></p>" +
                "<p>If you weren't expecting this, you can safely ignore this email.</p>";

            return SendAsync(toEmail, subject, body, cancellationToken);
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
