using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Users;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Api.Controllers
{
    /// <summary>
    /// User &amp; role administration endpoints — thin controller, all logic in IUserManagementService.
    /// Admin-only; Recruiter and Viewer have no access to user management.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    [Authorize]
    public sealed class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UsersController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        /// <summary>Lists every non-deleted user.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<List<UserListItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userManagementService.GetAllUsersAsync();
            return Ok(result);
        }

        /// <summary>Gets a single user by id.</summary>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<UserListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserListItemDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _userManagementService.GetUserByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Creates a user account. The new account has no usable password until the invitee
        /// redeems the "set up your password" email this triggers.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<UserListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserListItemDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<UserListItemDto>.FailureResponse("Validation failed.", errors));
            }

            var result = await _userManagementService.CreateUserAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Updates a user's profile fields and role.</summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<UserListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserListItemDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<UserListItemDto>.FailureResponse("Validation failed.", errors));
            }

            var result = await _userManagementService.UpdateUserAsync(id, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Activates or deactivates a user. An admin cannot deactivate their own account.</summary>
        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetStatus(Guid id, [FromBody] SetUserStatusRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<object>.FailureResponse("Validation failed.", errors));
            }

            var result = await _userManagementService.SetUserStatusAsync(id, request.IsActive, GetActingUserId());
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Soft-deletes a user. An admin cannot delete their own account.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userManagementService.DeleteUserAsync(id, GetActingUserId());
            return result.Success ? Ok(result) : BadRequest(result);
        }

        private Guid GetActingUserId()
            => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
