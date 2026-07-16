using StaffingManagementSystem.Core.Enums;

namespace StaffingManagementSystem.Core.Entities
{
    /// <summary>A skill entry on a candidate profile (RMS SRS Appendix A.2).</summary>
    public class CandidateSkill
    {
        public Guid Id { get; set; }

        public Guid CandidateId { get; set; }

        public Guid SkillId { get; set; }

        /// <summary>Navigation to the shared skill lookup row — populated via Include for display.</summary>
        public SkillMaster? Skill { get; set; }

        public ProficiencyLevel? Proficiency { get; set; }

        public decimal? YearsOfExperience { get; set; }
    }
}
