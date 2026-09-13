# Database Setup

This folder contains SQL scripts to set up the KeyCode database.

## Overview

The KeyCode application uses SQL Server LocalDB. The database file (`KeyCodeDB.mdf`) is not included in the repository, so you need to create it using these SQL scripts.

## Files

1. **01_CreateDatabase.sql** - Creates the KeyCodeDB database
2. **02_CreateTables.sql** - Creates all database tables
3. **03_InsertSampleData.sql** - (Optional) Inserts sample data for testing

## Setup Instructions

### Step 1: Open SQL Server Management Studio

- Start SQL Server Management Studio
- Server name: `(LocalDB)\MSSQLLocalDB`
- Click Connect

### Step 2: Run SQL Scripts

Run the scripts in order:

1. **Open and run `01_CreateDatabase.sql`**
   - This creates the KeyCodeDB database
   - Click "Execute" button

2. **Open and run `02_CreateTables.sql`**
   - This creates all required tables
   - Tables created:
	 - Users
	 - LearningMaterials
	 - MiniGameQuestions
	 - ForumPosts
	 - ForumReplies
	 - UserProgress
	 - SelfAssessments
	 - Certificates
	 - TutorAssignments
	 - ReactivationRequests

3. **(Optional) Open and run `03_InsertSampleData.sql`**
   - This inserts sample data for testing
   - Skip if you don't want sample data
   - Note: Replace hashed passwords with actual hashed values in production

### Step 3: Verify Database

After running the scripts:

1. In SQL Server Management Studio, expand "Databases"
2. You should see "KeyCodeDB"
3. Expand KeyCodeDB → Tables
4. You should see all 10 tables created

### Step 4: Run the Application

- Open `WAPP_Asm.slnx` in Visual Studio
- Press F5 to run
- The application will connect to the database automatically

## Troubleshooting

### "Database already exists" error
- This is normal if running scripts multiple times
- The scripts will drop and recreate the database

### "Cannot connect to (LocalDB)\MSSQLLocalDB"
- Ensure LocalDB is installed (comes with Visual Studio)
- Run: `sqllocaldb info` in PowerShell to verify
- If not installed, install SQL Server 2019 Express with LocalDB

### Tables not created
- Verify you're connected to the correct database (KeyCodeDB)
- Check that all previous scripts ran successfully
- Look for error messages at the bottom of SQL Server Management Studio

## Database Schema

### Users
- UserID (PK)
- Username, Email
- PasswordHash
- FirstName, LastName
- Role (student, tutor, admin, guest)
- IsActive, IsDeleted
- CreatedDate, UpdatedDate, LastLoginDate

### LearningMaterials
- MaterialID (PK)
- Title, Description, Content
- Chapter
- TutorID (FK → Users)
- FileURL
- CreatedDate, UpdatedDate

### MiniGameQuestions
- QuestionID (PK)
- Question, CorrectAnswer, Explanation
- Category, Difficulty
- CreatedDate

### ForumPosts
- PostID (PK)
- Title, Content
- UserID (FK → Users)
- Category
- IsApproved
- CreatedDate, UpdatedDate

### ForumReplies
- ReplyID (PK)
- PostID (FK → ForumPosts)
- UserID (FK → Users)
- Content
- IsApproved
- CreatedDate, UpdatedDate

### UserProgress
- ProgressID (PK)
- UserID (FK → Users)
- QuestionID (FK → MiniGameQuestions)
- Score, TimeSpent
- CompletedDate

### SelfAssessments
- AssessmentID (PK)
- UserID (FK → Users)
- Topic, Score, Feedback
- AssessmentDate

### Certificates
- CertificateID (PK)
- UserID (FK → Users)
- CertificateName
- IssuedDate

### TutorAssignments
- AssignmentID (PK)
- TutorID (FK → Users)
- StudentID (FK → Users)
- AssignedDate

### ReactivationRequests
- RequestID (PK)
- UserID (FK → Users)
- Reason
- Status (Pending, Approved, Rejected)
- RequestDate, ResolutionDate

## Notes

- All tables use `DATETIME` for timestamps with default `GETDATE()`
- Primary keys are `IDENTITY` integers (auto-increment)
- Foreign keys enforce referential integrity
- Indexes are created on frequently queried columns
- Sample data uses placeholder hashed passwords - replace with real hashed values
- LocalDB is sufficient for development and testing

## Support

If you encounter any issues:
1. Check the connection string in `Web.config`
2. Verify LocalDB is installed
3. Ensure SQL Server Management Studio can connect
4. Check file permissions in the database directory

---

For more information, see the main [README.md](../README.md)
