using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;
using StaffingManagementSystem.Core.DTOs.Dashboard;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Core.Enums;
using StaffingManagementSystem.Repositories.Interfaces;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Services
{
    /// <inheritdoc cref="IDashboardService"/>
    public class DashboardService : IDashboardService
    {
        private const int RecentListSize = 5;
        private const int TopSkillsSize = 8;
        private const int RecentDays = 7;

        private readonly ICandidateRepository _candidateRepository;
        private readonly IUserRepository _userRepository;

        public DashboardService(ICandidateRepository candidateRepository, IUserRepository userRepository)
        {
            _candidateRepository = candidateRepository;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(string actorRole, Guid actorUserId)
        {
            var candidates = await _candidateRepository.GetAllAsync();
            var userNames = await GetUserNameLookupAsync();

            var summary = new DashboardSummaryDto();

            if (IsAdmin(actorRole))
            {
                PopulateAdmin(summary, candidates, userNames);
            }
            else if (IsRecruiter(actorRole))
            {
                PopulateRecruiter(summary, candidates, userNames, actorUserId);
            }
            else
            {
                PopulateViewer(summary, candidates);
            }

            return ApiResponse<DashboardSummaryDto>.SuccessResponse(summary);
        }

        // ---------- per-role population ----------

        private static void PopulateAdmin(
            DashboardSummaryDto summary, List<Candidate> candidates, Dictionary<Guid, string> userNames)
        {
            summary.StatusCounts = ComputeStatusCounts(candidates);
            summary.PendingApprovalsCount = candidates.Count(c => c.Status == CandidateStatus.PendingApproval);

            summary.RecentlyApproved = candidates
                .Where(c => c.Status == CandidateStatus.Approved && c.ApprovedAtUtc.HasValue)
                .OrderByDescending(c => c.ApprovedAtUtc)
                .Take(RecentListSize)
                .Select(c => ToDashboardCandidateDto(c, c.ApprovedAtUtc!.Value, userNames))
                .ToList();

            summary.RecentlyRejected = candidates
                .Where(c => c.Status == CandidateStatus.Rejected && c.RejectedAtUtc.HasValue)
                .OrderByDescending(c => c.RejectedAtUtc)
                .Take(RecentListSize)
                .Select(c => ToDashboardCandidateDto(c, c.RejectedAtUtc!.Value, userNames))
                .ToList();

            summary.RecruiterWorkload = candidates
                .Where(c => c.OwnerRecruiterId.HasValue)
                .GroupBy(c => c.OwnerRecruiterId!.Value)
                .Select(g => new NameCountDto
                {
                    Name = userNames.GetValueOrDefault(g.Key, "Unknown"),
                    Count = g.Count(),
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name)
                .ToList();

            summary.TopSkills = candidates
                .SelectMany(c => c.Skills.Select(s => s.Skill?.Name).Where(n => !string.IsNullOrWhiteSpace(n)))
                .GroupBy(n => n!, StringComparer.OrdinalIgnoreCase)
                .Select(g => new NameCountDto { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name)
                .Take(TopSkillsSize)
                .ToList();

            var recentCutoff = DateTime.UtcNow.AddDays(-RecentDays);
            summary.RecentRegistrations = candidates
                .Where(c => c.OwnerRecruiterId is null && c.CreatedAtUtc >= recentCutoff)
                .OrderByDescending(c => c.CreatedAtUtc)
                .Select(c => ToDashboardCandidateDto(c, c.CreatedAtUtc, userNames))
                .ToList();
        }

        private static void PopulateRecruiter(
            DashboardSummaryDto summary, List<Candidate> candidates, Dictionary<Guid, string> userNames, Guid actorUserId)
        {
            var visible = VisibleForNonAdmin(candidates);
            var mine = visible.Where(c => c.OwnerRecruiterId == actorUserId).ToList();

            summary.MyCandidatesCount = mine.Count;
            summary.MyCandidatesByStatus = ComputeStatusCounts(mine);
            summary.MyInProcessCount = mine.Count(c => c.Status == CandidateStatus.InProcess);

            var recentCutoff = DateTime.UtcNow.AddDays(-RecentDays);
            summary.RecentlyAddedSystemWide = visible
                .Where(c => c.CreatedAtUtc >= recentCutoff)
                .OrderByDescending(c => c.CreatedAtUtc)
                .Select(c => ToDashboardCandidateDto(c, c.CreatedAtUtc, userNames))
                .ToList();
        }

        private static void PopulateViewer(DashboardSummaryDto summary, List<Candidate> candidates)
        {
            var visible = VisibleForNonAdmin(candidates);

            summary.TotalVisibleCandidates = visible.Count;
            summary.StatusCounts = ComputeStatusCounts(visible);
        }

        // ---------- helpers ----------

        /// <summary>
        /// Candidates a Recruiter/Viewer can ever see — excludes PendingApproval/Rejected,
        /// matching the row-level visibility rule in CandidateService.GetAllCandidatesAsync.
        /// </summary>
        private static List<Candidate> VisibleForNonAdmin(List<Candidate> candidates)
            => candidates
                .Where(c => c.Status != CandidateStatus.PendingApproval && c.Status != CandidateStatus.Rejected)
                .ToList();

        /// <summary>New/Available/InProcess/Placed/OnHold/Blacklisted, with Approved folded into New
        /// to match the Candidate Master list's display-only "New" relabel for Approved candidates.</summary>
        private static List<NameCountDto> ComputeStatusCounts(List<Candidate> candidates)
        {
            int CountFor(params CandidateStatus[] statuses) => candidates.Count(c => statuses.Contains(c.Status));

            return new List<NameCountDto>
            {
                new() { Name = "New", Count = CountFor(CandidateStatus.New, CandidateStatus.Approved) },
                new() { Name = "Available", Count = CountFor(CandidateStatus.Available) },
                new() { Name = "InProcess", Count = CountFor(CandidateStatus.InProcess) },
                new() { Name = "Placed", Count = CountFor(CandidateStatus.Placed) },
                new() { Name = "OnHold", Count = CountFor(CandidateStatus.OnHold) },
                new() { Name = "Blacklisted", Count = CountFor(CandidateStatus.Blacklisted) },
            };
        }

        private static DashboardCandidateDto ToDashboardCandidateDto(
            Candidate candidate, DateTime eventAtUtc, Dictionary<Guid, string> userNames)
            => new()
            {
                Id = candidate.Id,
                FullName = candidate.FullName,
                Status = candidate.Status.ToString(),
                EventAtUtc = eventAtUtc,
                OwnerRecruiterName = candidate.OwnerRecruiterId.HasValue
                    ? userNames.GetValueOrDefault(candidate.OwnerRecruiterId.Value)
                    : null,
            };

        private async Task<Dictionary<Guid, string>> GetUserNameLookupAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.ToDictionary(u => u.Id, u => u.FullName);
        }

        private static bool IsAdmin(string role) => string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        private static bool IsRecruiter(string role) => string.Equals(role, "Recruiter", StringComparison.OrdinalIgnoreCase);
    }
}
