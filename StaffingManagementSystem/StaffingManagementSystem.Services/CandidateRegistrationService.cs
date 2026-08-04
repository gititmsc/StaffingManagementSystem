using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.Configuration;
using StaffingManagementSystem.Core.DTOs.Candidates;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Core.Enums;
using StaffingManagementSystem.Core.Interfaces;
using StaffingManagementSystem.Repositories.Interfaces;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Services
{
    /// <inheritdoc cref="ICandidateRegistrationService"/>
    public class CandidateRegistrationService : ICandidateRegistrationService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly ICandidateAttachmentService _attachmentService;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IRecaptchaVerifier _recaptchaVerifier;
        private readonly CandidateRegistrationSettings _registrationSettings;
        private readonly AppUrlSettings _appUrlSettings;
        private readonly ILogger<CandidateRegistrationService> _logger;

        public CandidateRegistrationService(
            ICandidateRepository candidateRepository,
            ICandidateAttachmentService attachmentService,
            IUserRepository userRepository,
            IEmailService emailService,
            IRecaptchaVerifier recaptchaVerifier,
            IOptions<CandidateRegistrationSettings> registrationOptions,
            IOptions<AppUrlSettings> appUrlOptions,
            ILogger<CandidateRegistrationService> logger)
        {
            _candidateRepository = candidateRepository;
            _attachmentService = attachmentService;
            _userRepository = userRepository;
            _emailService = emailService;
            _recaptchaVerifier = recaptchaVerifier;
            _registrationSettings = registrationOptions.Value;
            _appUrlSettings = appUrlOptions.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<object>> RegisterAsync(
            CandidateSelfRegistrationRequestDto request,
            string resumeFileName,
            string resumeContentType,
            long resumeFileSizeBytes,
            Stream resumeContent,
            CancellationToken cancellationToken = default)
        {
            if (!await _recaptchaVerifier.VerifyAsync(request.RecaptchaToken, cancellationToken))
            {
                return ApiResponse<object>.FailureResponse(
                    "CAPTCHA verification failed. Please try again.",
                    ["CAPTCHA verification failed. Please try again."]);
            }

            if (resumeFileSizeBytes <= 0)
            {
                return ApiResponse<object>.FailureResponse(
                    "Please attach your resume.", ["Please attach your resume."]);
            }

            if (resumeFileSizeBytes > _registrationSettings.MaxResumeSizeBytes)
            {
                var maxMb = _registrationSettings.MaxResumeSizeBytes / (1024 * 1024);
                return ApiResponse<object>.FailureResponse(
                    $"Resume is too large. Maximum allowed size is {maxMb} MB.",
                    [$"Resume is too large. Maximum allowed size is {maxMb} MB."]);
            }

            var extension = Path.GetExtension(resumeFileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_registrationSettings.AllowedResumeExtensions.Contains(extension))
            {
                var allowed = string.Join(", ", _registrationSettings.AllowedResumeExtensions);
                return ApiResponse<object>.FailureResponse(
                    $"Resume file type not allowed. Allowed types: {allowed}",
                    [$"Resume file type not allowed. Allowed types: {allowed}"]);
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var normalizedPhone = request.Phone.Trim();

            if (await _candidateRepository.HasActiveDuplicateAsync(normalizedEmail, normalizedPhone))
            {
                return ApiResponse<object>.FailureResponse(
                    "It looks like you've already applied — we'll be in touch.",
                    ["A candidate with this email or mobile number has already registered."]);
            }

            var candidateId = Guid.NewGuid();
            var skills = new List<CandidateSkill>();
            var seenSkillIds = new HashSet<Guid>();

            foreach (var skillName in request.Skills)
            {
                if (string.IsNullOrWhiteSpace(skillName))
                {
                    continue;
                }

                var skillId = await _candidateRepository.GetOrCreateSkillIdAsync(skillName);
                if (!seenSkillIds.Add(skillId))
                {
                    continue;
                }

                skills.Add(new CandidateSkill
                {
                    Id = Guid.NewGuid(),
                    CandidateId = candidateId,
                    SkillId = skillId,
                });
            }

            var experience = CandidateGraphBuilder.BuildExperience(candidateId, request.Experience);
            var education = CandidateGraphBuilder.BuildEducation(candidateId, request.Education);
            var projects = CandidateGraphBuilder.BuildProjects(candidateId, request.Projects);

            var candidate = new Candidate
            {
                Id = candidateId,
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                Phone = normalizedPhone,
                CurrentLocation = Norm(request.CurrentLocation),
                LinkedInUrl = Norm(request.LinkedInUrl),
                Status = CandidateStatus.PendingApproval,
                OwnerRecruiterId = null,
                TotalExperienceYears = CandidateGraphBuilder.CalculateTotalExperienceYears(experience),
                CreatedAtUtc = DateTime.UtcNow,
                Skills = skills,
                Experience = experience,
                Education = education,
                Projects = projects,
            };

            await _candidateRepository.CreateAsync(candidate);

            await _attachmentService.UploadResumeAsync(
                candidateId, resumeFileName, resumeContentType, resumeFileSizeBytes, resumeContent, uploadedByUserId: null);

            await NotifyAdminsAndCandidateAsync(candidate, request.Skills, cancellationToken);

            return ApiResponse<object>.SuccessResponse(
                new { },
                "Thank you — your application has been received. Our team will review it and get back to you.");
        }

        private async Task NotifyAdminsAndCandidateAsync(Candidate candidate, List<string> skills, CancellationToken cancellationToken)
        {
            var primarySkills = string.Join(", ", skills.Where(s => !string.IsNullOrWhiteSpace(s)));
            var approvalsDeepLink = string.IsNullOrWhiteSpace(_appUrlSettings.FrontendBaseUrl)
                ? string.Empty
                : $"{_appUrlSettings.FrontendBaseUrl.TrimEnd('/')}/candidate-approvals";

            var admins = (await _userRepository.GetAllAsync())
                .Where(u => u.Role == UserRole.Admin && u.IsActive)
                .ToList();

            foreach (var admin in admins)
            {
                try
                {
                    await _emailService.SendCandidateRegistrationAdminNotificationAsync(
                        admin.Email,
                        admin.FullName,
                        candidate.FullName,
                        candidate.Email,
                        candidate.Phone,
                        candidate.TotalExperienceYears,
                        primarySkills,
                        candidate.CreatedAtUtc,
                        approvalsDeepLink,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    // Best-effort: one bad admin address must not block the rest or the registration itself.
                    _logger.LogError(ex, "Failed to send registration notification email to admin {AdminEmail}", admin.Email);
                }
            }

            try
            {
                await _emailService.SendCandidateRegistrationConfirmationEmailAsync(
                    candidate.Email, candidate.FullName, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send registration confirmation email to candidate {CandidateEmail}", candidate.Email);
            }
        }

        private static string? Norm(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
