using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Repositories.Interfaces;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Services
{
    /// <inheritdoc cref="ICandidateSearchService"/>
    public class CandidateSearchService : ICandidateSearchService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IUserRepository _userRepository;

        public CandidateSearchService(ICandidateRepository candidateRepository, IUserRepository userRepository)
        {
            _candidateRepository = candidateRepository;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<CandidateSearchResultDto>> SearchAsync(CandidateSearchRequestDto request)
        {
            var (items, totalCount) = await FilterAndSortAsync(request);

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 200);

            var pageItems = items
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

            var result = new CandidateSearchResultDto
            {
                Items = pageItems,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
            };

            return ApiResponse<CandidateSearchResultDto>.SuccessResponse(result);
        }

        public async Task<List<CandidateListItemDto>> SearchAllAsync(CandidateSearchRequestDto request)
        {
            var (items, _) = await FilterAndSortAsync(request);
            return items;
        }

        public async Task<ApiResponse<CandidateReportSummaryDto>> GetReportSummaryAsync()
        {
            var candidates = await _candidateRepository.GetAllAsync();

            var skillCounts = candidates
                .SelectMany(c => c.Skills.Select(s => s.Skill?.Name).Where(n => !string.IsNullOrWhiteSpace(n)))
                .GroupBy(n => n!, StringComparer.OrdinalIgnoreCase)
                .Select(g => new NameCountDto { Name = g.Key, Count = g.Select(_ => true).Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name)
                .Take(20)
                .ToList();

            var experienceBandCounts = new List<NameCountDto>
            {
                new() { Name = "0-2 yrs", Count = candidates.Count(c => c.TotalExperienceYears < 2) },
                new() { Name = "2-5 yrs", Count = candidates.Count(c => c.TotalExperienceYears >= 2 && c.TotalExperienceYears < 5) },
                new() { Name = "5-10 yrs", Count = candidates.Count(c => c.TotalExperienceYears >= 5 && c.TotalExperienceYears < 10) },
                new() { Name = "10+ yrs", Count = candidates.Count(c => c.TotalExperienceYears >= 10) },
            };

            var companyCounts = candidates
                .SelectMany(c => c.Experience.Select(e => e.CompanyName).Where(n => !string.IsNullOrWhiteSpace(n)))
                .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
                .Select(g => new NameCountDto { Name = g.Key, Count = g.Select(_ => true).Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name)
                .Take(20)
                .ToList();

            var summary = new CandidateReportSummaryDto
            {
                SkillCounts = skillCounts,
                ExperienceBandCounts = experienceBandCounts,
                CompanyCounts = companyCounts,
            };

            return ApiResponse<CandidateReportSummaryDto>.SuccessResponse(summary);
        }

        // ---------- helpers ----------

        private async Task<(List<CandidateListItemDto> Items, int TotalCount)> FilterAndSortAsync(CandidateSearchRequestDto request)
        {
            var candidates = await _candidateRepository.GetAllAsync();
            var userNames = await GetUserNameLookupAsync();

            IEnumerable<Candidate> filtered = candidates;

            var requestedSkills = (request.Skills ?? new List<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();

            if (requestedSkills.Count > 0)
            {
                var matchAll = string.Equals(request.SkillMatchMode, "AND", StringComparison.OrdinalIgnoreCase);

                filtered = filtered.Where(c =>
                {
                    var candidateSkillNames = c.Skills
                        .Select(s => s.Skill?.Name ?? string.Empty)
                        .Where(n => n.Length > 0)
                        .ToList();

                    return matchAll
                        ? requestedSkills.All(rs => candidateSkillNames.Any(cs => string.Equals(cs, rs, StringComparison.OrdinalIgnoreCase)))
                        : requestedSkills.Any(rs => candidateSkillNames.Any(cs => string.Equals(cs, rs, StringComparison.OrdinalIgnoreCase)));
                });
            }

            if (request.MinExperience.HasValue)
            {
                filtered = filtered.Where(c => c.TotalExperienceYears >= request.MinExperience.Value);
            }

            if (request.MaxExperience.HasValue)
            {
                filtered = filtered.Where(c => c.TotalExperienceYears <= request.MaxExperience.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Company))
            {
                var term = request.Company.Trim();
                filtered = filtered.Where(c => c.Experience.Any(e => e.CompanyName.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(request.Designation))
            {
                var term = request.Designation.Trim();
                filtered = filtered.Where(c =>
                {
                    var current = c.Experience.FirstOrDefault(e => e.IsCurrent);
                    return current is not null && current.JobTitle.Contains(term, StringComparison.OrdinalIgnoreCase);
                });
            }

            if (!string.IsNullOrWhiteSpace(request.Location))
            {
                var term = request.Location.Trim();
                filtered = filtered.Where(c => !string.IsNullOrWhiteSpace(c.CurrentLocation) && c.CurrentLocation.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                filtered = filtered.Where(c => string.Equals(c.Status.ToString(), request.Status, StringComparison.OrdinalIgnoreCase));
            }

            var materialized = filtered.ToList();
            var totalCount = materialized.Count;

            var sorted = ApplySort(materialized, request.SortBy, request.SortDescending);

            var items = sorted.Select(c => MapToListItemDto(c, userNames)).ToList();

            return (items, totalCount);
        }

        private static List<Candidate> ApplySort(List<Candidate> candidates, string sortBy, bool descending)
        {
            IOrderedEnumerable<Candidate> ordered = sortBy?.ToLowerInvariant() switch
            {
                "experience" => descending
                    ? candidates.OrderByDescending(c => c.TotalExperienceYears)
                    : candidates.OrderBy(c => c.TotalExperienceYears),
                "name" => descending
                    ? candidates.OrderByDescending(c => c.FullName)
                    : candidates.OrderBy(c => c.FullName),
                _ => descending
                    ? candidates.OrderByDescending(c => c.CreatedAtUtc)
                    : candidates.OrderBy(c => c.CreatedAtUtc),
            };

            return ordered.ToList();
        }

        private async Task<Dictionary<Guid, string>> GetUserNameLookupAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.ToDictionary(u => u.Id, u => u.FullName);
        }

        private static CandidateListItemDto MapToListItemDto(Candidate candidate, Dictionary<Guid, string> userNames)
        {
            userNames.TryGetValue(candidate.OwnerRecruiterId, out var ownerName);
            var currentExperience = candidate.Experience.FirstOrDefault(e => e.IsCurrent);

            return new CandidateListItemDto
            {
                Id = candidate.Id,
                FullName = candidate.FullName,
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
    }
}
