using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StaffingManagementSystem.Core.Configuration;
using StaffingManagementSystem.Core.Interfaces;

namespace StaffingManagementSystem.Infrastructure.Security
{
    /// <inheritdoc cref="IRecaptchaVerifier"/>
    public class RecaptchaVerifier : IRecaptchaVerifier
    {
        private const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

        private readonly HttpClient _httpClient;
        private readonly RecaptchaSettings _settings;
        private readonly ILogger<RecaptchaVerifier> _logger;

        public RecaptchaVerifier(HttpClient httpClient, IOptions<RecaptchaSettings> options, ILogger<RecaptchaVerifier> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<bool> VerifyAsync(string token, CancellationToken cancellationToken = default)
        {
            if (!_settings.IsConfigured)
            {
                _logger.LogWarning("Recaptcha secret key is not configured — skipping CAPTCHA verification.");
                return true;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            try
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = _settings.SecretKey,
                    ["response"] = token,
                });

                using var response = await _httpClient.PostAsync(VerifyUrl, content, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Recaptcha verification request failed with status {StatusCode}.", response.StatusCode);
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<RecaptchaVerifyResponse>(cancellationToken: cancellationToken);
                return result?.Success ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Recaptcha verification call failed.");
                return false;
            }
        }

        private sealed class RecaptchaVerifyResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }
        }
    }
}
