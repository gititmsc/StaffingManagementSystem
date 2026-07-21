/*
====================================================================
 Script     : 009_CandidateProfileEnhancements.sql
 Purpose    : Adds fields requested after Candidate Master UAT:
              - dbo.Candidates: Title (professional headline) and
                LinkedInUrl.
              - dbo.CandidateAttachments: Type, so a candidate's resume
                can be tracked separately from other documents.
 Matches    : StaffingManagementSystem.Core.Entities.Candidate,
              StaffingManagementSystem.Core.Entities.CandidateAttachment,
              StaffingManagementSystem.Core.Enums.CandidateAttachmentType
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

-- ---------------------------------------------------------------
-- dbo.Candidates: Title, LinkedInUrl
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'Title')
BEGIN
    ALTER TABLE dbo.Candidates ADD Title NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'LinkedInUrl')
BEGIN
    ALTER TABLE dbo.Candidates ADD LinkedInUrl NVARCHAR(300) NULL;
END
GO

-- ---------------------------------------------------------------
-- dbo.CandidateAttachments: Type (Resume / Other)
-- ---------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.CandidateAttachments') AND name = N'Type')
BEGIN
    ALTER TABLE dbo.CandidateAttachments ADD Type NVARCHAR(20) NOT NULL CONSTRAINT DF_CandidateAttachments_Type DEFAULT (N'Other');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_CandidateAttachments_Type')
BEGIN
    ALTER TABLE dbo.CandidateAttachments
        ADD CONSTRAINT CK_CandidateAttachments_Type CHECK (Type IN (N'Resume', N'Other'));
END
GO

-- Only one active resume per candidate is expected at the application level (the API
-- replaces the previous resume row/file when a new one is uploaded), so no DB-level
-- uniqueness constraint is added here — it would reject legitimate historical data if a
-- resume was ever duplicated manually.
