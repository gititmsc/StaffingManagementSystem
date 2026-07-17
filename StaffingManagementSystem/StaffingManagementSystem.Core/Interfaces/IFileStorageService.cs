namespace StaffingManagementSystem.Core.Interfaces
{
    /// <summary>
    /// Stores and retrieves candidate attachment files. Implemented in the Infrastructure
    /// layer (local disk on the API server for Phase 1).
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves <paramref name="content"/> under a folder scoped to <paramref name="candidateId"/>,
        /// using a generated, collision-proof file name that preserves the original extension.
        /// Returns the path (relative to the configured storage root) where the file was written.
        /// </summary>
        Task<string> SaveAsync(Guid candidateId, string originalFileName, Stream content, CancellationToken cancellationToken = default);

        /// <summary>Opens a previously saved file for reading, or null if it no longer exists on disk.</summary>
        Task<Stream?> OpenReadAsync(string storedPath, CancellationToken cancellationToken = default);

        /// <summary>Deletes a previously saved file. No-ops silently if it no longer exists.</summary>
        void Delete(string storedPath);
    }
}
