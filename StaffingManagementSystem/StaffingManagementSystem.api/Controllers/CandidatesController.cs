using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Api.Controllers
{
    /// <summary>
    /// Candidate master endpoints — thin controller, all logic in ICandidateService.
    /// Access follows the Section 7 permission matrix: SuperAdmin, HRAdmin and Recruiter have
    /// full CRUD; HiringManager and Viewer are read-only.
    /// </summary>
    [ApiController]
    [Route("api/candidates")]
    [Produces("application/json")]
    [Authorize]
    public sealed class CandidatesController : ControllerBase
    {
        private const string EditRoles = "SuperAdmin,HRAdmin,Recruiter";

        private readonly ICandidateService _candidateService;
        private readonly ICandidateAttachmentService _attachmentService;

        public CandidatesController(ICandidateService candidateService, ICandidateAttachmentService attachmentService)
        {
            _candidateService = candidateService;
            _attachmentService = attachmentService;
        }

        /// <summary>Lists every non-deleted candidate.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CandidateListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _candidateService.GetAllCandidatesAsync();
            return Ok(result);
        }

        /// <summary>Gets a single candidate's full profile.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CandidateDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CandidateDetailDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _candidateService.GetCandidateByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>Creates a candidate profile. The creating user becomes the owning recruiter.</summary>
        [HttpPost]
        [Authorize(Roles = EditRoles)]
        [ProducesResponseType(typeof(ApiResponse<CandidateDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CandidateDetailDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCandidateRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<CandidateDetailDto>.FailureResponse("Validation failed.", errors));
            }

            var result = await _candidateService.CreateCandidateAsync(request, GetActingUserId());
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Replaces a candidate's profile fields and its skills/experience/education/projects.</summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = EditRoles)]
        [ProducesResponseType(typeof(ApiResponse<CandidateDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CandidateDetailDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCandidateRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<CandidateDetailDto>.FailureResponse("Validation failed.", errors));
            }

            var result = await _candidateService.UpdateCandidateAsync(id, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Soft-deletes a candidate.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = EditRoles)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _candidateService.DeleteCandidateAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Appends a timestamped, attributed note to a candidate's activity log.</summary>
        [HttpPost("{id:guid}/notes")]
        [Authorize(Roles = EditRoles)]
        [ProducesResponseType(typeof(ApiResponse<CandidateNoteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CandidateNoteDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddNote(Guid id, [FromBody] AddCandidateNoteRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<CandidateNoteDto>.FailureResponse("Validation failed.", errors));
            }

            var result = await _candidateService.AddNoteAsync(id, request, GetActingUserId());
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Lists a candidate's attachments, including the resume if one has been uploaded.</summary>
        [HttpGet("{id:guid}/attachments")]
        [ProducesResponseType(typeof(ApiResponse<List<CandidateAttachmentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<CandidateAttachmentDto>>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAttachments(Guid id)
        {
            var result = await _attachmentService.GetByCandidateIdAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Uploads a new general (non-resume) attachment for a candidate.</summary>
        [HttpPost("{id:guid}/attachments")]
        [Authorize(Roles = EditRoles)]
        [RequestSizeLimit(20_000_000)]
        [ProducesResponseType(typeof(ApiResponse<CandidateAttachmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CandidateAttachmentDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadAttachment(Guid id, IFormFile? file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(ApiResponse<CandidateAttachmentDto>.FailureResponse(
                    "Please choose a file to upload.", ["Please choose a file to upload."]));
            }

            await using var stream = file.OpenReadStream();
            var result = await _attachmentService.UploadAsync(
                id, file.FileName, file.ContentType, file.Length, stream, GetActingUserId());

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Uploads (or replaces) a candidate's resume. A candidate has at most one active
        /// resume — uploading a new one deletes the previous resume file and row. Kept as a
        /// distinct endpoint/type so resumes never get mixed in with general attachments.
        /// </summary>
        [HttpPost("{id:guid}/resume")]
        [Authorize(Roles = EditRoles)]
        [RequestSizeLimit(20_000_000)]
        [ProducesResponseType(typeof(ApiResponse<CandidateAttachmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CandidateAttachmentDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadResume(Guid id, IFormFile? file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(ApiResponse<CandidateAttachmentDto>.FailureResponse(
                    "Please choose a file to upload.", ["Please choose a file to upload."]));
            }

            await using var stream = file.OpenReadStream();
            var result = await _attachmentService.UploadResumeAsync(
                id, file.FileName, file.ContentType, file.Length, stream, GetActingUserId());

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Downloads a candidate attachment (resume or general document).</summary>
        [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId)
        {
            var download = await _attachmentService.GetForDownloadAsync(attachmentId);
            if (download is null)
            {
                return NotFound(ApiResponse<object>.FailureResponse("Attachment not found.", ["Attachment not found."]));
            }

            return File(download.Content, download.ContentType, download.FileName);
        }

        /// <summary>Deletes a candidate attachment (resume or general document).</summary>
        [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
        [Authorize(Roles = EditRoles)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId)
        {
            var result = await _attachmentService.DeleteAsync(attachmentId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        private Guid GetActingUserId()
            => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
