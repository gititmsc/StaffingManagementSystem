/*
====================================================================
 Script     : 013_CreateRefreshTokensTable.sql
 Purpose    : Creates the dbo.RefreshTokens table backing silent
              session auto-extend — a client holds a long-lived
              refresh token and silently exchanges it for a new JWT
              access token before/after the short-lived access token
              expires, without the user having to sign in again.
 Matches    : StaffingManagementSystem.Core.Entities.RefreshToken
              StaffingManagementSystem.Infrastructure.Persistence.Configurations.RefreshTokenConfiguration
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'RefreshTokens' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        Id                  UNIQUEIDENTIFIER NOT NULL
                                CONSTRAINT DF_RefreshTokens_Id DEFAULT NEWID(),
        UserId              UNIQUEIDENTIFIER NOT NULL,
        TokenHash           NVARCHAR(128)    NOT NULL,
        ExpiresAtUtc        DATETIME2        NOT NULL,
        CreatedAtUtc        DATETIME2        NOT NULL
                                CONSTRAINT DF_RefreshTokens_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        RevokedAtUtc        DATETIME2        NULL,
        ReplacedByTokenId   UNIQUEIDENTIFIER NULL,

        CONSTRAINT PK_RefreshTokens PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_RefreshTokens_TokenHash ON dbo.RefreshTokens (TokenHash);
    CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens (UserId);
END
GO
