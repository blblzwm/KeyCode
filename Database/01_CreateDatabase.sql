-- KeyCode Database Setup
-- Create the KeyCodeDB database if it doesn't exist

USE master;
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'KeyCodeDB')
BEGIN
	ALTER DATABASE KeyCodeDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	DROP DATABASE KeyCodeDB;
END
GO

CREATE DATABASE KeyCodeDB;
GO

USE KeyCodeDB;
GO

PRINT 'Database KeyCodeDB created successfully.'
