namespace StaffingManagementSystem.Core.Enums
{
    /// <summary>
    /// Roles supported by the Staffing Management System.
    /// Mirrors the role model defined in the RMS Requirements Specification (Section 2.2 / Section 7).
    /// </summary>
    public enum UserRole
    {
        /// <summary>Full system access: user/role administration, candidates and reports.</summary>
        Admin = 1,

        /// <summary>Full candidate CRUD and report access; no user/role administration.</summary>
        Recruiter = 2,

        /// <summary>
        /// View-only access to the candidate list/detail pages. Cannot access reports or
        /// attachment/resume downloads. Name, Email, LinkedIn URL and Phone are masked
        /// ("XXXX") in every response served to this role — see CandidateService masking.
        /// </summary>
        Viewer = 3
    }
}
