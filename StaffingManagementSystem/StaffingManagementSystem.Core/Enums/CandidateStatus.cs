namespace StaffingManagementSystem.Core.Enums
{
    /// <summary>Lifecycle status of a candidate record (RMS SRS Appendix A.1).</summary>
    public enum CandidateStatus
    {
        New = 1,
        Available = 2,
        InProcess = 3,
        Placed = 4,
        OnHold = 5,
        Blacklisted = 6,

        /// <summary>Submitted via the public self-registration form; hidden from Recruiter/Viewer until Approved.</summary>
        PendingApproval = 7,

        /// <summary>Approved by an Admin from PendingApproval — now visible/searchable for every role.</summary>
        Approved = 8,

        /// <summary>Rejected by an Admin from PendingApproval, with a mandatory rejection comment.</summary>
        Rejected = 9
    }
}
