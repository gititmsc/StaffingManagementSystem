using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Api.Controllers
{
    /// <summary>
    /// Public, no-login candidate self-registration endpoint. Intentionally carries no
    /// [Authorize] attribute — same convention as AuthController — since anonymous visitors
    /// must be able to reach it without a token.
    /// </summary>
    [ApiController]
    [Route("api/candidate-registration")]
    [Produces("application/json")]
    public sealed class CandidateRegistrationController : ControllerBase
    {
        private readonly ICandidateRegistrationService _registrationService;

        public CandidateRegistrationController(ICandidateRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        /// <summary>Submits a candidate profile + resume for Admin approval.</summary>
        [HttpPost]
        [RequestSizeLimit(52_428_800)] // ~50MB, matches CandidateRegistration:MaxResumeSizeBytes
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromForm] CandidateSelfRegistrationRequestDto request, IFormFile? resume)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<object>.FailureResponse("Validation failed.", errors));
            }

            if (resume is null || resume.Length == 0)
            {
                return BadRequest(ApiResponse<object>.FailureResponse(
                    "Please attach your resume.", ["Please attach your resume."]));
            }

            await using var stream = resume.OpenReadStream();
            var result = await _registrationService.RegisterAsync(
                request, resume.FileName, resume.ContentType, resume.Length, stream);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
