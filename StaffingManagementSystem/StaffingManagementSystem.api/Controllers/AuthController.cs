using Microsoft.AspNetCore.Mvc;
using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Auth;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Api.Controllers
{
    /// <summary>
    /// Authentication endpoints — thin controller, all logic in IAuthService.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticates a user with email and password and returns a JWT access token.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<LoginResponseDto>.FailureResponse("Validation failed.", errors));
            }

            var result = await _authService.LoginAsync(request);

            return result.Success ? Ok(result) : Unauthorized(result);
        }

        /// <summary>
        /// Starts the "forgot password" flow: if the email matches an active account, a
        /// single-use reset link is emailed to it. The response is always the same generic
        /// success message so this endpoint cannot be used to enumerate registered accounts.
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<object>.FailureResponse("Validation failed.", errors));
            }

            var result = await _authService.ForgotPasswordAsync(request);

            return Ok(result);
        }

        /// <summary>
        /// Redeems a password reset token (from the emailed link) and sets a new password.
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<object>.FailureResponse("Validation failed.", errors));
            }

            var result = await _authService.ResetPasswordAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
