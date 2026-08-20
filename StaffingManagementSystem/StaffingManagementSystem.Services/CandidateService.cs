using Microsoft.Extensions.Logging;
using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Core.Enums;
using StaffingManagementSystem.Core.Interfaces;
using StaffingManagementSystem.Repositories.Interfaces;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Services
{
    /// <inheritdoc cref="ICandidateService"/>
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<CandidateService> _logger;

        public CandidateService(
            ICandidateRepository candidateRepository,
            IUserRepository userRepository,
            IEmailService emailService,
            ILogger<CandidateService> logger)
        {
            _candidateRepository = candidateRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<CandidateListResultDto>> GetAllCandidatesAsync(CandidateListRequestDto request, string actorRole)
        {
            var candidates = await _candidateRepository.GetAllAsync();
            var userNames = await GetUserNameLookupAsync();

            // Repository already orders by CreatedAtUtc descending; Select/Where preserve that order.
            IEnumerable<CandidateListItemDto> filtered = candidates.Select(c => MapToListItemDto(c, userNames));

            // Self-registered candidates stay out of the Candidate Master list until an Admin
            // approves them (PendingApproval) or permanently if rejected (Rejected) — Rejected
            // candidates are never shown here, not even to Admin, only via the dedicated
            // Candidate Approvals screen. The one exception: Admin explicitly filtering by
            // exactly that status (i.e. the Approvals screen's own request) is allowed through.
            var requestedStatus = Norm(request.Status);
            var isExplicitApprovalStatusRequest = IsAdmin(actorRole) &&
                (string.Equals(requestedStatus, nameof(CandidateStatus.PendingApproval), StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(requestedStatus, nameof(CandidateStatus.Rejected), StringComparison.OrdinalIgnoreCase));

            if (!isExplicitApprovalStatusRequest)
            {
                filtered = filtered.Where(d =>
                    !string.Equals(d.Status, nameof(CandidateStatus.PendingApproval), StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(d.Status, nameof(CandidateStatus.Rejected), StringComparison.OrdinalIgnoreCase));
            }

            var term = Norm(request.Search)?.ToLowerInvariant();
            if (term is not null)
            {
                filtered = filtered.Where(d =>
                    d.FullName.ToLowerInvariant().Contains(term) ||
                    d.Email.ToLowerInvariant().Contains(term) ||
                    (d.CurrentCompany ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    d.Skills.Any(s => s.ToLowerInvariant().Contains(term)));
            }

            var status = requestedStatus;
            if (status is not null)
            {
                filtered = filtered.Where(d => string.Equals(d.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            if (request.OwnerRecruiterId.HasValue)
            {
                filtered = filtered.Where(d => d.OwnerRecruiterId == request.OwnerRecruiterId.Value);
            }

            filtered = ApplySort(filtered, request.SortBy, request.SortDescending);

            var materialized = filtered.ToList();
            var totalCount = materialized.Count;

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 25 : Math.Min(request.PageSize, 200);

            var pageItems = materialized
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            pageItems.ForEach(dto => ApplyFieldVisibility(dto, actorRole));

            var totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

            var result = new CandidateListResultDto
            {
                Items = pageItems,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
            };

            return ApiResponse<CandidateListResultDto>.SuccessResponse(result);
        }

        public async Task<ApiResponse<CandidateDetailDto>> GetCandidateByIdAsync(Guid id, string actorRole)
        {
            var candidate = await _candidateRepository.GetByIdAsync(id);
            if (candidate is null)
            {
                return ApiResponse<CandidateDetailDto>.FailureResponse("Candidate not found.", ["Candidate not found."]);
            }

            // Recruiter/Viewer can't view a PendingApproval/Rejected candidate even via a direct
            // URL — same rule as the list endpoint, enforced again here.
            if (!IsAdmin(actorRole) &&
                (candidate.Status == CandidateStatus.PendingApproval || candidate.Status == CandidateStatus.Rejected))
            {
                return ApiResponse<CandidateDetailDto>.FailureResponse("Candidate not found.", ["Candidate not found."]);
            }

            var userNames = await GetUserNameLookupAsync();
            var dto = MapToDetailDto(candidate, userNames);
            ApplyFieldVisibility(dto, actorRole);

            return ApiResponse<CandidateDetailDto>.SuccessResponse(dto);
        }

        public async Task<ApiResponse<CandidateDetailDto>> CreateCandidateAsync(CreateCandidateRequestDto request, Guid ownerRecruiterId, string actorRole)
        {
            if (!TryParseRequiredEnum<CandidateStatus>(request.Status, out var status))
            {
                return ApiResponse<CandidateDetailDto>.FailureResponse("Invalid status.", ["Invalid status."]);
            }

            var source = ParseOptionalEnum<CandidateSource>(request.Source);
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var isDuplicateEmail = await _candidateRepository.EmailExistsAsync(normalizedEmail);

            var candidateId = Guid.NewGuid();

            var skills = await BuildSkillsAsync(candidateId, request.Skills);
            var experience = CandidateGraphBuilder.BuildExperience(candidateId, request.Experience);
            var education = CandidateGraphBuilder.BuildEducation(candidateId, request.Education);
            var projects = CandidateGraphBuilder.BuildProjects(candidateId, request.Projects);

            var candidate = new Candidate
            {
                Id = candidateId,
                FullName = request.FullName.Trim(),
                Title = Norm(request.Title),
                Email = normalizedEmail,
                Phone = Norm(request.Phone),
                Address = Norm(request.Address),
                CurrentLocation = Norm(request.CurrentLocation),
                DateOfBirth = request.DateOfBirth,
                Gender = Norm(request.Gender),
                LinkedInUrl = Norm(request.LinkedInUrl),
                Status = status,
                Source = source,
                OtherSourceText = source == CandidateSource.Other ? Norm(request.OtherSourceText) : null,
                OwnerRecruiterId = ownerRecruiterId,
                CostToCompany = IsAdmin(actorRole) ? request.CostToCompany : null,
                CostToVendor = request.CostToVendor,
                CurrentSalary = request.CurrentSalary,
                TotalExperienceYears = CandidateGraphBuilder.CalculateTotalExperienceYears(experience),
                CreatedAtUtc = DateTime.UtcNow,
                Skills = skills,
                Experience = experience,
                Education = education,
                Projects = projects,
            };

            await _candidateRepository.CreateAsync(candidate);

            var initialNote = Norm(request.InitialNote);
            if (initialNote is not null)
            {
                await _candidateRepository.AddNoteAsync(new CandidateNote
                {
                    Id = Guid.NewGuid(),
                    CandidateId = candidateId,
                    Note = initialNote,
                    CreatedByUserId = ownerRecruiterId,
                    CreatedAtUtc = DateTime.UtcNow,
                });
            }

            var refreshed = await _candidateRepository.GetByIdAsync(candidateId);
            var userNames = await GetUserNameLookupAsync();
            var dto = MapToDetailDto(refreshed ?? candidate, userNames);
            ApplyFieldVisibility(dto, actorRole);

            var message = isDuplicateEmail
                ? "Candidate created. Note: another candidate already uses this email address."
                : "Candidate created.";

            return ApiResponse<CandidateDetailDto>.SuccessResponse(dto, message);
        }

        public async Task<ApiResponse<CandidateDetailDto>> UpdateCandidateAsync(Guid id, UpdateCandidateRequestDto request, string actorRole)
        {
            var existing = await _candidateRepository.GetByIdAsync(id);
            if (existing is null)
            {
                return ApiResponse<CandidateDetailDto>.FailureResponse("Candidate not found.", ["Candidate not found."]);
            }

            if (!TryParseRequiredEnum<CandidateStatus>(request.Status, out var status))
            {
                return ApiResponse<CandidateDetailDto>.FailureResponse("Invalid status.", ["Invalid status."]);
            }

            var source = ParseOptionalEnum<CandidateSource>(request.Source);
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var isDuplicateEmail = await _candidateRepository.EmailExistsAsync(normalizedEmail, id);

            var skills = await BuildSkillsAsync(id, request.Skills);
            var experience = CandidateGraphBuilder.BuildExperience(id, request.Experience);
            var education = CandidateGraphBuilder.BuildEducation(id, request.Education);
            var projects = CandidateGraphBuilder.BuildProjects(id, request.Projects);

            var updated = new Candidate
            {
                Id = id,
                FullName = request.FullName.Trim(),
                Title = Norm(request.Title),
                Email = normalizedEmail,
                Phone = Norm(request.Phone),
                Address = Norm(request.Address),
                CurrentLocation = Norm(request.CurrentLocation),
                DateOfBirth = request.DateOfBirth,
                Gender = Norm(request.Gender),
                LinkedInUrl = Norm(request.LinkedInUrl),
                Status = status,
                Source = source,
                OtherSourceText = source == CandidateSource.Other ? Norm(request.OtherSourceText) : null,
                OwnerRecruiterId = request.OwnerRecruiterId ?? existing.OwnerRecruiterId,
                // A non-Admin editor's request never carries a real CostToCompany value (the field is
                // hidden from them), so keep whatever an Admin previously set rather than wiping it out.
                CostToCompany = IsAdmin(actorRole) ? request.CostToCompany : existing.CostToCompany,
                CostToVendor = request.CostToVendor,
                CurrentSalary = request.CurrentSalary,
                TotalExperienceYears = CandidateGraphBuilder.CalculateTotalExperienceYears(experience),
            };

            await _candidateRepository.UpdateAsync(updated, skills, experience, education, projects);

            var refreshed = await _candidateRepository.GetByIdAsync(id);
            var userNames = await GetUserNameLookupAsync();
            var dto = MapToDetailDto(refreshed!, userNames);
            ApplyFieldVisibility(dto, actorRole);

            var message = isDuplicateEmail
                ? "Candidate updated. Note: another candidate already uses this email address."
                : "Candidate updated.";

            return ApiResponse<CandidateDetailDto>.SuccessResponse(dto, message);
        }

        public async Task<ApiResponse<object>> DeleteCandidateAsync(Guid id)
        {
            var existing = await _candidateRepository.GetByIdAsync(id);
            if (existing is null)
            {
                return ApiResponse<object>.FailureResponse("Candidate not found.", ["Candidate not found."]);
            }

            await _candidateRepository.SoftDeleteAsync(id);

            return ApiResponse<object>.SuccessResponse(new { }, "Candidate deleted.");
        }

        public async Task<ApiResponse<CandidateNoteDto>> AddNoteAsync(Guid candidateId, AddCandidateNoteRequestDto request, Guid createdByUserId)
        {
            var existing = await _candidateRepository.GetByIdAsync(candidateId);
            if (existing is null)
            {
                return ApiResponse<CandidateNoteDto>.FailureResponse("Candidate not found.", ["Candidate not found."]);
            }

            var note = new CandidateNote
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                Note = request.Note.Trim(),
                CreatedByUserId = createdByUserId,
                CreatedAtUtc = DateTime.UtcNow,
            };

            await _candidateRepository.AddNoteAsync(note);

            var author = await _userRepository.GetByIdAsync(createdByUserId);

            var dto = new CandidateNoteDto
            {
                Id = note.Id,
                Note = note.Note,
                CreatedByName = author?.FullName,
                CreatedAtUtc = note.CreatedAtUtc,
            };

            return ApiResponse<CandidateNoteDto>.SuccessResponse(dto, "Note added.");
        }

        public async Task<ApiResponse<CandidateDetailDto>> ApproveAsync(Guid candidateId, Guid approvingAdminUserId)
        {
            var existing = await _candidateRepository.GetByIdAsync(candidateId);
            if (existing is null)
            {
                return ApiResponse<CandidateDetailDto>.FailureResponse("Candidate not found.", ["Candidate not found."]);
            }

            if (existing.Status != CandidateStatus.PendingApproval)
            {
                return ApiResponse<CandidateDetailDto>.FailureResponse(
                    "Only a candidate awaiting approval can be approved.",
                    ["Only a candidate awaiting approval can be approved."]);
            }

            await _candidateRepository.UpdateApprovalStatusAsync(
                candidateId,
                CandidateStatus.Approved,
                rejectionComment: null,
                approvedByUserId: approvingAdminUserId,
                approvedAtUtc: DateTime.UtcNow,
                rejectedByUserId: null,
                rejectedAtUtc: null);

            try
            {
                await _emailService.SendCandidateApprovalEmailAsync(existing.Email, existing.FullName);
            }
            catch (Exception ex)
            {
                // Best-effort: a failed email must not undo or block the approval itself.
                _logger.LogError(ex, "Failed to send approval email to candidate {CandidateEmail}", existing.Email);
            }

            var refreshed = await _candidateRepository.GetByIdAsync(candidateId);
            var userNames = await GetUserNameLookupAsync();
            var dto = MapToDetailDto(refreshed!, userNames);

            return ApiResponse<CandidateDetailDto>.SuccessResponse(dto, "Candidate approved.");
        }

        public async Task<ApiResponse<CandidateDetailDto>> RejectAsync(Guid candidateId, Guid rejectingAdminUserId, string comment)
        {
            var trimmedComment = comment?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedComment))
            {
                return ApiResponse<CandidateDetailDto>.FailureResponse(
                    "A rejection comment is required.", ["A rejection comment is required."]);
            }

            var existing = await _candidateRepository.GetByIdAsync(candidateId);
            if (existing is null)
            {
                return ApiResponse<CandidateDetailDto>.FailureResponse("Candidate not found.", ["Candidate not found."]);
            }

            if (existing.Status != CandidateStatus.PendingApproval)
            {
                return ApiResponse<CandidateDetailDto>.FailureResponse(
                    "Only a candidate awaiting approval can be rejected.",
                    ["Only a candidate awaiting approval can be rejected."]);
            }

            await _candidateRepository.UpdateApprovalStatusAsync(
                candidateId,
                CandidateStatus.Rejected,
                rejectionComment: trimmedComment,
                approvedByUserId: null,
                approvedAtUtc: null,
                rejectedByUserId: rejectingAdminUserId,
                rejectedAtUtc: DateTime.UtcNow);

            try
            {
                await _emailService.SendCandidateRejectionEmailAsync(existing.Email, existing.FullName, trimmedComment);
            }
            catch (Exception ex)
            {
                // Best-effort: a failed email must not undo or block the rejection itself.
                _logger.LogError(ex, "Failed to send rejection email to candidate {CandidateEmail}", existing.Email);
            }

            var refreshed = await _candidateRepository.GetByIdAsync(candidateId);
            var userNames = await GetUserNameLookupAsync();
            var dto = MapToDetailDto(refreshed!, userNames);

            return ApiResponse<CandidateDetailDto>.SuccessResponse(dto, "Candidate rejected.");
        }

        // ---------- helpers ----------

        private async Task<Dictionary<Guid, string>> GetUserNameLookupAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.ToDictionary(u => u.Id, u => u.FullName);
        }

        private async Task<List<CandidateSkill>> BuildSkillsAsync(Guid candidateId, List<CandidateSkillInputDto> inputs)
        {
            var result = new List<CandidateSkill>();
            var seenSkillIds = new HashSet<Guid>();

            foreach (var input in inputs)
            {
                if (string.IsNullOrWhiteSpace(input.SkillName))
                {
                    continue;
                }

                var skillId = await _candidateRepository.GetOrCreateSkillIdAsync(input.SkillName);
                if (!seenSkillIds.Add(skillId))
                {
                    // Same skill submitted twice in one form — keep the first occurrence only.
                    continue;
                }

                result.Add(new CandidateSkill
                {
                    Id = Guid.NewGuid(),
                    CandidateId = candidateId,
                    SkillId = skillId,
                    Proficiency = ParseOptionalEnum<ProficiencyLevel>(input.Proficiency),
                    YearsOfExperience = input.YearsOfExperience,
                });
            }

            return result;
        }

        private static CandidateListItemDto MapToListItemDto(Candidate candidate, Dictionary<Guid, string> userNames)
        {
            var ownerName = candidate.OwnerRecruiterId.HasValue
                ? userNames.GetValueOrDefault(candidate.OwnerRecruiterId.Value)
                : null;
            var currentExperience = candidate.Experience.FirstOrDefault(e => e.IsCurrent);

            return new CandidateListItemDto
            {
                Id = candidate.Id,
                FullName = candidate.FullName,
                Title = candidate.Title,
                Email = candidate.Email,
                Phone = candidate.Phone,
                CurrentLocation = candidate.CurrentLocation,
                LinkedInUrl = candidate.LinkedInUrl,
                Status = candidate.Status.ToString(),
                TotalExperienceYears = candidate.TotalExperienceYears,
                CurrentCompany = currentExperience?.CompanyName,
                Skills = candidate.Skills
                    .Select(s => s.Skill?.Name ?? string.Empty)
                    .Where(name => name.Length > 0)
                    .ToList(),
                OwnerRecruiterId = candidate.OwnerRecruiterId,
                OwnerRecruiterName = ownerName,
                CreatedAtUtc = candidate.CreatedAtUtc,
            };
        }

        private static CandidateDetailDto MapToDetailDto(Candidate candidate, Dictionary<Guid, string> userNames)
        {
            var ownerName = candidate.OwnerRecruiterId.HasValue
                ? userNames.GetValueOrDefault(candidate.OwnerRecruiterId.Value)
                : null;

            return new CandidateDetailDto
            {
                Id = candidate.Id,
                FullName = candidate.FullName,
                Title = candidate.Title,
                Email = candidate.Email,
                Phone = candidate.Phone,
                Address = candidate.Address,
                CurrentLocation = candidate.CurrentLocation,
                DateOfBirth = candidate.DateOfBirth,
                Gender = candidate.Gender,
                LinkedInUrl = candidate.LinkedInUrl,
                Status = candidate.Status.ToString(),
                Source = candidate.Source?.ToString(),
                OtherSourceText = candidate.OtherSourceText,
                OwnerRecruiterId = candidate.OwnerRecruiterId,
                OwnerRecruiterName = ownerName,
                CostToCompany = candidate.CostToCompany,
                CostToVendor = candidate.CostToVendor,
                CurrentSalary = candidate.CurrentSalary,
                RejectionComment = candidate.RejectionComment,
                ApprovedAtUtc = candidate.ApprovedAtUtc,
                RejectedAtUtc = candidate.RejectedAtUtc,
                TotalExperienceYears = candidate.TotalExperienceYears,
                CreatedAtUtc = candidate.CreatedAtUtc,
                UpdatedAtUtc = candidate.UpdatedAtUtc,
                Skills = candidate.Skills.Select(s => new CandidateSkillDto
                {
                    Id = s.Id,
                    SkillName = s.Skill?.Name ?? string.Empty,
                    Proficiency = s.Proficiency?.ToString(),
                    YearsOfExperience = s.YearsOfExperience,
                }).ToList(),
                Experience = candidate.Experience.Select(e => new CandidateExperienceDto
                {
                    Id = e.Id,
                    CompanyName = e.CompanyName,
                    JobTitle = e.JobTitle,
                    EmploymentType = e.EmploymentType?.ToString(),
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent,
                    Location = e.Location,
                    Description = e.Description,
                }).ToList(),
                Education = candidate.Education.Select(e => new CandidateEducationDto
                {
                    Id = e.Id,
                    Degree = e.Degree,
                    Institution = e.Institution,
                    FieldOfStudy = e.FieldOfStudy,
                    StartYear = e.StartYear,
                    EndYear = e.EndYear,
                    IsExpected = e.IsExpected,
                    Grade = e.Grade,
                }).ToList(),
                Projects = candidate.Projects.Select(p => new CandidateProjectDto
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName,
                    Role = p.Role,
                    DurationText = p.DurationText,
                    TechnologiesUsed = p.TechnologiesUsed,
                    Description = p.Description,
                }).ToList(),
                Notes = candidate.Notes
                    .OrderByDescending(n => n.CreatedAtUtc)
                    .Select(n => new CandidateNoteDto
                    {
                        Id = n.Id,
                        Note = n.Note,
                        CreatedByName = userNames.TryGetValue(n.CreatedByUserId, out var authorName) ? authorName : null,
                        CreatedAtUtc = n.CreatedAtUtc,
                    }).ToList(),
            };
        }

        private static bool TryParseRequiredEnum<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
            => Enum.TryParse(value, ignoreCase: true, out result) && Enum.IsDefined(result);

        private static TEnum? ParseOptionalEnum<TEnum>(string? value) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result) && Enum.IsDefined(result)
                ? result
                : null;
        }

        private static string? Norm(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>Sorts the Candidate Master list. Unrecognized/absent SortBy falls back to CreatedAtUtc.</summary>
        private static IEnumerable<CandidateListItemDto> ApplySort(
            IEnumerable<CandidateListItemDto> items, string? sortBy, bool descending)
        {
            Func<CandidateListItemDto, object> keySelector = (Norm(sortBy)?.ToLowerInvariant()) switch
            {
                "name" => d => d.FullName,
                "email" => d => d.Email,
                "experience" => d => d.TotalExperienceYears,
                "status" => d => d.Status,
                _ => d => d.CreatedAtUtc,
            };

            return descending ? items.OrderByDescending(keySelector) : items.OrderBy(keySelector);
        }

        private const string MaskedValue = "XXXX";

        private static bool IsAdmin(string role) => string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        private static bool IsViewer(string role) => string.Equals(role, "Viewer", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Applies every role-based field restriction to a list-item DTO before it leaves the
        /// service: Viewer gets Name/Email/Phone masked as "XXXX"; CostToCompany is cleared
        /// unless Admin; CurrentSalary is cleared unless Admin or Recruiter. CostToVendor is
        /// visible to everyone, per the current permission model.
        /// </summary>
        private static void ApplyFieldVisibility(CandidateListItemDto dto, string actorRole)
        {
            if (IsViewer(actorRole))
            {
                dto.FullName = MaskedValue;
                dto.Email = MaskedValue;
                dto.Phone = dto.Phone is null ? null : MaskedValue;
                dto.LinkedInUrl = dto.LinkedInUrl is null ? null : MaskedValue;
            }
        }

        /// <summary>
        /// Applies every role-based field restriction to a detail DTO before it leaves the
        /// service: Viewer gets Name/Email/Phone/LinkedIn masked as "XXXX"; CostToCompany is
        /// cleared unless Admin; CurrentSalary is cleared unless Admin or Recruiter.
        /// CostToVendor is visible to everyone, per the current permission model.
        /// </summary>
        private static void ApplyFieldVisibility(CandidateDetailDto dto, string actorRole)
        {
            if (IsViewer(actorRole))
            {
                dto.FullName = MaskedValue;
                dto.Email = MaskedValue;
                dto.Phone = dto.Phone is null ? null : MaskedValue;
                dto.LinkedInUrl = dto.LinkedInUrl is null ? null : MaskedValue;
            }

            if (!IsAdmin(actorRole))
            {
                dto.CostToCompany = null;
            }

            if (!IsAdmin(actorRole) && !string.Equals(actorRole, "Recruiter", StringComparison.OrdinalIgnoreCase))
            {
                dto.CurrentSalary = null;
            }
        }
    }
}
