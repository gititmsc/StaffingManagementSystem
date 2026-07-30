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
        /// Exchanges a still-valid refresh token for a new access token, silently extending the
        /// user's session. The refresh token is rotated: the old one is revoked and a new one is
        /// issued and returned, so the caller must persist the new value and discard the old.
        /// </summary>
        Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);

        /// <summary>
        /// Revokes the given refresh token (if any) so it can no longer be used to silently
        /// obtain a new access token. Always succeeds, even if the token was already invalid.
        /// </summary>
        Task<ApiResponse<object>> LogoutAsync(LogoutRequestDto request);

        /// <summary>
        /// Kicks off the "forgot password" flow. Always returns a generic success response —
        /// whether or not the email belongs to a known account is never revealed to the caller.
        /// </summary>
        Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequestDto request);

        /// <summary>
        /// Redeems a password reset token and sets a new password for the associated user.
        /// All of that user's refresh tokens are revoked as part of this, so any other
        /// signed-in session can no longer silently refresh and must sign in again.
        /// </summary>
        Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
