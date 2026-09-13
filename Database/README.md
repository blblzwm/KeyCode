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
- Click **Connect**

### Step 2: Run SQL Scripts

Run the scripts in order:

1. **Open and run `01_CreateDatabase.sql`**
   - This creates the `KeyCodeDB` database.
   - Click the **Execute** button.

2. **Open and run `02_CreateTables.sql`**
   - This creates all required tables.
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
   - This inserts sample data for testing.
   - Skip this step if you do not want sample data.
   - The sample data can include the test Admin account described below.
   - For the `sana` Admin account, copy and reuse the `PasswordHash` from the existing Admin user row in the database.

## Test Admin Account

For local development and testing, you can use the following Admin account:

- **Username:** `sana`
- **Password:** `Sana@1234`
- **Role:** `admin`

When adding this account in `03_InsertSampleData.sql`, do **not** insert the plain-text password into the `PasswordHash` column.

Instead, copy the existing `PasswordHash` value from the Admin account that already uses the password `Sana@1234`, and use that same hash for the `sana` user row.

For example, first find the existing Admin row:

```sql
SELECT UserID, Username, PasswordHash, Role
FROM Users
WHERE Role = 'admin';
```

Then copy the required `PasswordHash` value into the sample Admin user record in `03_InsertSampleData.sql`.

> **Note:** The `sana` account is intended for local development and testing only.

### Step 3: Verify Database

After running the scripts:

1. In SQL Server Management Studio, expand **Databases**.
2. You should see `KeyCodeDB`.
3. Expand **KeyCodeDB → Tables**.
4. You should see all 10 tables created.

### Step 4: Run the Application

- Open `WAPP_Asm.slnx` in Visual Studio.
- Press **F5** to run.
- The application will connect to the database automatically.

## Troubleshooting

### "Database already exists" error

- This is normal if the scripts are run multiple times.
- The scripts will drop and recreate the database.

### "Cannot connect to (LocalDB)\MSSQLLocalDB"

- Ensure LocalDB is installed. It normally comes with Visual Studio.
- Run the following command in PowerShell to verify:

```powershell
sqllocaldb info
```

- If LocalDB is not installed, install SQL Server Express with LocalDB support.

### Tables not created

- Verify that you are connected to the correct database (`KeyCodeDB`).
- Check that all previous scripts ran successfully.
- Look for error messages at the bottom of SQL Server Management Studio.

### Admin login does not work

- Confirm that the `sana` row exists in the `Users` table.
- Confirm that its `Role` is set to `admin`.
- Confirm that `IsActive` is enabled and `IsDeleted` is disabled.
- Confirm that the `PasswordHash` is copied exactly from the existing working Admin account that uses `Sana@1234`.
- Do not store `Sana@1234` directly in the `PasswordHash` column unless the application itself uses plain-text passwords, which is not recommended.

## Database Schema

### Users

- UserID (PK)
- Username, Email
- PasswordHash
- FirstName, LastName
- Role (`student`, `tutor`, `admin`, `guest`)
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
- Status (`Pending`, `Approved`, `Rejected`)
- RequestDate, ResolutionDate

## Notes

- All tables use `DATETIME` for timestamps with a default value of `GETDATE()`.
- Primary keys are `IDENTITY` integers (auto-increment).
- Foreign keys enforce referential integrity.
- Indexes are created on frequently queried columns.
- LocalDB is sufficient for development and testing.
- The sample Admin account is intended only for local testing.
- Passwords should be stored as hashes, not as plain text.

## Support

If you encounter any issues:

1. Check the connection string in `Web.config`.
2. Verify LocalDB is installed.
3. Ensure SQL Server Management Studio can connect.
4. Check file permissions in the database directory.

---

For more information, see the main [README.md](../README.md).
