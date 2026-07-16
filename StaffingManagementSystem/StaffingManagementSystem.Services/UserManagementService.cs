using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.Configuration;
using StaffingManagementSystem.Core.DTOs.Users;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Core.Enums;
using StaffingManagementSystem.Core.Interfaces;
using StaffingManagementSystem.Repositories.Interfaces;
using StaffingManagementSystem.Services.Interfaces;
using StaffingManagementSystem.Services.Security;

namespace StaffingManagementSystem.Services
{
    /// <inheritdoc cref="IUserManagementService"/>
    public class UserManagementService : IUserManagementService
    {
        /// <summary>How long an account-setup link stays valid after a user is created.</summary>
        private const int SetupTokenExpiryMinutes = 60;

        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly AppUrlSettings _appUrlSettings;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IPasswordHasher passwordHasher,
            IEmailService emailService,
            IOptions<AppUrlSettings> appUrlOptions,
            ILogger<UserManagementService> logger)
        {
            _userRepository = userRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _appUrlSettings = appUrlOptions.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<List<UserListItemDto>>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return ApiResponse<List<UserListItemDto>>.SuccessResponse(users.Select(ToDto).ToList());
        }

        public async Task<ApiResponse<UserListItemDto>> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
            {
                return ApiResponse<UserListItemDto>.FailureResponse("User not found.", ["User not found."]);
            }

            return ApiResponse<UserListItemDto>.SuccessResponse(ToDto(user));
        }

        public async Task<ApiResponse<UserListItemDto>> CreateUserAsync(CreateUserRequestDto request)
        {
            if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role) || !Enum.IsDefined(role))
            {
                return ApiResponse<UserListItemDto>.FailureResponse("Invalid role.", ["Invalid role."]);
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            if (await _userRepository.EmailExistsAsync(normalizedEmail))
            {
                return ApiResponse<UserListItemDto>.FailureResponse(
                    "A user with this email address already exists.",
                    ["A user with this email address already exists."]);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = normalizedEmail,
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim(),
                Role = role,
                IsActive = true,
                // No one knows this password — the account only becomes usable once the user
                // redeems the "set up your password" link emailed below.
                PasswordHash = _passwordHasher.Hash(ResetTokenHelper.GenerateSecureToken()),
                CreatedAtUtc = DateTime.UtcNow,
            };

            await _userRepository.CreateAsync(user);

            var rawToken = ResetTokenHelper.GenerateSecureToken();
            await _passwordResetTokenRepository.CreateAsync(new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = ResetTokenHelper.HashToken(rawToken),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(SetupTokenExpiryMinutes),
                CreatedAtUtc = DateTime.UtcNow,
            });

            var setupLink = $"{_appUrlSettings.FrontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(rawToken)}";
            var message = "User created. An email has been sent so they can set their password.";

            try
            {
                await _emailService.SendAccountSetupEmailAsync(user.Email, user.FullName, setupLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send account setup email for user {UserId}", user.Id);
                message = "User created, but the setup email could not be sent. Ask them to use \"Forgot password\" on the login page instead.";
            }

            return ApiResponse<UserListItemDto>.SuccessResponse(ToDto(user), message);
        }

        public async Task<ApiResponse<UserListItemDto>> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
        {
            if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role) || !Enum.IsDefined(role))
            {
                return ApiResponse<UserListItemDto>.FailureResponse("Invalid role.", ["Invalid role."]);
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
            {
                return ApiResponse<UserListItemDto>.FailureResponse("User not found.", ["User not found."]);
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            user.Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim();
            user.Role = role;

            await _userRepository.UpdateAsync(user);

            return ApiResponse<UserListItemDto>.SuccessResponse(ToDto(user), "User updated.");
        }

        public async Task<ApiResponse<object>> SetUserStatusAsync(Guid id, bool isActive, Guid actingUserId)
        {
            if (id == actingUserId && !isActive)
            {
                return ApiResponse<object>.FailureResponse(
                    "You cannot deactivate your own account.",
                    ["You cannot deactivate your own account."]);
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
            {
                return ApiResponse<object>.FailureResponse("User not found.", ["User not found."]);
            }

            await _userRepository.SetActiveStatusAsync(id, isActive);

            return ApiResponse<object>.SuccessResponse(new { }, isActive ? "User activated." : "User deactivated.");
        }

        public async Task<ApiResponse<object>> DeleteUserAsync(Guid id, Guid actingUserId)
        {
            if (id == actingUserId)
            {
                return ApiResponse<object>.FailureResponse(
                    "You cannot delete your own account.",
                    ["You cannot delete your own account."]);
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
            {
                return ApiResponse<object>.FailureResponse("User not found.", ["User not found."]);
            }

            await _userRepository.SoftDeleteAsync(id);

            return ApiResponse<object>.SuccessResponse(new { }, "User deleted.");
        }

        private static UserListItemDto ToDto(User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Department = user.Department,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            LastLoginAtUtc = user.LastLoginAtUtc,
        };
    }
}
