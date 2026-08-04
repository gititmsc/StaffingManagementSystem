using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;

namespace StaffingManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Business logic for the public, no-login Candidate Self-Registration form. Kept separate
    /// from <see cref="ICandidateService"/> — different validation (CAPTCHA, duplicate check),
    /// different caller (anonymous), different DTO shape.
    /// </summary>
    public interface ICandidateRegistrationService
    {
        /// <summary>
        /// Verifies the CAPTCHA token, validates the resume, checks for an active duplicate by
        /// email/phone, creates the candidate with Status = PendingApproval and no owning
        /// recruiter, uploads the resume, then best-effort emails every active Admin plus a
        /// confirmation email to the candidate.
        /// </summary>
        Task<ApiResponse<object>> RegisterAsync(
            CandidateSelfRegistrationRequestDto request,
            string resumeFileName,
            string resumeContentType,
            long resumeFileSizeBytes,
            Stream resumeContent,
            CancellationToken cancellationToken = default);
    }
}
