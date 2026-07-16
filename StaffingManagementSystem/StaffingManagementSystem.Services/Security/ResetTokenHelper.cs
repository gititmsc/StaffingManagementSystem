using System.Security.Cryptography;
using System.Text;

namespace StaffingManagementSystem.Services.Security
{
    /// <summary>
    /// Shared helpers for issuing and hashing single-use tokens (password reset, account
    /// setup). Only the hash is ever persisted; the raw token is emailed and never stored.
    /// </summary>
    internal static class ResetTokenHelper
    {
        /// <summary>Generates a URL-safe, cryptographically random token.</summary>
        public static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        /// <summary>Hashes a token with SHA-256 so only the hash is ever persisted.</summary>
        public static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }
}
