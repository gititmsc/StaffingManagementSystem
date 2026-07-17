using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Api.Controllers
{
    /// <summary>
    /// Advanced candidate search and the Phase 1 standard reports (RMS SRS 3.4 Search &amp;
    /// Reporting / Section 6). Read-only for every authenticated role, per the Section 7
    /// permission matrix.
    /// </summary>
    [ApiController]
    [Route("api/reports")]
    [Produces("application/json")]
    [Authorize]
    public sealed class ReportsController : ControllerBase
    {
        private readonly ICandidateSearchService _searchService;

        public ReportsController(ICandidateSearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>Filtered, sorted, paginated candidate search across skill, experience, company, designation, location and status.</summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<CandidateSearchResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] CandidateSearchRequestDto request)
        {
            var result = await _searchService.SearchAsync(request);
            return Ok(result);
        }

        /// <summary>Exports every candidate matching the same filters as <see cref="Search"/> to CSV (no pagination).</summary>
        [HttpGet("search/export")]
        public async Task<IActionResult> ExportSearch([FromQuery] CandidateSearchRequestDto request)
        {
            var items = await _searchService.SearchAllAsync(request);
            var csvBytes = BuildCsv(items);

            var fileName = $"candidates-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
            return File(csvBytes, "text/csv", fileName);
        }

        /// <summary>Grouped counts backing the skill-wise, experience-wise and company-wise report views.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<CandidateReportSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Summary()
        {
            var result = await _searchService.GetReportSummaryAsync();
            return Ok(result);
        }

        private static byte[] BuildCsv(List<CandidateListItemDto> items)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Full Name,Email,Phone,Location,Status,Total Experience (yrs),Current Company,Skills,Owner Recruiter,Added");

            foreach (var item in items)
            {
                sb.AppendLine(string.Join(",",
                    CsvField(item.FullName),
                    CsvField(item.Email),
                    CsvField(item.Phone),
                    CsvField(item.CurrentLocation),
                    CsvField(item.Status),
                    CsvField(item.TotalExperienceYears.ToString("0.0")),
                    CsvField(item.CurrentCompany),
                    CsvField(string.Join("; ", item.Skills)),
                    CsvField(item.OwnerRecruiterName),
                    CsvField(item.CreatedAtUtc.ToString("yyyy-MM-dd"))));
            }

            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }

        private static string CsvField(string? value)
        {
            var text = value ?? string.Empty;
            var needsQuoting = text.Contains(',') || text.Contains('"') || text.Contains('\n') || text.Contains('\r');

            if (!needsQuoting)
            {
                return text;
            }

            return $"\"{text.Replace("\"", "\"\"")}\"";
        }
    }
}
