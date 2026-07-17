/*
====================================================================
 Script     : 008_CreateCandidateAttachmentsTable.sql
 Purpose    : Creates the CandidateAttachments table (RMS Requirements
              Specification, Section 3.3.6 — resume/document
              attachments on a candidate profile). Files themselves are
              stored on local disk on the API server; this table tracks
              their metadata and relative storage path.
 Matches    : StaffingManagementSystem.Core.Entities.CandidateAttachment
              StaffingManagementSystem.Infrastructure.Persistence
              .Configurations.CandidateAttachmentConfiguration
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'CandidateAttachments' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.CandidateAttachments
    (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CandidateAttachments_Id DEFAULT NEWID(),
        CandidateId      UNIQUEIDENTIFIER NOT NULL,
        FileName         NVARCHAR(260)    NOT NULL,
        StoredPath       NVARCHAR(400)    NOT NULL,
        ContentType      NVARCHAR(150)    NOT NULL,
        FileSizeBytes    BIGINT           NOT NULL,
        UploadedByUserId UNIQUEIDENTIFIER NOT NULL,
        UploadedAtUtc    DATETIME2        NOT NULL CONSTRAINT DF_CandidateAttachments_UploadedAtUtc DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_CandidateAttachments PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_CandidateAttachments_Candidates FOREIGN KEY (CandidateId)
            REFERENCES dbo.Candidates (Id) ON DELETE CASCADE,
        CONSTRAINT FK_CandidateAttachments_Users FOREIGN KEY (UploadedByUserId)
            REFERENCES dbo.Users (Id)
    );

    CREATE INDEX IX_CandidateAttachments_CandidateId ON dbo.CandidateAttachments (CandidateId);
END
GO
