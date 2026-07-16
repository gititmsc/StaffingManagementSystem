/*
====================================================================
 Script     : 001_CreateUsersTable.sql
 Purpose    : Creates the dbo.Users table backing authentication for
              the Staffing Management System.
 Matches    : StaffingManagementSystem.Core.Entities.User
              StaffingManagementSystem.Infrastructure.Persistence.Configurations.UserConfiguration
====================================================================
*/

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'StaffingManagementSystemDb')
BEGIN
    CREATE DATABASE [StaffingManagementSystemDb];
END
GO

USE [StaffingManagementSystemDb];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'Users' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.Users
    (
        Id              UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT DF_Users_Id DEFAULT NEWID(),
        FirstName       NVARCHAR(100)    NOT NULL,
        LastName        NVARCHAR(100)    NOT NULL,
        Email           NVARCHAR(256)    NOT NULL,
        PasswordHash    NVARCHAR(512)    NOT NULL,
        Role            NVARCHAR(50)     NOT NULL,
        IsActive        BIT              NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedAtUtc    DATETIME2        NOT NULL CONSTRAINT DF_Users_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc    DATETIME2        NULL,
        LastLoginAtUtc  DATETIME2        NULL,

        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'Recruiter', N'HiringManager', N'Interviewer'))
    );

    CREATE UNIQUE INDEX UX_Users_Email ON dbo.Users (Email);
END
GO
