# Repository Contents Summary

This document outlines the final structure of the KeyCode repository after cleanup.

## 📦 What's in the Repository

### Root Level
- **README.md** - Main documentation with setup instructions and API key configuration
- **.gitignore** - Prevents sensitive files from being committed
- **WAPP_Asm.slnx** - Solution file

### Database/ Folder
Contains SQL scripts to recreate the database locally:
- **01_CreateDatabase.sql** - Creates KeyCodeDB database
- **02_CreateTables.sql** - Creates all 10 database tables
- **03_InsertSampleData.sql** - Optional sample data for testing
- **README.md** - Database setup instructions

### WAPP_Asm/ Folder
The main ASP.NET Web Forms application:
- **Web.config** - ⚠️ Local configuration (NOT in Git) - you create this locally
- **Web.config.Debug.template** - Template for Debug configuration
- **Web.config.Release.template** - Template for Release configuration
- All web pages, stylesheets, scripts, and other application files

### Important Notes

❌ **NOT in Repository** (ignored by .gitignore):
- `WAPP_Asm/Web.config` - Replace with your own API keys
- `WAPP_Asm/App_Data/KeyCodeDB.mdf` - Created locally by running SQL scripts
- `WAPP_Asm/App_Data/KeyCodeDB_log.ldf` - Database log file
- Build output directories (`bin/`, `obj/`)
- Visual Studio cache (`.vs/`)

✅ **In Repository**:
- All source code (.cs, .aspx files)
- Stylesheets (.css)
- Scripts (.js)
- SQL database creation scripts
- Configuration templates
- README and documentation

## 🚀 Getting Started

1. **Clone** the repository
2. **Run SQL scripts** from `Database/` folder to create database
3. **Configure Web.config** with your API keys
4. **Run** the application in Visual Studio

See **README.md** for detailed instructions.

## 🔐 Security Features

- `Web.config` is in `.gitignore` - your secrets stay local
- `.mdf` database file is not committed - created locally from SQL scripts
- Use `Web.config.template` files as reference for configuration
- Never commit actual API keys or passwords

## 📋 Database Tables

The SQL scripts create 10 tables:
1. **Users** - Student, tutor, admin accounts
2. **LearningMaterials** - Course content
3. **MiniGameQuestions** - Coding challenges
4. **ForumPosts** - Discussion posts
5. **ForumReplies** - Post replies
6. **UserProgress** - Learning progress tracking
7. **SelfAssessments** - Self-evaluation records
8. **Certificates** - Achievement certificates
9. **TutorAssignments** - Tutor-student relationships
10. **ReactivationRequests** - Account reactivation requests

See `Database/README.md` for complete schema documentation.

## 🔑 API Keys Required

To run the application, you need API keys for:
- Google Gemini API
- Google reCAPTCHA v3
- Google OAuth 2.0
- OpenAI Moderation API (optional)
- Gmail SMTP credentials

Instructions for getting each key are in the main **README.md**.

---

**Last Updated**: Repository cleanup complete
**Status**: Ready for development and GitHub upload
