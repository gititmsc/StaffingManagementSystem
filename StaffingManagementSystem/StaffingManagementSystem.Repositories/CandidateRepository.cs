using Microsoft.EntityFrameworkCore;
using StaffingManagementSystem.Core.Entities;
using StaffingManagementSystem.Infrastructure.Persistence;
using StaffingManagementSystem.Repositories.Interfaces;

namespace StaffingManagementSystem.Repositories
{
    /// <inheritdoc cref="ICandidateRepository"/>
    public class CandidateRepository : ICandidateRepository
    {
        private readonly AppDbContext _dbContext;

        public CandidateRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<Candidate>> GetAllAsync()
            => _dbContext.Candidates
                .Where(c => !c.IsDeleted)
                .Include(c => c.Skills).ThenInclude(s => s.Skill)
                .Include(c => c.Experience)
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToListAsync();

        public Task<Candidate?> GetByIdAsync(Guid id)
            => _dbContext.Candidates
                .Where(c => c.Id == id && !c.IsDeleted)
                .Include(c => c.Skills).ThenInclude(s => s.Skill)
                .Include(c => c.Experience)
                .Include(c => c.Education)
                .Include(c => c.Projects)
                .Include(c => c.Notes)
                .FirstOrDefaultAsync();

        public Task<bool> EmailExistsAsync(string email, Guid? excludeCandidateId = null)
            => _dbContext.Candidates.AnyAsync(c =>
                !c.IsDeleted &&
                c.Email == email &&
                (excludeCandidateId == null || c.Id != excludeCandidateId));

        public async Task<Guid> GetOrCreateSkillIdAsync(string skillName)
        {
            var normalized = skillName.Trim();

            var existing = await _dbContext.SkillMaster
                .FirstOrDefaultAsync(s => s.Name.ToLower() == normalized.ToLower());

            if (existing is not null)
            {
                return existing.Id;
            }

            var skill = new SkillMaster { Id = Guid.NewGuid(), Name = normalized };

            try
            {
                await _dbContext.SkillMaster.AddAsync(skill);
                await _dbContext.SaveChangesAsync();
                return skill.Id;
            }
            catch (DbUpdateException)
            {
                // Lost a race to create the same skill concurrently — use the row that won.
                _dbContext.Entry(skill).State = EntityState.Detached;
                var winner = await _dbContext.SkillMaster.FirstAsync(s => s.Name.ToLower() == normalized.ToLower());
                return winner.Id;
            }
        }

        public async Task CreateAsync(Candidate candidate)
        {
            await _dbContext.Candidates.AddAsync(candidate);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(
            Candidate candidate,
            List<CandidateSkill> skills,
            List<CandidateExperience> experience,
            List<CandidateEducation> education,
            List<CandidateProject> projects)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            await _dbContext.CandidateSkills.Where(s => s.CandidateId == candidate.Id).ExecuteDeleteAsync();
            await _dbContext.CandidateExperience.Where(e => e.CandidateId == candidate.Id).ExecuteDeleteAsync();
            await _dbContext.CandidateEducation.Where(e => e.CandidateId == candidate.Id).ExecuteDeleteAsync();
            await _dbContext.CandidateProjects.Where(p => p.CandidateId == candidate.Id).ExecuteDeleteAsync();

            await _dbContext.Candidates
                .Where(c => c.Id == candidate.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.FullName, candidate.FullName)
                    .SetProperty(c => c.Email, candidate.Email)
                    .SetProperty(c => c.Phone, candidate.Phone)
                    .SetProperty(c => c.Address, candidate.Address)
                    .SetProperty(c => c.CurrentLocation, candidate.CurrentLocation)
                    .SetProperty(c => c.DateOfBirth, candidate.DateOfBirth)
                    .SetProperty(c => c.Gender, candidate.Gender)
                    .SetProperty(c => c.Status, candidate.Status)
                    .SetProperty(c => c.Source, candidate.Source)
                    .SetProperty(c => c.OwnerRecruiterId, candidate.OwnerRecruiterId)
                    .SetProperty(c => c.TotalExperienceYears, candidate.TotalExperienceYears)
                    .SetProperty(c => c.UpdatedAtUtc, DateTime.UtcNow));

            if (skills.Count > 0)
            {
                await _dbContext.CandidateSkills.AddRangeAsync(skills);
            }

            if (experience.Count > 0)
            {
                await _dbContext.CandidateExperience.AddRangeAsync(experience);
            }

            if (education.Count > 0)
            {
                await _dbContext.CandidateEducation.AddRangeAsync(education);
            }

            if (projects.Count > 0)
            {
                await _dbContext.CandidateProjects.AddRangeAsync(projects);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            await _dbContext.Candidates
                .Where(c => c.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.IsDeleted, true)
                    .SetProperty(c => c.UpdatedAtUtc, DateTime.UtcNow));
        }

        public async Task AddNoteAsync(CandidateNote note)
        {
            await _dbContext.CandidateNotes.AddAsync(note);
            await _dbContext.SaveChangesAsync();
        }
    }
}
