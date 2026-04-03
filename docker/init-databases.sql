-- Initialize test databases for dbmig
-- This script runs when the SQL Server container starts for the first time

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'dbmig_default')
BEGIN
    CREATE DATABASE dbmig_default;
    PRINT 'Created database: dbmig_default';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'dbmig_unit_test')
BEGIN
    CREATE DATABASE dbmig_unit_test;
    PRINT 'Created database: dbmig_unit_test';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'dbmig_integration_test')
BEGIN
    CREATE DATABASE dbmig_integration_test;
    PRINT 'Created database: dbmig_integration_test';
END
GO
