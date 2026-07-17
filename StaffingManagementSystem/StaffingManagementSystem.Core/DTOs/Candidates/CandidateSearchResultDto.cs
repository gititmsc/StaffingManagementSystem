namespace StaffingManagementSystem.Core.DTOs.Candidates
{
    /// <summary>Paginated result shape returned by GET /api/reports/search.</summary>
    public class CandidateSearchResultDto
    {
        public List<CandidateListItemDto> Items { get; set; } = new();

        public int TotalCount { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
    }
}
