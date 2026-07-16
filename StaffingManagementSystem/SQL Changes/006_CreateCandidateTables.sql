/*
====================================================================
 Script     : 006_CreateCandidateTables.sql
 Purpose    : Creates the Candidate Master tables (RMS Requirements
              Specification, Section 3.3 / Appendix A): the core
              candidate profile plus its skills, experience, education,
              project and note child records.
 Matches    : StaffingManagementSystem.Core.Entities.Candidate and
              related entities; StaffingManagementSystem.Infrastructure
              .Persistence.Configurations.Candidate*Configuration
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

-- ---------------------------------------------------------------
-- dbo.Candidates
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'Candidates' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.Candidates
    (
        Id                     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Candidates_Id DEFAULT NEWID(),
        FullName               NVARCHAR(200)    NOT NULL,
        Email                  NVARCHAR(256)    NOT NULL,
        Phone                  NVARCHAR(30)     NULL,
        Address                NVARCHAR(500)    NULL,
        CurrentLocation        NVARCHAR(200)    NULL,
        DateOfBirth            DATETIME2        NULL,
        Gender                 NVARCHAR(30)     NULL,
        Status                 NVARCHAR(30)     NOT NULL,
        Source                 NVARCHAR(30)     NULL,
        OwnerRecruiterId       UNIQUEIDENTIFIER NOT NULL,
        TotalExperienceYears   DECIMAL(5,2)     NOT NULL CONSTRAINT DF_Candidates_TotalExperienceYears DEFAULT (0),
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_Candidates_IsDeleted DEFAULT (0),
        CreatedAtUtc           DATETIME2        NOT NULL CONSTRAINT DF_Candidates_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc           DATETIME2        NULL,

        CONSTRAINT PK_Candidates PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Candidates_Users_OwnerRecruiterId FOREIGN KEY (OwnerRecruiterId)
            REFERENCES dbo.Users (Id)
    );

    CREATE INDEX IX_Candidates_Email ON dbo.Candidates (Email);
    CREATE INDEX IX_Candidates_Phone ON dbo.Candidates (Phone);
    CREATE INDEX IX_Candidates_Status ON dbo.Candidates (Status);
END
GO

-- ---------------------------------------------------------------
-- dbo.SkillMaster
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'SkillMaster' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.SkillMaster
    (
        Id       UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SkillMaster_Id DEFAULT NEWID(),
        Name     NVARCHAR(150)    NOT NULL,
        Category NVARCHAR(100)    NULL,

        CONSTRAINT PK_SkillMaster PRIMARY KEY CLUSTERED (Id)
    );

    CREATE UNIQUE INDEX UX_SkillMaster_Name ON dbo.SkillMaster (Name);
END
GO

-- ---------------------------------------------------------------
-- dbo.CandidateSkills
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'CandidateSkills' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.CandidateSkills
    (
        Id                UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CandidateSkills_Id DEFAULT NEWID(),
        CandidateId       UNIQUEIDENTIFIER NOT NULL,
        SkillId           UNIQUEIDENTIFIER NOT NULL,
        Proficiency       NVARCHAR(30)     NULL,
        YearsOfExperience DECIMAL(4,1)     NULL,

        CONSTRAINT PK_CandidateSkills PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_CandidateSkills_Candidates FOREIGN KEY (CandidateId)
            REFERENCES dbo.Candidates (Id) ON DELETE CASCADE,
        CONSTRAINT FK_CandidateSkills_SkillMaster FOREIGN KEY (SkillId)
            REFERENCES dbo.SkillMaster (Id)
    );

    CREATE INDEX IX_CandidateSkills_CandidateId ON dbo.CandidateSkills (CandidateId);
    CREATE INDEX IX_CandidateSkills_SkillId ON dbo.CandidateSkills (SkillId);
END
GO

-- ---------------------------------------------------------------
-- dbo.CandidateExperience
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'CandidateExperience' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.CandidateExperience
    (
        Id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CandidateExperience_Id DEFAULT NEWID(),
        CandidateId    UNIQUEIDENTIFIER NOT NULL,
        CompanyName    NVARCHAR(200)    NOT NULL,
        JobTitle       NVARCHAR(150)    NOT NULL,
        EmploymentType NVARCHAR(30)     NULL,
        StartDate      DATETIME2        NOT NULL,
        EndDate        DATETIME2        NULL,
        IsCurrent      BIT              NOT NULL CONSTRAINT DF_CandidateExperience_IsCurrent DEFAULT (0),
        Location       NVARCHAR(200)    NULL,
        Description    NVARCHAR(2000)   NULL,

        CONSTRAINT PK_CandidateExperience PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_CandidateExperience_Candidates FOREIGN KEY (CandidateId)
            REFERENCES dbo.Candidates (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_CandidateExperience_CandidateId ON dbo.CandidateExperience (CandidateId);
END
GO

-- ---------------------------------------------------------------
-- dbo.CandidateProjects
-- Note: ExperienceId is intentionally NOT a foreign key — a real FK back to
-- CandidateExperience would create a second cascade path to Candidates, which
-- SQL Server disallows (matches the EF Core mapping, which also leaves it unconstrained).
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'CandidateProjects' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.CandidateProjects
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CandidateProjects_Id DEFAULT NEWID(),
        CandidateId      UNIQUEIDENTIFIER NOT NULL,
        ExperienceId     UNIQUEIDENTIFIER NULL,
        ProjectName      NVARCHAR(200)    NOT NULL,
        Role             NVARCHAR(150)    NULL,
        DurationText     NVARCHAR(100)    NULL,
        TechnologiesUsed NVARCHAR(500)    NULL,
        Description      NVARCHAR(2000)   NULL,

        CONSTRAINT PK_CandidateProjects PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_CandidateProjects_Candidates FOREIGN KEY (CandidateId)
            REFERENCES dbo.Candidates (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_CandidateProjects_CandidateId ON dbo.CandidateProjects (CandidateId);
    CREATE INDEX IX_CandidateProjects_ExperienceId ON dbo.CandidateProjects (ExperienceId);
END
GO

-- ---------------------------------------------------------------
-- dbo.CandidateEducation
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'CandidateEducation' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.CandidateEducation
    (
        Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CandidateEducation_Id DEFAULT NEWID(),
        CandidateId   UNIQUEIDENTIFIER NOT NULL,
        Degree        NVARCHAR(150)    NOT NULL,
        Institution   NVARCHAR(200)    NOT NULL,
        FieldOfStudy  NVARCHAR(150)    NULL,
        StartYear     INT              NULL,
        EndYear       INT              NULL,
        IsExpected    BIT              NOT NULL CONSTRAINT DF_CandidateEducation_IsExpected DEFAULT (0),
        Grade         NVARCHAR(50)     NULL,

        CONSTRAINT PK_CandidateEducation PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_CandidateEducation_Candidates FOREIGN KEY (CandidateId)
            REFERENCES dbo.Candidates (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_CandidateEducation_CandidateId ON dbo.CandidateEducation (CandidateId);
END
GO

-- ---------------------------------------------------------------
-- dbo.CandidateNotes
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'CandidateNotes' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.CandidateNotes
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CandidateNotes_Id DEFAULT NEWID(),
        CandidateId     UNIQUEIDENTIFIER NOT NULL,
        Note            NVARCHAR(2000)   NOT NULL,
        CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
        CreatedAtUtc    DATETIME2        NOT NULL CONSTRAINT DF_CandidateNotes_CreatedAtUtc DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_CandidateNotes PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_CandidateNotes_Candidates FOREIGN KEY (CandidateId)
            REFERENCES dbo.Candidates (Id) ON DELETE CASCADE,
        CONSTRAINT FK_CandidateNotes_Users FOREIGN KEY (CreatedByUserId)
            REFERENCES dbo.Users (Id)
    );

    CREATE INDEX IX_CandidateNotes_CandidateId ON dbo.CandidateNotes (CandidateId);
END
GO
