/*
====================================================================
 Script     : 012_CandidateCostFields.sql
 Purpose    : Adds the three cost/compensation fields to dbo.Candidates:
                  CostToCompany  - visible/editable to Admin only
                  CostToVendor   - visible to everyone, editable by Admin/Recruiter
                  CurrentSalary  - visible/editable to Admin and Recruiter only
              Role-based visibility is enforced by the API (CandidateService),
              not by the database.
 Matches    : StaffingManagementSystem.Core.Entities.Candidate
              StaffingManagementSystem.Infrastructure.Persistence.Configurations.CandidateConfiguration
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'CostToCompany')
BEGIN
    ALTER TABLE dbo.Candidates ADD CostToCompany DECIMAL(12,2) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'CostToVendor')
BEGIN
    ALTER TABLE dbo.Candidates ADD CostToVendor DECIMAL(12,2) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Candidates') AND name = N'CurrentSalary')
BEGIN
    ALTER TABLE dbo.Candidates ADD CurrentSalary DECIMAL(12,2) NULL;
END
GO
