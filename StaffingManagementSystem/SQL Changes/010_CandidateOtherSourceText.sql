/*
====================================================================
 Script     : 010_CandidateOtherSourceText.sql
 Purpose    : Adds dbo.Candidates.OtherSourceText — free-text capture
              for the "Specify Other Source" field shown when a
              candidate's Source is set to 'Other'.
 Matches    : StaffingManagementSystem.Core.Entities.Candidate.OtherSourceText
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'OtherSourceText')
BEGIN
    ALTER TABLE dbo.Candidates ADD OtherSourceText NVARCHAR(200) NULL;
END
GO
