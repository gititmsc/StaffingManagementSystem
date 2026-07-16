using StaffingManagementSystem.Core.Enums;

namespace StaffingManagementSystem.Core.Entities
{
    /// <summary>
    /// A user account that can authenticate into the Staffing Management System.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        /// <summary>PBKDF2 password hash, stored as "{iterations}.{saltBase64}.{hashBase64}".</summary>
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public DateTime? LastLoginAtUtc { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
