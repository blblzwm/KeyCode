-- KeyCode Database Tables
-- Schema definition for KeyCode application

USE KeyCodeDB;
GO

-- Users Table
CREATE TABLE Users (
	UserID INT PRIMARY KEY IDENTITY(1,1),
	Username NVARCHAR(100) NOT NULL UNIQUE,
	Email NVARCHAR(255) NOT NULL UNIQUE,
	PasswordHash NVARCHAR(MAX) NOT NULL,
	FirstName NVARCHAR(100),
	LastName NVARCHAR(100),
	Role NVARCHAR(50) NOT NULL DEFAULT 'student', -- student, tutor, admin, guest
	IsActive BIT NOT NULL DEFAULT 1,
	IsDeleted BIT NOT NULL DEFAULT 0,
	CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
	LastLoginDate DATETIME
);
GO

-- Learning Materials Table
CREATE TABLE LearningMaterials (
	MaterialID INT PRIMARY KEY IDENTITY(1,1),
	Title NVARCHAR(255) NOT NULL,
	Description NVARCHAR(MAX),
	Content NVARCHAR(MAX),
	Chapter INT,
	TutorID INT NOT NULL,
	FileURL NVARCHAR(MAX),
	CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
	FOREIGN KEY (TutorID) REFERENCES Users(UserID)
);
GO

-- Mini Game Questions Table
CREATE TABLE MiniGameQuestions (
	QuestionID INT PRIMARY KEY IDENTITY(1,1),
	Question NVARCHAR(MAX) NOT NULL,
	CorrectAnswer NVARCHAR(MAX) NOT NULL,
	Explanation NVARCHAR(MAX),
	Category NVARCHAR(100),
	Difficulty INT, -- 1=Easy, 2=Medium, 3=Hard
	CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Forum Posts Table
CREATE TABLE ForumPosts (
	PostID INT PRIMARY KEY IDENTITY(1,1),
	Title NVARCHAR(255) NOT NULL,
	Content NVARCHAR(MAX) NOT NULL,
	UserID INT NOT NULL,
	Category NVARCHAR(100),
	IsApproved BIT NOT NULL DEFAULT 1,
	CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
	FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- Forum Replies Table
CREATE TABLE ForumReplies (
	ReplyID INT PRIMARY KEY IDENTITY(1,1),
	PostID INT NOT NULL,
	UserID INT NOT NULL,
	Content NVARCHAR(MAX) NOT NULL,
	IsApproved BIT NOT NULL DEFAULT 1,
	CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
	UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
	FOREIGN KEY (PostID) REFERENCES ForumPosts(PostID),
	FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- User Progress Table
CREATE TABLE UserProgress (
	ProgressID INT PRIMARY KEY IDENTITY(1,1),
	UserID INT NOT NULL,
	QuestionID INT,
	Score INT,
	TimeSpent INT, -- in seconds
	CompletedDate DATETIME NOT NULL DEFAULT GETDATE(),
	FOREIGN KEY (UserID) REFERENCES Users(UserID),
	FOREIGN KEY (QuestionID) REFERENCES MiniGameQuestions(QuestionID)
);
GO

-- Self Assessment Table
CREATE TABLE SelfAssessments (
	AssessmentID INT PRIMARY KEY IDENTITY(1,1),
	UserID INT NOT NULL,
	Topic NVARCHAR(255),
	Score INT, -- 0-100
	Feedback NVARCHAR(MAX),
	AssessmentDate DATETIME NOT NULL DEFAULT GETDATE(),
	FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- Certificates Table
CREATE TABLE Certificates (
	CertificateID INT PRIMARY KEY IDENTITY(1,1),
	UserID INT NOT NULL,
	CertificateName NVARCHAR(255),
	IssuedDate DATETIME NOT NULL DEFAULT GETDATE(),
	FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- Tutor Assignments Table
CREATE TABLE TutorAssignments (
	AssignmentID INT PRIMARY KEY IDENTITY(1,1),
	TutorID INT NOT NULL,
	StudentID INT NOT NULL,
	AssignedDate DATETIME NOT NULL DEFAULT GETDATE(),
	FOREIGN KEY (TutorID) REFERENCES Users(UserID),
	FOREIGN KEY (StudentID) REFERENCES Users(UserID)
);
GO

-- Reactivation Requests Table
CREATE TABLE ReactivationRequests (
	RequestID INT PRIMARY KEY IDENTITY(1,1),
	UserID INT NOT NULL,
	Reason NVARCHAR(MAX),
	Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected
	RequestDate DATETIME NOT NULL DEFAULT GETDATE(),
	ResolutionDate DATETIME,
	FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- Create Indexes
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_LearningMaterials_TutorID ON LearningMaterials(TutorID);
CREATE INDEX IX_LearningMaterials_Chapter ON LearningMaterials(Chapter);
CREATE INDEX IX_MiniGameQuestions_Category ON MiniGameQuestions(Category);
CREATE INDEX IX_MiniGameQuestions_Difficulty ON MiniGameQuestions(Difficulty);
CREATE INDEX IX_ForumPosts_UserID ON ForumPosts(UserID);
CREATE INDEX IX_ForumPosts_Category ON ForumPosts(Category);
CREATE INDEX IX_ForumReplies_PostID ON ForumReplies(PostID);
CREATE INDEX IX_ForumReplies_UserID ON ForumReplies(UserID);
CREATE INDEX IX_UserProgress_UserID ON UserProgress(UserID);
CREATE INDEX IX_UserProgress_QuestionID ON UserProgress(QuestionID);
CREATE INDEX IX_SelfAssessments_UserID ON SelfAssessments(UserID);
CREATE INDEX IX_Certificates_UserID ON Certificates(UserID);
CREATE INDEX IX_TutorAssignments_TutorID ON TutorAssignments(TutorID);
CREATE INDEX IX_TutorAssignments_StudentID ON TutorAssignments(StudentID);
CREATE INDEX IX_ReactivationRequests_UserID ON ReactivationRequests(UserID);

GO

PRINT 'All tables created successfully.'
