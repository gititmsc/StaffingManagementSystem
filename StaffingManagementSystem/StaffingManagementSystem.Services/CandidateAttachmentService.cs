using Microsoft.Extensions.Options;
using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.Configuration;
using StaffingManagementSystem.Core.DTOs.Candidates;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Core.Interfaces;
using StaffingManagementSystem.Repositories.Interfaces;
using StaffingManagementSystem.Services.Interfaces;

namespace StaffingManagementSystem.Services
{
    /// <inheritdoc cref="ICandidateAttachmentService"/>
    public class CandidateAttachmentService : ICandidateAttachmentService
    {
        private readonly ICandidateAttachmentRepository _attachmentRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly FileStorageSettings _settings;

        public CandidateAttachmentService(
            ICandidateAttachmentRepository attachmentRepository,
            ICandidateRepository candidateRepository,
            IUserRepository userRepository,
            IFileStorageService fileStorageService,
            IOptions<FileStorageSettings> options)
        {
            _attachmentRepository = attachmentRepository;
            _candidateRepository = candidateRepository;
            _userRepository = userRepository;
            _fileStorageService = fileStorageService;
            _settings = options.Value;
        }

        public async Task<ApiResponse<List<CandidateAttachmentDto>>> GetByCandidateIdAsync(Guid candidateId)
        {
            if (!await _candidateRepository.ExistsAsync(candidateId))
            {
                return ApiResponse<List<CandidateAttachmentDto>>.FailureResponse("Candidate not found.", ["Candidate not found."]);
            }

            var attachments = await _attachmentRepository.GetByCandidateIdAsync(candidateId);
            var userNames = await GetUserNameLookupAsync();

            return ApiResponse<List<CandidateAttachmentDto>>.SuccessResponse(
                attachments.Select(a => MapToDto(a, userNames)).ToList());
        }

        public async Task<ApiResponse<CandidateAttachmentDto>> UploadAsync(
            Guid candidateId,
            string fileName,
            string contentType,
            long fileSizeBytes,
            Stream content,
            Guid uploadedByUserId)
        {
            if (!await _candidateRepository.ExistsAsync(candidateId))
            {
                return ApiResponse<CandidateAttachmentDto>.FailureResponse("Candidate not found.", ["Candidate not found."]);
            }

            if (fileSizeBytes <= 0)
            {
                return ApiResponse<CandidateAttachmentDto>.FailureResponse("The selected file is empty.", ["The selected file is empty."]);
            }

            if (fileSizeBytes > _settings.MaxFileSizeBytes)
            {
                var maxMb = _settings.MaxFileSizeBytes / (1024 * 1024);
                return ApiResponse<CandidateAttachmentDto>.FailureResponse(
                    $"File is too large. Maximum allowed size is {maxMb} MB.",
                    [$"File is too large. Maximum allowed size is {maxMb} MB."]);
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_settings.AllowedExtensions.Contains(extension))
            {
                var allowed = string.Join(", ", _settings.AllowedExtensions);
                return ApiResponse<CandidateAttachmentDto>.FailureResponse(
                    $"File type not allowed. Allowed types: {allowed}",
                    [$"File type not allowed. Allowed types: {allowed}"]);
            }

            var storedPath = await _fileStorageService.SaveAsync(candidateId, fileName, content);

            var attachment = new CandidateAttachment
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                FileName = fileName,
                StoredPath = storedPath,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                FileSizeBytes = fileSizeBytes,
                UploadedByUserId = uploadedByUserId,
                UploadedAtUtc = DateTime.UtcNow,
            };

            await _attachmentRepository.AddAsync(attachment);

            var userNames = await GetUserNameLookupAsync();
            return ApiResponse<CandidateAttachmentDto>.SuccessResponse(MapToDto(attachment, userNames), "Attachment uploaded.");
        }

        public async Task<CandidateAttachmentDownload?> GetForDownloadAsync(Guid attachmentId)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
            if (attachment is null)
            {
                return null;
            }

            var stream = await _fileStorageService.OpenReadAsync(attachment.StoredPath);
            if (stream is null)
            {
                return null;
            }

            return new CandidateAttachmentDownload
            {
                FileName = attachment.FileName,
                ContentType = string.IsNullOrWhiteSpace(attachment.ContentType) ? "application/octet-stream" : attachment.ContentType,
                Content = stream,
            };
        }

        public async Task<ApiResponse<object>> DeleteAsync(Guid attachmentId)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
            if (attachment is null)
            {
                return ApiResponse<object>.FailureResponse("Attachment not found.", ["Attachment not found."]);
            }

            await _attachmentRepository.DeleteAsync(attachment);
            _fileStorageService.Delete(attachment.StoredPath);

            return ApiResponse<object>.SuccessResponse(new { }, "Attachment deleted.");
        }

        private async Task<Dictionary<Guid, string>> GetUserNameLookupAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim());
        }

        private static CandidateAttachmentDto MapToDto(CandidateAttachment attachment, Dictionary<Guid, string> userNames)
            => new()
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                FileSizeBytes = attachment.FileSizeBytes,
                UploadedByName = userNames.GetValueOrDefault(attachment.UploadedByUserId),
                UploadedAtUtc = attachment.UploadedAtUtc,
            };
    }
}
