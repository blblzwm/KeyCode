# KeyCode 🎓

An interactive educational web application designed to teach programming concepts through engaging mini-games, real-time AI assistance, and community-driven learning.

## 📋 Table of Contents

- [Features](#features)
- [Project Overview](#project-overview)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Setup Instructions](#setup-instructions)
- [Database Setup](#database-setup)
- [Project Structure](#project-structure)
- [Key Features](#key-features)
- [User Roles](#user-roles)
- [Contributing](#contributing)
- [License](#license)

## Features

✨ **Student-Centric Learning Platform**
- 👾 **Mini-Games**: Interactive coding challenges with instant feedback
- 🤖 **AI Assistant**: Ask AI feature powered by Google Gemini for real-time help
- 📚 **Learning Materials**: Structured tutorials and code documentation
- 🎯 **Self-Assessment**: Track your progress with self-evaluation tools
- 📊 **Analytics Dashboard**: Visualize learning progress and statistics
- 👥 **Community Forum**: Discuss concepts and get peer support
- 🔐 **Secure Authentication**: Support for local accounts and Google OAuth

**Tutor Features**
- 📤 **Content Upload**: Share custom learning materials
- 📈 **Student Progress Tracking**: Monitor student analytics
- 📋 **Dashboard**: Overview of assigned students and submissions

**Admin Features**
- 👨‍💼 **User Management**: Add, remove, and manage user accounts
- 🔍 **System Analytics**: View platform-wide analytics
- ✅ **Reactivation Requests**: Handle account reactivation requests
- 🛡️ **Content Moderation**: Moderate forum posts and user-generated content

## Project Overview

KeyCode is a full-stack ASP.NET Web Forms application built to make programming education interactive and accessible. It provides a structured learning pathway with gamification elements, real-time AI support, and community engagement tools.

## Tech Stack

- **Backend Framework**: ASP.NET Framework 4.7.2
- **Language**: C# with ASP.NET Web Forms
- **Frontend**: HTML5, CSS3, JavaScript with Bootstrap 5
- **Database**: SQL Server (LocalDB)
- **APIs**:
  - Google Gemini 3.5 Flash (AI Assistance)
  - Google OAuth 2.0 (Authentication)
  - reCAPTCHA v3 (Bot Protection)
  - OpenAI Moderation API (Content Safety)
- **Scripting**: Custom JavaScript for mini-games and interactions
- **Email Service**: Gmail SMTP for notifications

## Getting Started

### Prerequisites

- Visual Studio Community 2026 or later
- .NET Framework 4.7.2
- SQL Server 2019 or later (or SQL Server Express with LocalDB)
- Active internet connection for API services

## Setup Instructions

### Quick Start (30 minutes)

1. **Clone the repository**
   ```bash
   git clone https://github.com/blblzwm/KeyCode.git
   cd KeyCode
   ```

2. **Setup Database**
   - Open SQL Server Management Studio
   - Connect to: `(LocalDB)\MSSQLLocalDB`
   - Run scripts from `Database/` folder in order:
     - Run: `01_CreateDatabase.sql`
     - Run: `02_CreateTables.sql`
     - (Optional) Run: `03_InsertSampleData.sql`
   - See `Database/README.md` for detailed instructions

3. **Configure Web.config**
   ```bash
   cd WAPP_Asm
   # Edit Web.config with your API keys (see section below)
   cd ..
   ```

4. **Open and Run in Visual Studio**
   - Open `WAPP_Asm.slnx` in Visual Studio
   - NuGet packages will restore automatically
   - Press `F5` to run
   - App opens at: `https://localhost:44302`

### 🔑 Getting API Keys for Testing

You'll need to get your own API keys from these services. Copy `Web.config.example` to `Web.config` and add your keys:

#### 1. Google Gemini API Key
- Go to: https://aistudio.google.com/app/apikey
- Click "Get API Key"
- Copy the key to `Web.config`:
  ```xml
  <add key="GeminiKey" value="YOUR_KEY_HERE" />
  ```

#### 2. Google reCAPTCHA v3
- Go to: https://www.google.com/recaptcha/admin
- Create a new site
- Add keys to `Web.config`:
  ```xml
  <add key="RecaptchaSiteKey" value="YOUR_SITE_KEY_HERE" />
  <add key="RecaptchaSecretKey" value="YOUR_SECRET_KEY_HERE" />
  ```

#### 3. Google OAuth 2.0
- Go to: https://console.cloud.google.com
- Create OAuth 2.0 credentials
- Add to `Web.config`:
  ```xml
  <add key="GoogleClientId" value="YOUR_CLIENT_ID_HERE" />
  <add key="GoogleRedirectUri" value="https://localhost:44302/Asm_WebPage/GoogleAuth.aspx" />
  ```

#### 4. OpenAI Moderation API (Optional)
- Go to: https://platform.openai.com/account/api-keys
- Create API key
- Add to `Web.config`:
  ```xml
  <add key="OpenAIModerationKey" value="YOUR_KEY_HERE" />
  ```

#### 5. Gmail SMTP (For password reset emails)
- Go to: Google Account → Security → App passwords
- Generate app-specific password
- Add to `Web.config`:
  ```xml
  <network 
      host="smtp.gmail.com" 
      port="587" 
      userName="your-email@gmail.com" 
      password="YOUR_APP_PASSWORD" />
  ```

⚠️ **Important**: Never commit `Web.config` to Git - it's in `.gitignore`

## Database Setup

The application uses SQL Server LocalDB. The database file (`KeyCodeDB.mdf`) cannot be uploaded to GitHub.

### Option 1: Using SQL Files (Recommended)

SQL setup scripts are provided in the `Database/` folder to recreate the database schema:

```bash
# Open SQL Server Management Studio (LocalDB)
# Server name: (LocalDB)\MSSQLLocalDB
# Run the scripts in order:
# 1. Database/01_CreateDatabase.sql
# 2. Database/02_CreateTables.sql
# 3. Database/03_InsertSampleData.sql (optional)
```

### Option 2: Automatic Database Creation

The application will automatically create the database on first run if it doesn't exist. You'll need to:

1. Ensure LocalDB is installed (comes with Visual Studio)
2. The connection string in `Web.config` should be:
   ```xml
   <add name="KeyCodeDB" 
        connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\KeyCodeDB.mdf;Integrated Security=True" 
        providerName="System.Data.SqlClient" />
   ```
3. Run the application - database will be created automatically

### Verify Database

After setup, verify the database:
```bash
# In SQL Server Management Studio
# Connect to: (LocalDB)\MSSQLLocalDB
# Check: KeyCodeDB database exists
# Check: All tables are created
```

## Project Structure

```
WAPP_Asm/
├── Asm_WebPage/           # Core web pages (.aspx files)
│   ├── HomePage.aspx
│   ├── Login.aspx
│   ├── SignUp.aspx
│   ├── StudentDashboard.aspx
│   ├── TutorDashboard.aspx
│   ├── AdminDashboard.aspx
│   ├── MiniGame.aspx      # Interactive coding challenges
│   ├── LearningMaterial.aspx
│   ├── Forum.aspx         # Community discussion
│   ├── SelfAssessment.aspx
│   ├── Analytics.aspx     # Progress tracking
│   └── AskAI.ashx         # AI assistant endpoint
├── Asm_StyleSheet/        # CSS stylesheets
│   ├── MasterStyle.css    # Global styles
│   ├── HomeStyle.css
│   ├── MiniGameStyle.css
│   ├── ForumStyle.css
│   └── ... (feature-specific styles)
├── Asm_Scripts/           # JavaScript files
│   ├── MiniGameQuestions.js  # Game logic
│   ├── PythonRunner.js       # Code execution
│   └── ... (utility scripts)
├── App_Data/              # Database files
│   ├── KeyCodeDB.mdf
│   └── KeyCodeDB_log.ldf
├── App_Start/             # ASP.NET configuration
│   ├── RouteConfig.cs
│   └── BundleConfig.cs
├── Content/               # Bootstrap and vendor styles
├── Scripts/               # jQuery, Bootstrap JS libraries
├── Web.config             # Application configuration
├── Site.Master            # Master page layout
└── Global.asax            # Application initialization
```

## Key Features

### 1. Mini-Games (MiniGame.aspx)
Interactive coding challenges that teach programming concepts through gamification.
- Dynamic question loading from database
- Real-time feedback and scoring
- Progress tracking

### 2. AI Assistance (AskAI.ashx)
Powered by Google Gemini API for intelligent tutoring.
- Context-aware responses to student questions
- Content moderation via OpenAI API
- Session-based question history

### 3. Forum (Forum.aspx)
Community learning space for peer-to-peer support.
- Post creation and discussion threads
- Content moderation
- User reputation system

### 4. Learning Materials (LearningMaterial.aspx)
Curated educational content with tutor contributions.
- Organized by chapters/topics
- File upload and download capabilities
- Markdown support for formatted content

### 5. Analytics Dashboard (Analytics.aspx)
Comprehensive progress tracking.
- Visual charts and statistics
- Time spent on activities
- Performance metrics

### 6. User Management
Role-based access control:
- **Students**: Full access to learning materials and games
- **Tutors**: Can upload materials and view assigned students
- **Admins**: Full system control

## User Roles

| Role | Capabilities |
|------|--------------|
| **Guest** | View home page, limited content preview |
| **Student** | Access all learning features, participate in forum |
| **Tutor** | Manage learning materials, track student progress |
| **Admin** | User management, system analytics, moderation |

## Database

### Key Tables

- **Users**: User accounts and authentication
- **Roles**: User role definitions
- **LearningMaterials**: Educational content
- **MiniGameQuestions**: Game questions and answers
- **ForumPosts**: Forum discussions
- **UserProgress**: Learning analytics and progress tracking
- **Certificates**: User achievements

**Connection String**:
```
Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\KeyCodeDB.mdf;Integrated Security=True
```

## API Integration

### Google Gemini
- **Purpose**: AI-powered tutoring assistant
- **Endpoint**: Custom handler via `AskAI.ashx`
- **Key Config**: `appSettings["GeminiKey"]` and `GeminiModel`

### Google OAuth
- **Purpose**: SecureLogin with Google accounts
- **Redirect URI**: `https://localhost:44302/Asm_WebPage/GoogleAuth.aspx`
- **Config**: `GoogleClientId`, `GoogleRedirectUri`

### reCAPTCHA v3
- **Purpose**: Bot prevention on sign-up and login
- **Config**: `RecaptchaSiteKey`, `RecaptchaSecretKey`

### Gmail SMTP
- **Purpose**: Email notifications (password reset, etc.)
- **Config**: Located in `Web.config` system.net/mailSettings section
- **From Address**: `keycode.org@gmail.com`

## Development

### Running Tests
```bash
# Build the project
dotnet build WAPP_Asm.csproj

# Run in Debug mode
# Use Visual Studio F5 or IDE controls
```

### Building Release
```bash
# Build release configuration
dotnet build -c Release WAPP_Asm.csproj
```

### Code Organization

- **Code-behind files** (.aspx.cs): Page logic and server-side handlers
- **Designer files** (.aspx.designer.cs): Auto-generated control declarations
- **Global.asax.cs**: Application-level events and initialization

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards

- Follow C# naming conventions (PascalCase for public members)
- Write meaningful commit messages
- Test changes thoroughly before submitting PR
- Update documentation for new features

## Troubleshooting

### Common Issues

**Database Connection Error**
- Ensure LocalDB is installed and running
- Check connection string in Web.config
- Verify database file exists in App_Data folder

**API Key Errors**
- Validate all API keys in Web.config
- Ensure keys are not expired
- Check API service quotas

**HTTPS/SSL Issues**
- Development uses self-signed certificates
- Trust the certificate or disable SSL validation for localhost

## Security Considerations

- 🔐 API keys are stored in Web.config (use environment variables in production)
- 🛡️ Password hashing with OTP pepper in config
- ✅ reCAPTCHA integration for bot prevention
- 🚫 Content moderation via OpenAI API
- 🔑 Role-based access control on all protected pages

## Performance Optimization

- Bootstrap bundle optimization in BundleConfig.cs
- CSS/JS minification in production builds
- LocalDB connection pooling
- Session-based caching for user data

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support & Contact

For issues, questions, or suggestions:
- GitHub Issues: [KeyCode Issues](https://github.com/blblzwm/KeyCode/issues)
- Email: keycode.org@gmail.com

---

**Built with ❤️ for educators and learners**

Last Updated: 2026 | Version: 1.0
