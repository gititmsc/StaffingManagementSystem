using StaffingManagementSystem.Core.DTOs.Candidates;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Core.Enums;

namespace StaffingManagementSystem.Services
{
    /// <summary>
    /// Builds a candidate's Experience/Education/Projects child graph and derives
    /// TotalExperienceYears from it. Shared by <see cref="CandidateService"/> (authenticated
    /// create/update) and <see cref="CandidateRegistrationService"/> (public self-registration)
    /// so both paths compute experience totals identically.
    /// </summary>
    internal static class CandidateGraphBuilder
    {
        public static List<CandidateExperience> BuildExperience(Guid candidateId, List<CandidateExperienceInputDto> inputs)
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

        public static List<CandidateEducation> BuildEducation(Guid candidateId, List<CandidateEducationInputDto> inputs)
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

        public static List<CandidateProject> BuildProjects(Guid candidateId, List<CandidateProjectInputDto> inputs)
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
        public static decimal CalculateTotalExperienceYears(List<CandidateExperience> experience)
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
