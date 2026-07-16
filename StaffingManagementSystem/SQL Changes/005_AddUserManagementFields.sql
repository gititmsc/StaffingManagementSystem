/*
====================================================================
 Script     : 005_AddUserManagementFields.sql
 Purpose    : Adds the columns needed for the User & Role Management
              module (Phone, Department, soft-delete flag) to dbo.Users.
 Matches    : StaffingManagementSystem.Core.Entities.User
              StaffingManagementSystem.Infrastructure.Persistence.Configurations.UserConfiguration
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'PhoneNumber')
BEGIN
    ALTER TABLE dbo.Users ADD PhoneNumber NVARCHAR(30) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'Department')
BEGIN
    ALTER TABLE dbo.Users ADD Department NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'IsDeleted')
BEGIN
    ALTER TABLE dbo.Users ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT (0);
END
GO
