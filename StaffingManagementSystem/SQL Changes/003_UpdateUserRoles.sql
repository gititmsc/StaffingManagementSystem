/*
====================================================================
 Script     : 003_UpdateUserRoles.sql
 Purpose    : Aligns the dbo.Users role model with the role model
              defined in the RMS Requirements Specification
              (Section 2.2 User Roles / Section 7 Permission Matrix):
              Super Admin, HR Admin, Recruiter, Hiring Manager, Viewer.

 Changes    : - Old role values are remapped:
                  Admin         -> SuperAdmin
                  Interviewer   -> Viewer
                  Recruiter, HiringManager are unchanged.
              - CK_Users_Role is dropped and recreated with the new
                allowed values.
              - A demo HR Admin account is seeded (idempotent).

 Matches    : StaffingManagementSystem.Core.Enums.UserRole
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

-- 1. Drop the old CHECK constraint so existing 'Admin'/'Interviewer' rows can be remapped.
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Users_Role')
BEGIN
    ALTER TABLE dbo.Users DROP CONSTRAINT CK_Users_Role;
END
GO

-- 2. Remap existing role values to the new model.
UPDATE dbo.Users SET Role = N'SuperAdmin' WHERE Role = N'Admin';
UPDATE dbo.Users SET Role = N'Viewer'     WHERE Role = N'Interviewer';
GO

-- 3. Recreate the CHECK constraint with the new allowed role values.
ALTER TABLE dbo.Users
    ADD CONSTRAINT CK_Users_Role CHECK (Role IN (N'SuperAdmin', N'HRAdmin', N'Recruiter', N'HiringManager', N'Viewer'));
GO

-- 4. Seed a demo HR Admin account (password: Passw0rd!123, same convention as 002_SeedUsers.sql).
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'hradmin@itmusketeers.com')
BEGIN
    INSERT INTO dbo.Users (Id, FirstName, LastName, Email, PasswordHash, Role, IsActive, CreatedAtUtc)
    VALUES (NEWID(), N'Nora', N'Bennett', N'hradmin@itmusketeers.com',
            N'100000.pfsszAj55j5C1FNppUptaQ==.F3pcA6h9VkO3JtrKZrEzAAGQkx9NBCMxWf+TD7MGTqM=',
            N'HRAdmin', 1, SYSUTCDATETIME());
END

-- 5. Seed a demo Viewer account (replaces the retired Interviewer role for demo purposes).
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'viewer@itmusketeers.com')
BEGIN
    INSERT INTO dbo.Users (Id, FirstName, LastName, Email, PasswordHash, Role, IsActive, CreatedAtUtc)
    VALUES (NEWID(), N'Emma', N'Clarke', N'viewer@itmusketeers.com',
            N'100000.J6cL6mS589eUJugtKTgr6Q==.kNFUJXHAU2fRDGNgM3hIYVi1fPdOhhJ2wfP6aiIDeM4=',
            N'Viewer', 1, SYSUTCDATETIME());
END
GO
