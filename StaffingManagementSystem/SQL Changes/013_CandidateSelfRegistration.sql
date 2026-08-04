/*
====================================================================
 Script     : 013_CandidateSelfRegistration.sql
 Purpose    : Supports the Candidate Self-Registration module — a public,
              no-login registration form that creates candidates with
              Status = PendingApproval, plus the Admin approve/reject
              workflow (CandidateStatus gains PendingApproval/Approved/
              Rejected — string-converted, so no CHECK constraint change
              is needed since none exists on dbo.Candidates.Status).

              - OwnerRecruiterId becomes nullable: a self-registered
                candidate has no owning recruiter until one is assigned.
              - New approval-audit columns on dbo.Candidates.
              - CandidateAttachments.UploadedByUserId becomes nullable:
                a resume submitted through the public form has no
                authenticated uploader.
 Matches    : StaffingManagementSystem.Core.Entities.Candidate
              StaffingManagementSystem.Core.Entities.CandidateAttachment
              StaffingManagementSystem.Infrastructure.Persistence.Configurations.CandidateConfiguration
              StaffingManagementSystem.Infrastructure.Persistence.Configurations.CandidateAttachmentConfiguration
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'OwnerRecruiterId' AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.Candidates ALTER COLUMN OwnerRecruiterId UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'RejectionComment')
BEGIN
    ALTER TABLE dbo.Candidates ADD RejectionComment NVARCHAR(1000) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'ApprovedByUserId')
BEGIN
    ALTER TABLE dbo.Candidates ADD ApprovedByUserId UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'ApprovedAtUtc')
BEGIN
    ALTER TABLE dbo.Candidates ADD ApprovedAtUtc DATETIME2 NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'RejectedByUserId')
BEGIN
    ALTER TABLE dbo.Candidates ADD RejectedByUserId UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'RejectedAtUtc')
BEGIN
    ALTER TABLE dbo.Candidates ADD RejectedAtUtc DATETIME2 NULL;
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.CandidateAttachments') AND name = N'UploadedByUserId' AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.CandidateAttachments ALTER COLUMN UploadedByUserId UNIQUEIDENTIFIER NULL;
END
GO
