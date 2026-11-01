
-- COMP306 Lab 03
-- Learning Resource Portal Database

IF DB_ID('LearningPortalDB') IS NULL
    CREATE DATABASE LearningPortalDB;
GO

USE LearningPortalDB;
GO

-- Users table
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE Users (
        UserID INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        Role NVARCHAR(20) NOT NULL
    );
END
GO

-- Categories table
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE Categories (
        CategoryID INT PRIMARY KEY IDENTITY(1,1),
        CategoryName NVARCHAR(100) NOT NULL
    );
END
GO

-- Learning Resources table
IF OBJECT_ID('dbo.LearningResources', 'U') IS NULL
BEGIN
    CREATE TABLE LearningResources (
        ResourceID INT PRIMARY KEY IDENTITY(1,1),
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        CategoryID INT NOT NULL,
        AuthorID INT NOT NULL,
        CreatedDate DATETIME NOT NULL,
        S3Key NVARCHAR(255),
        ViewCount INT NOT NULL DEFAULT 0,
        FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
        FOREIGN KEY (AuthorID) REFERENCES Users(UserID)
    );
END
GO

-- Sample Users
-- SEED_ONLY is a placeholder, not a real password hash.
-- Use the website registration page for working login accounts.
IF NOT EXISTS (SELECT 1 FROM Users)
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, Role)
    VALUES
    ('johnsmith', 'john@example.com', 'SEED_ONLY', 'Instructor'),
    ('marylee', 'mary@example.com', 'SEED_ONLY', 'Instructor'),
    ('alexbrown', 'alex@example.com', 'SEED_ONLY', 'Student'),
    ('sarahkim', 'sarah@example.com', 'SEED_ONLY', 'Student'),
    ('davidng', 'david@example.com', 'SEED_ONLY', 'Student');
END
GO

-- Sample Categories
IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
    INSERT INTO Categories (CategoryName)
    VALUES
    ('Programming'),
    ('Database'),
    ('Web Development'),
    ('Cloud Computing'),
    ('Networking');
END
GO

-- Sample Learning Resources
IF NOT EXISTS (SELECT 1 FROM LearningResources)
BEGIN
    INSERT INTO LearningResources
    (Title, Description, CategoryID, AuthorID,
     CreatedDate, S3Key, ViewCount)
    VALUES
    ('C# Basics', 'Introduction to C# programming',
     1, 1, DATEADD(DAY, -4, GETDATE()), NULL, 10),

    ('SQL Fundamentals', 'Learn SQL queries and tables',
     2, 2, DATEADD(DAY, -3, GETDATE()), NULL, 15),

    ('ASP.NET MVC Guide', 'Guide to ASP.NET Core MVC',
     3, 1, DATEADD(DAY, -2, GETDATE()), NULL, 7),

    ('AWS Cloud Intro', 'Basics of AWS cloud services',
     4, 2, DATEADD(DAY, -1, GETDATE()), NULL, 12),

    ('Network Security Notes', 'Introduction to network security',
     5, 1, GETDATE(), NULL, 5);
END
GO

-- Verify record counts
SELECT 'Users' AS TableName, COUNT(*) AS Total FROM Users
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL
SELECT 'LearningResources', COUNT(*) FROM LearningResources;
GO
