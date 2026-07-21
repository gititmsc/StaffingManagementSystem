using StaffingManagementSystem.Core.Common;
using StaffingManagementSystem.Core.DTOs.Candidates;

namespace StaffingManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Business logic contract for candidate attachments (RMS SRS 3.3.6 — resumes and other
    /// candidate documents, stored on local disk on the API server for Phase 1).
    /// </summary>
    public interface ICandidateAttachmentService
    {
        Task<ApiResponse<List<CandidateAttachmentDto>>> GetByCandidateIdAsync(Guid candidateId);

        /// <summary>Adds a general (non-resume) document. Multiple are allowed per candidate.</summary>
        Task<ApiResponse<CandidateAttachmentDto>> UploadAsync(
            Guid candidateId,
            string fileName,
            string contentType,
            long fileSizeBytes,
            Stream content,
            Guid uploadedByUserId);

        /// <summary>
        /// Uploads a candidate's resume, replacing (deleting) any previous resume file/row so a
        /// candidate always has at most one active resume, kept separate from other attachments.
        /// </summary>
        Task<ApiResponse<CandidateAttachmentDto>> UploadResumeAsync(
            Guid candidateId,
            string fileName,
            string contentType,
            long fileSizeBytes,
            Stream content,
            Guid uploadedByUserId);

        /// <summary>Returns the file stream, content type and original file name for download, or null if not found.</summary>
        Task<CandidateAttachmentDownload?> GetForDownloadAsync(Guid attachmentId);

        Task<ApiResponse<object>> DeleteAsync(Guid attachmentId);
    }

    /// <summary>Result of resolving a candidate attachment for download.</summary>
    public class CandidateAttachmentDownload
    {
        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public Stream Content { get; set; } = Stream.Null;
    }
}
