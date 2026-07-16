/*
====================================================================
 Script     : 004_CreatePasswordResetTokensTable.sql
 Purpose    : Creates the dbo.PasswordResetTokens table backing the
              "forgot password" flow for the Staffing Management System.
 Matches    : StaffingManagementSystem.Core.Entities.PasswordResetToken
              StaffingManagementSystem.Infrastructure.Persistence.Configurations.PasswordResetTokenConfiguration
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'PasswordResetTokens' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.PasswordResetTokens
    (
        Id              UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT DF_PasswordResetTokens_Id DEFAULT NEWID(),
        UserId          UNIQUEIDENTIFIER NOT NULL,
        TokenHash       NVARCHAR(128)    NOT NULL,
        ExpiresAtUtc    DATETIME2        NOT NULL,
        CreatedAtUtc    DATETIME2        NOT NULL
                            CONSTRAINT DF_PasswordResetTokens_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UsedAtUtc       DATETIME2        NULL,

        CONSTRAINT PK_PasswordResetTokens PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_PasswordResetTokens_TokenHash ON dbo.PasswordResetTokens (TokenHash);
    CREATE INDEX IX_PasswordResetTokens_UserId ON dbo.PasswordResetTokens (UserId);
END
GO
