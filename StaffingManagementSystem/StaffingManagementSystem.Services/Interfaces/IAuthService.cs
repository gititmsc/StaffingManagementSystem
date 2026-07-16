using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Auth;

namespace StaffingManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Business logic contract for authentication.
    /// </summary>
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// Kicks off the "forgot password" flow. Always returns a generic success response —
        /// whether or not the email belongs to a known account is never revealed to the caller.
        /// </summary>
        Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequestDto request);

        /// <summary>
        /// Redeems a password reset token and sets a new password for the associated user.
        /// </summary>
        Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
