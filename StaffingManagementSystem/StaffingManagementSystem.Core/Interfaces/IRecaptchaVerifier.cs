namespace StaffingManagementSystem.Core.Interfaces
{
    /// <summary>
    /// Verifies a Google reCAPTCHA v2 token submitted by the public candidate
    /// self-registration form. Implemented in the Infrastructure layer.
    /// </summary>
    public interface IRecaptchaVerifier
    {
        /// <summary>
        /// True if the token is valid. Also true (with a logged warning) when the secret key
        /// is still the unconfigured placeholder, so local/dev testing isn't blocked before
        /// real reCAPTCHA keys are issued.
        /// </summary>
        Task<bool> VerifyAsync(string token, CancellationToken cancellationToken = default);
    }
}
