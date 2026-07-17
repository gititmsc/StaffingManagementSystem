/*
====================================================================
 Script     : 007_SeedSampleCandidates.sql
 Purpose    : Sample candidate profiles for testing the Candidate
              Master module (list/search, view, edit, skills,
              experience, education, projects, notes) and to give the
              upcoming Search & Reports module (skill-wise,
              experience-wise, company-wise) realistic data to query.

 Notes      : - Idempotent: each candidate insert is guarded by an
                IF NOT EXISTS check on Email, so re-running this script
                is safe and will not create duplicates.
              - OwnerRecruiterId is resolved by email against the
                accounts seeded in 002_SeedUsers.sql / 003_UpdateUserRoles.sql
                (recruiter@itmusketeers.com, hradmin@itmusketeers.com).
                Run this script AFTER those.
              - Skill names are de-duplicated into dbo.SkillMaster first,
                then referenced by name for each candidate's skills.
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

-- ---------------------------------------------------------------
-- 1. Make sure every skill used below exists in SkillMaster.
-- ---------------------------------------------------------------
DECLARE @Skills TABLE (Name NVARCHAR(150));
INSERT INTO @Skills (Name) VALUES
    (N'C#'), (N'ASP.NET Core'), (N'React'), (N'SQL Server'), (N'TypeScript'),
    (N'Java'), (N'Spring Boot'), (N'AWS'), (N'Microservices'),
    (N'Python'), (N'SQL'), (N'Power BI'), (N'Data Analysis'),
    (N'Azure'), (N'Docker'), (N'Kubernetes'), (N'CI/CD'),
    (N'Selenium'), (N'Manual Testing'), (N'Test Automation'),
    (N'.NET Core'), (N'Angular'), (N'Node.js');

INSERT INTO dbo.SkillMaster (Id, Name)
SELECT NEWID(), s.Name
FROM @Skills s
WHERE NOT EXISTS (SELECT 1 FROM dbo.SkillMaster sm WHERE sm.Name = s.Name);
GO

-- ---------------------------------------------------------------
-- 2. Candidate 1 — Ananya Verma (Full Stack .NET/React Developer)
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Candidates WHERE Email = N'ananya.verma@sample.itmusketeers.com')
BEGIN
    DECLARE @C1 UNIQUEIDENTIFIER = NEWID();
    DECLARE @C1Owner UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Users WHERE Email = N'recruiter@itmusketeers.com');
    DECLARE @C1Exp1 UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.Candidates
        (Id, FullName, Email, Phone, Address, CurrentLocation, DateOfBirth, Gender, Status, Source, OwnerRecruiterId, TotalExperienceYears, CreatedAtUtc)
    VALUES
        (@C1, N'Ananya Verma', N'ananya.verma@sample.itmusketeers.com', N'+91-98765-43210', N'201, Lotus Residency, Bandra', N'Mumbai, India', N'1996-03-14', N'Female', N'Available', N'LinkedIn', @C1Owner, 5.5, SYSUTCDATETIME());

    INSERT INTO dbo.CandidateSkills (Id, CandidateId, SkillId, Proficiency, YearsOfExperience)
    SELECT NEWID(), @C1, sm.Id, v.Proficiency, v.Years
    FROM (VALUES (N'C#', N'Expert', 5.5), (N'ASP.NET Core', N'Expert', 5.0), (N'React', N'Intermediate', 3.0), (N'SQL Server', N'Expert', 5.5), (N'TypeScript', N'Intermediate', 2.5)) AS v(Name, Proficiency, Years)
    JOIN dbo.SkillMaster sm ON sm.Name = v.Name;

    INSERT INTO dbo.CandidateExperience (Id, CandidateId, CompanyName, JobTitle, EmploymentType, StartDate, EndDate, IsCurrent, Location, Description)
    VALUES
        (@C1Exp1, @C1, N'Infosys Ltd.', N'Senior Software Engineer', N'FullTime', N'2022-06-01', NULL, 1, N'Mumbai, India', N'Leading a 4-person squad building an ASP.NET Core + React recruitment portal.'),
        (NEWID(), @C1, N'TCS', N'Software Engineer', N'FullTime', N'2019-07-01', N'2022-05-20', 0, N'Pune, India', N'Built internal HR tooling in C# and SQL Server.');

    INSERT INTO dbo.CandidateEducation (Id, CandidateId, Degree, Institution, FieldOfStudy, StartYear, EndYear, IsExpected, Grade)
    VALUES (NEWID(), @C1, N'B.E.', N'University of Mumbai', N'Computer Engineering', 2015, 2019, 0, N'8.4 CGPA');

    INSERT INTO dbo.CandidateProjects (Id, CandidateId, ExperienceId, ProjectName, Role, DurationText, TechnologiesUsed, Description)
    VALUES (NEWID(), @C1, @C1Exp1, N'Recruitment Management System', N'Tech Lead', N'Jun 2022 - Present', N'ASP.NET Core, React, SQL Server, JWT', N'Modular recruitment platform covering candidate master, user management and reporting.');

    INSERT INTO dbo.CandidateNotes (Id, CandidateId, Note, CreatedByUserId, CreatedAtUtc)
    VALUES (NEWID(), @C1, N'Strong communicator, available to join within 30 days. Shortlist for senior .NET roles.', @C1Owner, SYSUTCDATETIME());
END
GO

-- ---------------------------------------------------------------
-- 3. Candidate 2 — Rohit Malhotra (Java Backend Developer)
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Candidates WHERE Email = N'rohit.malhotra@sample.itmusketeers.com')
BEGIN
    DECLARE @C2 UNIQUEIDENTIFIER = NEWID();
    DECLARE @C2Owner UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Users WHERE Email = N'hradmin@itmusketeers.com');

    INSERT INTO dbo.Candidates
        (Id, FullName, Email, Phone, Address, CurrentLocation, DateOfBirth, Gender, Status, Source, OwnerRecruiterId, TotalExperienceYears, CreatedAtUtc)
    VALUES
        (@C2, N'Rohit Malhotra', N'rohit.malhotra@sample.itmusketeers.com', N'+91-99887-66554', N'45 Sector 21', N'Gurugram, India', N'1993-11-02', N'Male', N'InProcess', N'JobPortal', @C2Owner, 7.0, SYSUTCDATETIME());

    INSERT INTO dbo.CandidateSkills (Id, CandidateId, SkillId, Proficiency, YearsOfExperience)
    SELECT NEWID(), @C2, sm.Id, v.Proficiency, v.Years
    FROM (VALUES (N'Java', N'Expert', 7.0), (N'Spring Boot', N'Expert', 5.0), (N'AWS', N'Intermediate', 3.0), (N'Microservices', N'Expert', 4.0), (N'SQL', N'Intermediate', 6.0)) AS v(Name, Proficiency, Years)
    JOIN dbo.SkillMaster sm ON sm.Name = v.Name;

    INSERT INTO dbo.CandidateExperience (Id, CandidateId, CompanyName, JobTitle, EmploymentType, StartDate, EndDate, IsCurrent, Location, Description)
    VALUES (NEWID(), @C2, N'Wipro Technologies', N'Lead Java Developer', N'FullTime', N'2018-01-15', NULL, 1, N'Gurugram, India', N'Owns backend microservices for a fintech client, deployed on AWS.');

    INSERT INTO dbo.CandidateEducation (Id, CandidateId, Degree, Institution, FieldOfStudy, StartYear, EndYear, IsExpected, Grade)
    VALUES (NEWID(), @C2, N'B.Tech', N'Delhi Technological University', N'Information Technology', 2012, 2016, 0, N'8.0 CGPA');

    INSERT INTO dbo.CandidateNotes (Id, CandidateId, Note, CreatedByUserId, CreatedAtUtc)
    VALUES (NEWID(), @C2, N'In final round with a client for a Lead Developer role. Follow up next week.', @C2Owner, SYSUTCDATETIME());
END
GO

-- ---------------------------------------------------------------
-- 4. Candidate 3 — Sneha Kapoor (Data Analyst)
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Candidates WHERE Email = N'sneha.kapoor@sample.itmusketeers.com')
BEGIN
    DECLARE @C3 UNIQUEIDENTIFIER = NEWID();
    DECLARE @C3Owner UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Users WHERE Email = N'recruiter@itmusketeers.com');

    INSERT INTO dbo.Candidates
        (Id, FullName, Email, Phone, Address, CurrentLocation, DateOfBirth, Gender, Status, Source, OwnerRecruiterId, TotalExperienceYears, CreatedAtUtc)
    VALUES
        (@C3, N'Sneha Kapoor', N'sneha.kapoor@sample.itmusketeers.com', N'+91-97654-32109', N'12 MG Road', N'Bengaluru, India', N'1998-07-22', N'Female', N'New', N'Referral', @C3Owner, 2.5, SYSUTCDATETIME());

    INSERT INTO dbo.CandidateSkills (Id, CandidateId, SkillId, Proficiency, YearsOfExperience)
    SELECT NEWID(), @C3, sm.Id, v.Proficiency, v.Years
    FROM (VALUES (N'Python', N'Intermediate', 2.5), (N'SQL', N'Expert', 2.5), (N'Power BI', N'Intermediate', 2.0), (N'Data Analysis', N'Expert', 2.5)) AS v(Name, Proficiency, Years)
    JOIN dbo.SkillMaster sm ON sm.Name = v.Name;

    INSERT INTO dbo.CandidateExperience (Id, CandidateId, CompanyName, JobTitle, EmploymentType, StartDate, EndDate, IsCurrent, Location, Description)
    VALUES
        (NEWID(), @C3, N'Accenture', N'Data Analyst', N'FullTime', N'2023-02-01', NULL, 1, N'Bengaluru, India', N'Builds Power BI dashboards for supply-chain reporting.'),
        (NEWID(), @C3, N'Capgemini', N'Business Analyst Intern', N'Internship', N'2022-06-01', N'2023-01-15', 0, N'Bengaluru, India', N'Assisted with data cleaning and SQL reporting for internal teams.');

    INSERT INTO dbo.CandidateEducation (Id, CandidateId, Degree, Institution, FieldOfStudy, StartYear, EndYear, IsExpected, Grade)
    VALUES (NEWID(), @C3, N'B.Sc.', N'Christ University', N'Statistics', 2019, 2022, 0, N'First Class');
END
GO

-- ---------------------------------------------------------------
-- 5. Candidate 4 — Vikram Nair (DevOps Engineer)
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Candidates WHERE Email = N'vikram.nair@sample.itmusketeers.com')
BEGIN
    DECLARE @C4 UNIQUEIDENTIFIER = NEWID();
    DECLARE @C4Owner UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Users WHERE Email = N'recruiter@itmusketeers.com');

    INSERT INTO dbo.Candidates
        (Id, FullName, Email, Phone, Address, CurrentLocation, DateOfBirth, Gender, Status, Source, OwnerRecruiterId, TotalExperienceYears, CreatedAtUtc)
    VALUES
        (@C4, N'Vikram Nair', N'vikram.nair@sample.itmusketeers.com', N'+91-96543-21098', N'88 Marine Drive', N'Kochi, India', N'1990-01-30', N'Male', N'Placed', N'Agency', @C4Owner, 8.0, SYSUTCDATETIME());

    INSERT INTO dbo.CandidateSkills (Id, CandidateId, SkillId, Proficiency, YearsOfExperience)
    SELECT NEWID(), @C4, sm.Id, v.Proficiency, v.Years
    FROM (VALUES (N'Azure', N'Expert', 6.0), (N'Docker', N'Expert', 5.0), (N'Kubernetes', N'Expert', 4.0), (N'CI/CD', N'Expert', 6.0)) AS v(Name, Proficiency, Years)
    JOIN dbo.SkillMaster sm ON sm.Name = v.Name;

    INSERT INTO dbo.CandidateExperience (Id, CandidateId, CompanyName, JobTitle, EmploymentType, StartDate, EndDate, IsCurrent, Location, Description)
    VALUES (NEWID(), @C4, N'Cognizant', N'DevOps Lead', N'FullTime', N'2016-04-01', N'2024-12-31', 0, N'Kochi, India', N'Owned CI/CD pipelines and Kubernetes clusters for a healthcare client.');

    INSERT INTO dbo.CandidateEducation (Id, CandidateId, Degree, Institution, FieldOfStudy, StartYear, EndYear, IsExpected, Grade)
    VALUES (NEWID(), @C4, N'B.Tech', N'NIT Calicut', N'Computer Science', 2008, 2012, 0, N'7.9 CGPA');

    INSERT INTO dbo.CandidateNotes (Id, CandidateId, Note, CreatedByUserId, CreatedAtUtc)
    VALUES (NEWID(), @C4, N'Placed with client on 2025-01-06. Keep profile for future DevOps openings.', @C4Owner, SYSUTCDATETIME());
END
GO

-- ---------------------------------------------------------------
-- 6. Candidate 5 — Farah Sheikh (QA Engineer)
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Candidates WHERE Email = N'farah.sheikh@sample.itmusketeers.com')
BEGIN
    DECLARE @C5 UNIQUEIDENTIFIER = NEWID();
    DECLARE @C5Owner UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Users WHERE Email = N'hradmin@itmusketeers.com');

    INSERT INTO dbo.Candidates
        (Id, FullName, Email, Phone, Address, CurrentLocation, DateOfBirth, Gender, Status, Source, OwnerRecruiterId, TotalExperienceYears, CreatedAtUtc)
    VALUES
        (@C5, N'Farah Sheikh', N'farah.sheikh@sample.itmusketeers.com', N'+91-95432-10987', N'7 Residency Road', N'Hyderabad, India', N'1995-09-18', N'Female', N'OnHold', N'WalkIn', @C5Owner, 4.0, SYSUTCDATETIME());

    INSERT INTO dbo.CandidateSkills (Id, CandidateId, SkillId, Proficiency, YearsOfExperience)
    SELECT NEWID(), @C5, sm.Id, v.Proficiency, v.Years
    FROM (VALUES (N'Selenium', N'Expert', 4.0), (N'Manual Testing', N'Expert', 4.0), (N'Test Automation', N'Intermediate', 3.0), (N'Java', N'Intermediate', 3.0)) AS v(Name, Proficiency, Years)
    JOIN dbo.SkillMaster sm ON sm.Name = v.Name;

    INSERT INTO dbo.CandidateExperience (Id, CandidateId, CompanyName, JobTitle, EmploymentType, StartDate, EndDate, IsCurrent, Location, Description)
    VALUES (NEWID(), @C5, N'Tech Mahindra', N'QA Engineer', N'FullTime', N'2020-08-01', NULL, 1, N'Hyderabad, India', N'Maintains a Selenium + Java automation suite for a banking client.');

    INSERT INTO dbo.CandidateEducation (Id, CandidateId, Degree, Institution, FieldOfStudy, StartYear, EndYear, IsExpected, Grade)
    VALUES (NEWID(), @C5, N'B.E.', N'Osmania University', N'Electronics & Communication', 2016, 2020, 0, N'7.5 CGPA');

    INSERT INTO dbo.CandidateNotes (Id, CandidateId, Note, CreatedByUserId, CreatedAtUtc)
    VALUES (NEWID(), @C5, N'Candidate requested to pause the process for 2 months due to personal reasons.', @C5Owner, SYSUTCDATETIME());
END
GO

-- ---------------------------------------------------------------
-- 7. Candidate 6 — Karan Bhatia (.NET Developer, Blacklisted example)
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Candidates WHERE Email = N'karan.bhatia@sample.itmusketeers.com')
BEGIN
    DECLARE @C6 UNIQUEIDENTIFIER = NEWID();
    DECLARE @C6Owner UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Users WHERE Email = N'recruiter@itmusketeers.com');

    INSERT INTO dbo.Candidates
        (Id, FullName, Email, Phone, Address, CurrentLocation, DateOfBirth, Gender, Status, Source, OwnerRecruiterId, TotalExperienceYears, CreatedAtUtc)
    VALUES
        (@C6, N'Karan Bhatia', N'karan.bhatia@sample.itmusketeers.com', N'+91-94321-09876', N'15 Civil Lines', N'Jaipur, India', N'1992-05-09', N'Male', N'Blacklisted', N'Website', @C6Owner, 6.0, SYSUTCDATETIME());

    INSERT INTO dbo.CandidateSkills (Id, CandidateId, SkillId, Proficiency, YearsOfExperience)
    SELECT NEWID(), @C6, sm.Id, v.Proficiency, v.Years
    FROM (VALUES (N'C#', N'Expert', 6.0), (N'.NET Core', N'Expert', 5.0), (N'Azure', N'Intermediate', 3.0)) AS v(Name, Proficiency, Years)
    JOIN dbo.SkillMaster sm ON sm.Name = v.Name;

    INSERT INTO dbo.CandidateExperience (Id, CandidateId, CompanyName, JobTitle, EmploymentType, StartDate, EndDate, IsCurrent, Location, Description)
    VALUES (NEWID(), @C6, N'Infosys Ltd.', N'.NET Developer', N'FullTime', N'2017-03-01', N'2023-09-30', 0, N'Jaipur, India', N'Built internal billing applications in C# and .NET Core.');

    INSERT INTO dbo.CandidateEducation (Id, CandidateId, Degree, Institution, FieldOfStudy, StartYear, EndYear, IsExpected, Grade)
    VALUES (NEWID(), @C6, N'B.C.A.', N'University of Rajasthan', N'Computer Applications', 2013, 2016, 0, N'7.2 CGPA');

    INSERT INTO dbo.CandidateNotes (Id, CandidateId, Note, CreatedByUserId, CreatedAtUtc)
    VALUES (NEWID(), @C6, N'No-showed twice for scheduled interviews without notice. Blacklisted per policy.', @C6Owner, SYSUTCDATETIME());
END
GO
