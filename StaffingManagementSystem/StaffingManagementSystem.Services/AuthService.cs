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
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IEmailService _emailService;
        private readonly AppUrlSettings _appUrlSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IEmailService emailService,
            IOptions<AppUrlSettings> appUrlOptions,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
            _appUrlSettings = appUrlOptions.Value;
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
            await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow);

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
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

            return ApiResponse<object>.SuccessResponse(
                new { },
                "Your password has been reset. You can now sign in with your new password.");
        }
    }
}
