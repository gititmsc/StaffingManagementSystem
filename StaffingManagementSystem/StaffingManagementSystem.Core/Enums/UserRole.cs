namespace StaffingManagementSystem.Core.Enums
{
    /// <summary>
    /// Roles supported by the Staffing Management System.
    /// Mirrors the role model defined in the RMS Requirements Specification (Section 2.2 / Section 7).
    /// </summary>
    public enum UserRole
    {
        /// <summary>Full system access, including user/role administration and system configuration.</summary>
        SuperAdmin = 1,

        /// <summary>Manages candidates, requisitions, reports and recruiters; no system configuration access.</summary>
        HRAdmin = 2,

        /// <summary>Adds/edits candidates, searches the database, manages assigned requisitions and pipeline stages.</summary>
        Recruiter = 3,

        /// <summary>Views candidates/pipeline for their own requisitions and gives interview feedback.</summary>
        HiringManager = 4,

        /// <summary>View-only access to candidate profiles and reports.</summary>
        Viewer = 5
    }
}
