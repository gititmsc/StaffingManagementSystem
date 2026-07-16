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
    }
}
