using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.Configuration;
using StaffingManagementSystem.Core.DTOs.Auth;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Core.Interfaces;
using StaffingManagementSystem.Repositories.Interfaces;
using StaffingManagementSystem.Services.Interfaces;
using StaffingManagementSystem.Services.Security;

namespace StaffingManagementSystem.Services
{
    /// <inheritdoc cref="IAuthService"/>
    public class AuthService : IAuthService
    {
        /// <summary>How long a password reset link stays valid after it is issued.</summary>
        private const int ResetTokenExpiryMinutes = 60;

        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IEmailService _emailService;
        private readonly AppUrlSettings _appUrlSettings;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IEmailService emailService,
            IOptions<AppUrlSettings> appUrlOptions,
            IOptions<JwtSettings> jwtOptions,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
            _appUrlSettings = appUrlOptions.Value;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

            if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                return ApiResponse<LoginResponseDto>.FailureResponse(
                    "Invalid email or password.",
                    ["Invalid email or password."]);
            }

            var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);
            var (rawRefreshToken, _) = await IssueRefreshTokenAsync(user.Id);
            await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow);

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                RefreshToken = rawRefreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                },
            };

            return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful.");
        }

        public async Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var tokenHash = ResetTokenHelper.HashToken(request.RefreshToken);
            var existing = await _refreshTokenRepository.GetValidByTokenHashAsync(tokenHash, DateTime.UtcNow);

            if (existing is null)
            {
                return ApiResponse<RefreshTokenResponseDto>.FailureResponse(
                    "Your session has expired. Please sign in again.",
                    ["Invalid or expired refresh token."]);
            }

            var user = await _userRepository.GetByIdAsync(existing.UserId);
            if (user is null || !user.IsActive)
            {
                return ApiResponse<RefreshTokenResponseDto>.FailureResponse(
                    "Your session has expired. Please sign in again.",
                    ["Account is no longer active."]);
            }

            var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);
            var (rawRefreshToken, newTokenId) = await IssueRefreshTokenAsync(user.Id);

            // Rotation: once used, the old refresh token can never be redeemed again — even if
            // it had leaked, it's now worthless.
            await _refreshTokenRepository.RevokeAsync(existing.Id, DateTime.UtcNow, newTokenId);

            var response = new RefreshTokenResponseDto
            {
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                RefreshToken = rawRefreshToken,
            };

            return ApiResponse<RefreshTokenResponseDto>.SuccessResponse(response);
        }

        public async Task<ApiResponse<object>> LogoutAsync(LogoutRequestDto request)
        {
            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                var tokenHash = ResetTokenHelper.HashToken(request.RefreshToken);
                var existing = await _refreshTokenRepository.GetValidByTokenHashAsync(tokenHash, DateTime.UtcNow);

                if (existing is not null)
                {
                    await _refreshTokenRepository.RevokeAsync(existing.Id, DateTime.UtcNow);
                }
            }

            // Always succeeds — whether or not the token was still valid isn't the caller's concern.
            return ApiResponse<object>.SuccessResponse(new { }, "Signed out.");
        }

        public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            const string genericMessage = "If an account exists for that email address, we've sent a password reset link.";

            var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

            if (user is not null && user.IsActive)
            {
                var rawToken = ResetTokenHelper.GenerateSecureToken();

                await _passwordResetTokenRepository.InvalidateActiveTokensForUserAsync(user.Id);
                await _passwordResetTokenRepository.CreateAsync(new PasswordResetToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TokenHash = ResetTokenHelper.HashToken(rawToken),
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(ResetTokenExpiryMinutes),
                    CreatedAtUtc = DateTime.UtcNow,
                });

                var resetLink = $"{_appUrlSettings.FrontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(rawToken)}";

                try
                {
                    await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);
                }
                catch (Exception ex)
                {
                    // Email delivery is best-effort: a downed SMTP server must not reveal
                    // whether the account exists, so we log and still return the generic message.
                    _logger.LogError(ex, "Failed to send password reset email for user {UserId}", user.Id);
                }
            }

            // Always the same response, regardless of whether the email matched an account,
            // so this endpoint can't be used to enumerate registered users.
            return ApiResponse<object>.SuccessResponse(new { }, genericMessage);
        }

        public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return ApiResponse<object>.FailureResponse(
                    "Passwords do not match.",
                    ["Passwords do not match."]);
            }

            var tokenHash = ResetTokenHelper.HashToken(request.Token);
            var resetToken = await _passwordResetTokenRepository.GetValidByTokenHashAsync(tokenHash, DateTime.UtcNow);

            if (resetToken is null)
            {
                return ApiResponse<object>.FailureResponse(
                    "This reset link is invalid or has expired. Please request a new one.",
                    ["Invalid or expired token."]);
            }

            var newPasswordHash = _passwordHasher.Hash(request.NewPassword);
            await _userRepository.UpdatePasswordHashAsync(resetToken.UserId, newPasswordHash);

            await _passwordResetTokenRepository.MarkUsedAsync(resetToken.Id, DateTime.UtcNow);
            await _passwordResetTokenRepository.InvalidateActiveTokensForUserAsync(resetToken.UserId);

            // The password just changed — every other signed-in session (and its ability to
            // silently refresh) must be cut off, not just this one.
            await _refreshTokenRepository.RevokeAllForUserAsync(resetToken.UserId, DateTime.UtcNow);

            return ApiResponse<object>.SuccessResponse(
                new { },
                "Your password has been reset. You can now sign in with your new password.");
        }

        private async Task<(string RawToken, Guid TokenId)> IssueRefreshTokenAsync(Guid userId)
        {
            var rawToken = ResetTokenHelper.GenerateSecureToken();
            var tokenId = Guid.NewGuid();

            await _refreshTokenRepository.CreateAsync(new RefreshToken
            {
                Id = tokenId,
                UserId = userId,
                TokenHash = ResetTokenHelper.HashToken(rawToken),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                CreatedAtUtc = DateTime.UtcNow,
            });

            return (rawToken, tokenId);
        }
    }
}
