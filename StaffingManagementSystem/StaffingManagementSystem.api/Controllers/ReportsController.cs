using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
        [Produces("text/csv")]
        public async Task<IActionResult> ExportSearch([FromQuery] CandidateSearchRequestDto request)
        {
            var items = await _searchService.SearchAllAsync(request);
            var csvBytes = BuildCsv(items);

            var fileName = $"candidates-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
            return File(csvBytes, "text/csv", fileName);
        }

        /// <summary>Exports every candidate matching the same filters as <see cref="Search"/> to a formatted PDF (no pagination).</summary>
        [HttpGet("search/export/pdf")]
        [Produces("application/pdf")]
        public async Task<IActionResult> ExportSearchPdf([FromQuery] CandidateSearchRequestDto request, [FromQuery] string? reportTitle)
        {
            var items = await _searchService.SearchAllAsync(request);
            var pdfBytes = BuildPdf(items, string.IsNullOrWhiteSpace(reportTitle) ? "Candidate Report" : reportTitle, request);

            var fileName = $"candidates-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        /// <summary>Grouped counts backing the skill-wise, experience-wise and company-wise report views.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<CandidateReportSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Summary()
        {
            var result = await _searchService.GetReportSummaryAsync();
            return Ok(result);
        }

        // ---------- CSV export ----------

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

        // ---------- PDF export ----------

        private static byte[] BuildPdf(List<CandidateListItemDto> items, string reportTitle, CandidateSearchRequestDto request)
        {
            var filterSummary = BuildFilterSummary(request);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(28);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().Text(reportTitle).FontSize(16).Bold();
                        column.Item().PaddingTop(2).Text(
                            $"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC  ·  {items.Count} candidate(s){(filterSummary.Length > 0 ? $"  ·  {filterSummary}" : string.Empty)}")
                            .FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2f);
                            columns.RelativeColumn(2.2f);
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(0.9f);
                            columns.RelativeColumn(1.8f);
                            columns.RelativeColumn(2.6f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("Name");
                            header.Cell().Element(HeaderCellStyle).Text("Email");
                            header.Cell().Element(HeaderCellStyle).Text("Location");
                            header.Cell().Element(HeaderCellStyle).Text("Status");
                            header.Cell().Element(HeaderCellStyle).Text("Exp (yrs)");
                            header.Cell().Element(HeaderCellStyle).Text("Current Company");
                            header.Cell().Element(HeaderCellStyle).Text("Skills");
                            header.Cell().Element(HeaderCellStyle).Text("Owner Recruiter");
                        });

                        foreach (var item in items)
                        {
                            table.Cell().Element(BodyCellStyle).Text(item.FullName);
                            table.Cell().Element(BodyCellStyle).Text(item.Email);
                            table.Cell().Element(BodyCellStyle).Text(item.CurrentLocation ?? "-");
                            table.Cell().Element(BodyCellStyle).Text(item.Status);
                            table.Cell().Element(BodyCellStyle).Text(item.TotalExperienceYears.ToString("0.0"));
                            table.Cell().Element(BodyCellStyle).Text(item.CurrentCompany ?? "-");
                            table.Cell().Element(BodyCellStyle).Text(string.Join(", ", item.Skills));
                            table.Cell().Element(BodyCellStyle).Text(item.OwnerRecruiterName ?? "-");
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer HeaderCellStyle(IContainer container)
            => container.Background(Colors.Grey.Lighten3).Padding(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).DefaultTextStyle(x => x.Bold());

        private static IContainer BodyCellStyle(IContainer container)
            => container.Padding(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);

        private static string BuildFilterSummary(CandidateSearchRequestDto request)
        {
            var parts = new List<string>();

            if (request.Skills is { Count: > 0 })
            {
                var mode = string.Equals(request.SkillMatchMode, "AND", StringComparison.OrdinalIgnoreCase) ? "all of" : "any of";
                parts.Add($"skills: {mode} {string.Join(", ", request.Skills)}");
            }

            if (!string.IsNullOrWhiteSpace(request.SkillProficiency))
            {
                parts.Add($"proficiency: {request.SkillProficiency}");
            }

            if (request.MinYearsInSkill.HasValue)
            {
                parts.Add($"min years in skill: {request.MinYearsInSkill.Value}");
            }

            if (request.MinExperience.HasValue || request.MaxExperience.HasValue)
            {
                parts.Add($"experience: {request.MinExperience ?? 0}-{(request.MaxExperience.HasValue ? request.MaxExperience.Value.ToString("0.#") : "+")} yrs");
            }

            if (!string.IsNullOrWhiteSpace(request.Company))
            {
                parts.Add($"company: {request.Company}");
            }

            if (!string.IsNullOrWhiteSpace(request.Designation))
            {
                parts.Add($"designation: {request.Designation}");
            }

            if (!string.IsNullOrWhiteSpace(request.Location))
            {
                parts.Add($"location: {request.Location}");
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                parts.Add($"status: {request.Status}");
            }

            return string.Join("  ·  ", parts);
        }
    }
}
