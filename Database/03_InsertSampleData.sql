-- KeyCode Sample Data
-- Insert sample data for testing (optional)

USE KeyCodeDB;
GO

-- Insert Sample Users
INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Role, IsActive)
VALUES 
	('admin1', 'admin@keycode.org', 'hashed_password_1', 'Admin', 'User', 'admin', 1),
	('tutor1', 'tutor@keycode.org', 'hashed_password_2', 'John', 'Tutor', 'tutor', 1),
	('student1', 'student1@keycode.org', 'hashed_password_3', 'Jane', 'Student', 'student', 1),
	('student2', 'student2@keycode.org', 'hashed_password_4', 'Bob', 'Student', 'student', 1);
GO

-- Insert Sample Learning Materials
INSERT INTO LearningMaterials (Title, Description, Content, Chapter, TutorID)
VALUES 
	('Introduction to Programming', 'Basics of programming concepts', 'Programming is...', 1, 2),
	('Variables and Data Types', 'Understanding variables in C#', 'Variables are...', 1, 2),
	('Loops and Conditionals', 'Control flow in programming', 'Loops allow you to...', 2, 2);
GO

-- Insert Sample Mini Game Questions
INSERT INTO MiniGameQuestions (Question, CorrectAnswer, Explanation, Category, Difficulty)
VALUES 
	('What is a variable?', 'A named storage location', 'Variables store data values', 'Basics', 1),
	('What does ''if'' statement do?', 'Checks a condition', 'If statement executes code based on condition', 'Conditionals', 1),
	('Write a for loop in C#', 'for(int i=0; i<10; i++)', 'This loops from 0 to 9', 'Loops', 2);
GO

-- Insert Sample Forum Posts
INSERT INTO ForumPosts (Title, Content, UserID, Category, IsApproved)
VALUES 
	('How to start learning programming?', 'What resources do you recommend?', 3, 'General', 1),
	('Need help with loops', 'I don''t understand for loops', 4, 'Help', 1);
GO

-- Insert Sample Forum Replies
INSERT INTO ForumReplies (PostID, UserID, Content, IsApproved)
VALUES 
	(1, 2, 'Start with the basics in chapter 1', 1),
	(2, 2, 'Check the learning materials on loops', 1);
GO

-- Insert Sample User Progress
INSERT INTO UserProgress (UserID, QuestionID, Score, TimeSpent)
VALUES 
	(3, 1, 100, 120),
	(3, 2, 80, 150),
	(4, 1, 90, 100);
GO

-- Insert Sample Self Assessments
INSERT INTO SelfAssessments (UserID, Topic, Score, Feedback)
VALUES 
	(3, 'Variables', 85, 'Good understanding of variables'),
	(4, 'Loops', 70, 'Need more practice with loops');
GO

-- Insert Sample Tutor Assignments
INSERT INTO TutorAssignments (TutorID, StudentID)
VALUES 
	(2, 3),
	(2, 4);
GO

PRINT 'Sample data inserted successfully.'
PRINT 'Note: Replace hashed_password values with actual hashed passwords in production.'
