using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Core.Enums;
using StaffingManagementSystem.Repositories.Interfaces;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Services
{
    /// <inheritdoc cref="ICandidateService"/>
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IUserRepository _userRepository;

        public CandidateService(ICandidateRepository candidateRepository, IUserRepository userRepository)
        {
            _candidateRepository = candidateRepository;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<List<CandidateListItemDto>>> GetAllCandidatesAsync()
        {
            var candidates = await _candidateRepository.GetAllAsync();
            var userNames = await GetUserNameLookupAsync();

            return ApiResponse<List<CandidateListItemDto>>.SuccessResponse(
                candidates.Select(c => MapToListItemDto(c, userNames)).ToList());
        }

        public async Task<ApiResponse<CandidateDetailDto>> GetCandidateByIdAsync(Guid id)
        {
            var candidate = await _candidateRepository.GetByIdAsync(id);
            if (candidate is null)
            {
                return ApiResponse<CandidateDetailDto>.FailureResponse("Candidate not found.", ["Candidate not found."]);
            }

            var userNames = await GetUserNameLookupAsync();
            return ApiResponse<CandidateDetailDto>.SuccessResponse(MapToDetailDto(candidate, userNames));
        }

        public async Task<ApiResponse<CandidateDetailDto>> CreateCandidateAsync(CreateCandidateRequestDto request, Guid ownerRecruiterId)
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
            var experience = BuildExperience(candidateId, request.Experience);
            var education = BuildEducation(candidateId, request.Education);
            var projects = BuildProjects(candidateId, request.Projects);

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
                TotalExperienceYears = CalculateTotalExperienceYears(experience),
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

            var message = isDuplicateEmail
                ? "Candidate created. Note: another candidate already uses this email address."
                : "Candidate created.";

            return ApiResponse<CandidateDetailDto>.SuccessResponse(dto, message);
        }

        public async Task<ApiResponse<CandidateDetailDto>> UpdateCandidateAsync(Guid id, UpdateCandidateRequestDto request)
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
            var experience = BuildExperience(id, request.Experience);
            var education = BuildEducation(id, request.Education);
            var projects = BuildProjects(id, request.Projects);

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
                TotalExperienceYears = CalculateTotalExperienceYears(experience),
            };

            await _candidateRepository.UpdateAsync(updated, skills, experience, education, projects);

            var refreshed = await _candidateRepository.GetByIdAsync(id);
            var userNames = await GetUserNameLookupAsync();
            var dto = MapToDetailDto(refreshed!, userNames);

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

        private static List<CandidateExperience> BuildExperience(Guid candidateId, List<CandidateExperienceInputDto> inputs)
            => inputs.Select(input => new CandidateExperience
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                CompanyName = input.CompanyName.Trim(),
                JobTitle = input.JobTitle.Trim(),
                EmploymentType = ParseOptionalEnum<EmploymentType>(input.EmploymentType),
                StartDate = input.StartDate,
                EndDate = input.IsCurrent ? null : input.EndDate,
                IsCurrent = input.IsCurrent,
                Location = Norm(input.Location),
                Description = Norm(input.Description),
            }).ToList();

        private static List<CandidateEducation> BuildEducation(Guid candidateId, List<CandidateEducationInputDto> inputs)
            => inputs.Select(input => new CandidateEducation
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                Degree = input.Degree.Trim(),
                Institution = input.Institution.Trim(),
                FieldOfStudy = Norm(input.FieldOfStudy),
                StartYear = input.StartYear,
                EndYear = input.EndYear,
                IsExpected = input.IsExpected,
                Grade = Norm(input.Grade),
            }).ToList();

        private static List<CandidateProject> BuildProjects(Guid candidateId, List<CandidateProjectInputDto> inputs)
            => inputs.Select(input => new CandidateProject
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                ProjectName = input.ProjectName.Trim(),
                Role = Norm(input.Role),
                DurationText = Norm(input.DurationText),
                TechnologiesUsed = Norm(input.TechnologiesUsed),
                Description = Norm(input.Description),
            }).ToList();

        /// <summary>
        /// Sums each experience entry's duration in months and converts to years (1 decimal place).
        /// Overlapping date ranges are not de-duplicated — this is an estimate, not a precise total.
        /// </summary>
        private static decimal CalculateTotalExperienceYears(List<CandidateExperience> experience)
        {
            var totalMonths = 0;
            var now = DateTime.UtcNow;

            foreach (var entry in experience)
            {
                var end = entry.IsCurrent ? now : (entry.EndDate ?? entry.StartDate);
                if (end < entry.StartDate)
                {
                    continue;
                }

                var months = ((end.Year - entry.StartDate.Year) * 12) + (end.Month - entry.StartDate.Month);
                totalMonths += Math.Max(months, 0);
            }

            return Math.Round(totalMonths / 12m, 1);
        }

        private static CandidateListItemDto MapToListItemDto(Candidate candidate, Dictionary<Guid, string> userNames)
        {
            userNames.TryGetValue(candidate.OwnerRecruiterId, out var ownerName);
            var currentExperience = candidate.Experience.FirstOrDefault(e => e.IsCurrent);

            return new CandidateListItemDto
            {
                Id = candidate.Id,
                FullName = candidate.FullName,
                Title = candidate.Title,
                Email = candidate.Email,
                Phone = candidate.Phone,
                CurrentLocation = candidate.CurrentLocation,
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
            userNames.TryGetValue(candidate.OwnerRecruiterId, out var ownerName);

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
    }
}
