/*
====================================================================
 Script     : 011_ConsolidateUserRoles.sql
 Purpose    : Reduces the role model to exactly three roles per the
              latest requirements:
                  Admin      - full system access (was SuperAdmin + HRAdmin)
                  Recruiter  - candidates + reports only (unchanged)
                  Viewer     - view-only candidate list/detail, with
                               Name/Email/LinkedIn/Phone masked by the
                               API for this role (was Viewer + HiringManager)

 Changes    : - Old role values are remapped:
                  SuperAdmin    -> Admin
                  HRAdmin       -> Admin
                  HiringManager -> Viewer
                  Recruiter, Viewer are unchanged.
              - CK_Users_Role is dropped and recreated with the new
                allowed values (Admin, Recruiter, Viewer).

 Matches    : StaffingManagementSystem.Core.Enums.UserRole
====================================================================
*/

USE [StaffingManagementSystemDb];
GO

-- 1. Drop the old CHECK constraint so existing rows can be remapped.
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Users_Role')
BEGIN
    ALTER TABLE dbo.Users DROP CONSTRAINT CK_Users_Role;
END
GO

-- 2. Remap existing role values to the new 3-role model.
UPDATE dbo.Users SET Role = N'Admin'  WHERE Role = N'SuperAdmin';
UPDATE dbo.Users SET Role = N'Admin'  WHERE Role = N'HRAdmin';
UPDATE dbo.Users SET Role = N'Viewer' WHERE Role = N'HiringManager';
GO

-- 3. Recreate the CHECK constraint with the new allowed role values.
ALTER TABLE dbo.Users
    ADD CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'Recruiter', N'Viewer'));
GO
