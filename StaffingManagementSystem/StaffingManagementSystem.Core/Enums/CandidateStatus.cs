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
        Blacklisted = 6
    }
}
