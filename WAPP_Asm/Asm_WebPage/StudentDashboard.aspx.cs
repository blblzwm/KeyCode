using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Script.Serialization;

namespace WAPP_Asm
{
    public partial class StudentDashboard : System.Web.UI.Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        protected int OverallProgressPercent = 0;
        protected bool IsGuestForPage = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            IsGuestForPage = IsGuest();
            string ajaxAction = Request.QueryString["ajax"];
            if (!string.IsNullOrEmpty(ajaxAction))
            {
                HandleRewardAjax(ajaxAction);
                return;
            }

            if (!IsPostBack)
            {
                BindDashboard();
            }
        }

        // ===================== Reward system (badges + XP + bonus quizzes) =====================

        // Fixed, deterministic reward per milestone — not random, since the badge/XP/quiz
        // attached to each milestone is specific content, not a chance draw.
        private class RewardDef
        {
            public string Name;
            public string Icon;
            public int Xp;
        }

        private static readonly Dictionary<int, RewardDef> RewardConfig = new Dictionary<int, RewardDef>
        {
            { 20, new RewardDef { Name = "Rookie Coder",     Icon = "🐣", Xp = 30 } },
            { 40, new RewardDef { Name = "Loop Ninja",       Icon = "🥷", Xp = 50 } },
            { 60, new RewardDef { Name = "Debug Detective",  Icon = "🕵️", Xp = 70 } },
            { 80, new RewardDef { Name = "Code Wizard",      Icon = "🧙", Xp = 100 } }
        };

        // Shape of each answer the client submits for grading a bonus quiz.
        private class BonusAnswer
        {
            public int questionID { get; set; }
            public string selected { get; set; }
        }

        private void HandleRewardAjax(string action)
        {
            Response.Clear();
            Response.ContentType = "application/json";
            var serializer = new JavaScriptSerializer();

            string studentId = CurrentStudentId();

            if (IsGuest())
            {
                Response.StatusCode = 401;
                Response.Write(serializer.Serialize(new { error = "not_logged_in" }));
                Response.End();
                return;
            }

            try
            {
                if (action == "list" && Request.HttpMethod == "GET")
                {
                    Response.Write(serializer.Serialize(GetClaimedRewards(studentId)));
                }
                else if (action == "claim" && Request.HttpMethod == "POST")
                {
                    int percent;
                    int.TryParse(Request.Form["percent"], out percent);

                    if (!RewardConfig.ContainsKey(percent))
                    {
                        Response.StatusCode = 400;
                        Response.Write(serializer.Serialize(new { error = "invalid_percent" }));
                    }
                    else
                    {
                        var result = ClaimMilestone(studentId, percent);
                        Response.Write(serializer.Serialize(result));
                    }
                }
                else if (action == "bonusquiz" && Request.HttpMethod == "GET")
                {
                    int percent;
                    int.TryParse(Request.QueryString["percent"], out percent);

                    if (!RewardConfig.ContainsKey(percent))
                    {
                        Response.StatusCode = 400;
                        Response.Write(serializer.Serialize(new { error = "invalid_percent" }));
                    }
                    else if (!HasClaimedMilestone(studentId, percent))
                    {
                        Response.StatusCode = 403;
                        Response.Write(serializer.Serialize(new { error = "not_unlocked" }));
                    }
                    else
                    {
                        var questions = GetBonusQuizQuestions(percent);
                        Response.Write(serializer.Serialize(new { questions = questions }));
                    }
                }
                else if (action == "bonusquizsubmit" && Request.HttpMethod == "POST")
                {
                    int percent;
                    int.TryParse(Request.Form["percent"], out percent);
                    string answersJson = Request.Form["answers"] ?? "[]";

                    if (!RewardConfig.ContainsKey(percent))
                    {
                        Response.StatusCode = 400;
                        Response.Write(serializer.Serialize(new { error = "invalid_percent" }));
                    }
                    else if (!HasClaimedMilestone(studentId, percent))
                    {
                        Response.StatusCode = 403;
                        Response.Write(serializer.Serialize(new { error = "not_unlocked" }));
                    }
                    else
                    {
                        var answers = serializer.Deserialize<List<BonusAnswer>>(answersJson);
                        var result = GradeBonusQuiz(percent, answers);
                        Response.Write(serializer.Serialize(result));
                    }
                }
                else
                {
                    Response.StatusCode = 400;
                    Response.Write(serializer.Serialize(new { error = "bad_request" }));
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write(serializer.Serialize(new { error = "server_error", message = ex.Message }));
            }

            Response.End();
        }

        private object GetClaimedRewards(string studentId)
        {
            var list = new List<object>();
            int totalXp = 0;

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(
                "SELECT milestonePercent, badgeName, badgeIcon, xpAwarded FROM StudentRewards WHERE userID=@uid", con))
            {
                cmd.Parameters.AddWithValue("@uid", studentId);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        int xp = Convert.ToInt32(dr["xpAwarded"]);
                        totalXp += xp;

                        list.Add(new
                        {
                            percent = Convert.ToInt32(dr["milestonePercent"]),
                            badgeName = dr["badgeName"].ToString(),
                            badgeIcon = dr["badgeIcon"].ToString(),
                            xpAwarded = xp
                        });
                    }
                }
            }

            return new { rewards = list, totalXp = totalXp };
        }

        private bool HasClaimedMilestone(string studentId, int percent)
        {
            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM StudentRewards WHERE userID=@uid AND milestonePercent=@p", con))
            {
                cmd.Parameters.AddWithValue("@uid", studentId);
                cmd.Parameters.AddWithValue("@p", percent);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private object ClaimMilestone(string studentId, int percent)
        {
            using (var con = new SqlConnection(Cs))
            {
                con.Open();

                // Already claimed? Return the same reward so re-opening the modal is consistent.
                using (var cmdCheck = new SqlCommand(
                    "SELECT badgeName, badgeIcon, xpAwarded FROM StudentRewards WHERE userID=@uid AND milestonePercent=@p", con))
                {
                    cmdCheck.Parameters.AddWithValue("@uid", studentId);
                    cmdCheck.Parameters.AddWithValue("@p", percent);

                    using (var dr = cmdCheck.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new
                            {
                                percent = percent,
                                badgeName = dr["badgeName"].ToString(),
                                badgeIcon = dr["badgeIcon"].ToString(),
                                xpAwarded = Convert.ToInt32(dr["xpAwarded"]),
                                alreadyClaimed = true
                            };
                        }
                    }
                }

                var def = RewardConfig[percent];

                try
                {
                    using (var cmdInsert = new SqlCommand(@"
                        INSERT INTO StudentRewards (userID, milestonePercent, badgeName, badgeIcon, xpAwarded, dateClaimed)
                        VALUES (@uid, @p, @name, @icon, @xp, GETDATE())", con))
                    {
                        cmdInsert.Parameters.AddWithValue("@uid", studentId);
                        cmdInsert.Parameters.AddWithValue("@p", percent);
                        cmdInsert.Parameters.AddWithValue("@name", def.Name);
                        cmdInsert.Parameters.AddWithValue("@icon", def.Icon);
                        cmdInsert.Parameters.AddWithValue("@xp", def.Xp);
                        cmdInsert.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    // Unique constraint hit = a second tab/request claimed it a moment earlier.
                    using (var cmdRefetch = new SqlCommand(
                        "SELECT badgeName, badgeIcon, xpAwarded FROM StudentRewards WHERE userID=@uid AND milestonePercent=@p", con))
                    {
                        cmdRefetch.Parameters.AddWithValue("@uid", studentId);
                        cmdRefetch.Parameters.AddWithValue("@p", percent);

                        using (var dr = cmdRefetch.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                return new
                                {
                                    percent = percent,
                                    badgeName = dr["badgeName"].ToString(),
                                    badgeIcon = dr["badgeIcon"].ToString(),
                                    xpAwarded = Convert.ToInt32(dr["xpAwarded"]),
                                    alreadyClaimed = true
                                };
                            }
                        }
                    }

                    throw;
                }

                return new
                {
                    percent = percent,
                    badgeName = def.Name,
                    badgeIcon = def.Icon,
                    xpAwarded = def.Xp,
                    alreadyClaimed = false
                };
            }
        }

        private List<object> GetBonusQuizQuestions(int percent)
        {
            var list = new List<object>();

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                SELECT questionID, questionText, optionA, optionB, optionC, optionD
                FROM BonusQuizQuestions
                WHERE milestonePercent=@p
                ORDER BY questionID", con))
            {
                cmd.Parameters.AddWithValue("@p", percent);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new
                        {
                            questionID = Convert.ToInt32(dr["questionID"]),
                            questionText = dr["questionText"].ToString(),
                            optionA = dr["optionA"].ToString(),
                            optionB = dr["optionB"].ToString(),
                            optionC = dr["optionC"].ToString(),
                            optionD = dr["optionD"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        private object GradeBonusQuiz(int percent, List<BonusAnswer> answers)
        {
            var correctMap = new Dictionary<int, string>();

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(
                "SELECT questionID, correctOption FROM BonusQuizQuestions WHERE milestonePercent=@p", con))
            {
                cmd.Parameters.AddWithValue("@p", percent);
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        correctMap[Convert.ToInt32(dr["questionID"])] = dr["correctOption"].ToString();
                    }
                }
            }

            var results = new List<object>();
            int score = 0;

            foreach (var qid in correctMap.Keys)
            {
                string correctOption = correctMap[qid];
                var submitted = answers?.Find(a => a.questionID == qid);
                bool isCorrect = submitted != null &&
                    string.Equals(submitted.selected, correctOption, StringComparison.OrdinalIgnoreCase);

                if (isCorrect) score++;

                results.Add(new
                {
                    questionID = qid,
                    correct = isCorrect,
                    correctOption = correctOption
                });
            }

            return new { results = results, score = score, total = correctMap.Count };
        }


        private bool IsGuest()
        {
            string role = (Session["role"] ?? "nonreg_student").ToString();
            return role.Equals("nonreg_student", StringComparison.OrdinalIgnoreCase)
                || role.Equals("guest", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrEmpty((Session["UserID"] ?? "").ToString());
        }

        private string CurrentStudentId()
        {
            return (Session["UserID"] ?? "").ToString().Trim();
        }

        private void BindDashboard()
        {
            bool isGuest = IsGuest();
            string username = (Session["username"] ?? "Visitor").ToString().Trim();

            if (isGuest)
            {
                litSubWelcome.Text = "⚠ Sign Up to unlock more courses and track your progress.";
            }

            pnlOverall.Visible = !isGuest;
            pnlBonusQuizzes.Visible = !isGuest;

            var list = new List<CourseRow>();

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand("SELECT chapterID, title, description FROM Chapters ORDER BY chapterID", con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string chapterId = dr["chapterID"].ToString().Trim();
                        string title = dr["title"].ToString();
                        string desc = dr["description"].ToString();

                        bool locked = isGuest && !chapterId.Equals("C001", StringComparison.OrdinalIgnoreCase);

                        int progress = 0;
                        if (!locked && !isGuest)
                            progress = GetChapterProgressPercent(chapterId);

                        var stats = GetChapterStats(chapterId);
                        string teachers = GetChapterTeachers(chapterId);

                        list.Add(new CourseRow
                        {
                            ChapterId = chapterId,
                            Title = title,
                            Subtitle = desc,
                            IsLocked = locked,
                            Link = ResolveUrl("~/Asm_WebPage/LearningMaterial.aspx?chapter=" + Server.UrlEncode(chapterId)),
                            ProgressPercent = progress,
                            ImageUrl = GetChapterImage(chapterId),
                            TeacherNames = string.IsNullOrWhiteSpace(teachers) ? "KeyCode Tutors" : teachers,
                            SubtopicCount = stats.SubtopicCount,
                            PracticeQuestionCount = stats.PracticeQuestionCount,
                            SelfAssessmentCount = stats.SelfAssessmentCount
                        });
                    }
                }
            }

            rptStudentDashboard.DataSource = list;
            rptStudentDashboard.DataBind();

            if (!isGuest)
            {
                OverallProgressPercent = GetOverallProgressPercent();
                litOverallPercent.Text = $"{OverallProgressPercent}% Completed";
            }
        }

        private int GetOverallProgressPercent()
        {
            string studentId = CurrentStudentId();
            if (string.IsNullOrEmpty(studentId)) return 0;

            int totalItems = 0;
            int doneItems = 0;

            using (var con = new SqlConnection(Cs))
            {
                con.Open();

                using (var cmdTotal = new SqlCommand(@"
                    SELECT 
                        (SELECT COUNT(*) FROM Subtopics WHERE isDeleted = 0) + 
                        (SELECT COUNT(DISTINCT chapterID) FROM SelfAssessments WHERE isDeleted = 0) AS GrandTotal", con))
                {
                    totalItems = Convert.ToInt32(cmdTotal.ExecuteScalar());
                }

                using (var cmdDone = new SqlCommand(@"
                    SELECT 
                        (SELECT COUNT(DISTINCT pt.subtopicID)
                         FROM ProgressTracking pt
                         INNER JOIN Subtopics s ON s.subtopicID = pt.subtopicID
                         WHERE pt.userID=@id AND s.isDeleted = 0) +
                        (SELECT COUNT(DISTINCT ar.chapterID)
                         FROM AssessmentResults ar
                         INNER JOIN SelfAssessments sa ON sa.chapterID = ar.chapterID
                         WHERE ar.userID=@id AND sa.isDeleted = 0) AS GrandDone", con))
                {
                    cmdDone.Parameters.AddWithValue("@id", studentId);
                    doneItems = Convert.ToInt32(cmdDone.ExecuteScalar());
                }
            }

            if (totalItems == 0) return 0;
            return (int)Math.Round((doneItems * 100.0) / totalItems);
        }

        private int GetChapterProgressPercent(string chapterId)
        {
            string studentId = CurrentStudentId();
            if (string.IsNullOrEmpty(studentId)) return 0;

            int totalSubtopics = 0;
            int completedSubtopics = 0;
            int assessmentPointsTotal = 0;
            int assessmentPointsDone = 0;

            using (var con = new SqlConnection(Cs))
            {
                con.Open();

                using (var cmdTotal = new SqlCommand("SELECT COUNT(*) FROM Subtopics WHERE chapterID=@cid AND isDeleted=0", con))
                {
                    cmdTotal.Parameters.AddWithValue("@cid", chapterId);
                    totalSubtopics = Convert.ToInt32(cmdTotal.ExecuteScalar());
                }

                using (var cmdDone = new SqlCommand(@"
                    SELECT COUNT(DISTINCT pt.subtopicID)
                    FROM ProgressTracking pt
                    INNER JOIN Subtopics s ON s.subtopicID = pt.subtopicID
                    WHERE pt.userID=@uid AND s.chapterID=@cid AND s.isDeleted=0", con))
                {
                    cmdDone.Parameters.AddWithValue("@uid", studentId);
                    cmdDone.Parameters.AddWithValue("@cid", chapterId);
                    completedSubtopics = Convert.ToInt32(cmdDone.ExecuteScalar());
                }

                using (var cmdCheckSA = new SqlCommand("SELECT COUNT(*) FROM SelfAssessments WHERE chapterID=@cid AND isDeleted=0", con))
                {
                    cmdCheckSA.Parameters.AddWithValue("@cid", chapterId);
                    int saCount = Convert.ToInt32(cmdCheckSA.ExecuteScalar());
                    assessmentPointsTotal = (saCount > 0) ? 1 : 0;
                }

                if (assessmentPointsTotal > 0)
                {
                    using (var cmdAss = new SqlCommand(@"
                        SELECT COUNT(*)
                        FROM AssessmentResults ar
                        INNER JOIN SelfAssessments sa ON sa.chapterID = ar.chapterID
                        WHERE ar.userID=@uid AND ar.chapterID=@cid AND sa.isDeleted=0", con))
                    {
                        cmdAss.Parameters.AddWithValue("@uid", studentId);
                        cmdAss.Parameters.AddWithValue("@cid", chapterId);
                        int hasRecord = Convert.ToInt32(cmdAss.ExecuteScalar());
                        assessmentPointsDone = (hasRecord > 0) ? 1 : 0;
                    }
                }
            }

            int grandTotal = totalSubtopics + assessmentPointsTotal;
            int grandDone = completedSubtopics + assessmentPointsDone;

            if (grandTotal == 0) return 0;

            return (int)Math.Round((grandDone * 100.0) / grandTotal);
        }

        private ChapterStats GetChapterStats(string chapterId)
        {
            var stats = new ChapterStats();

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                SELECT
                    (SELECT COUNT(*) 
                     FROM Subtopics 
                     WHERE chapterID=@cid AND isDeleted=0) AS Subtopics,

                    (SELECT COUNT(*)
                     FROM PracticeQuestions pq
                     INNER JOIN Subtopics s ON s.subtopicID = pq.subtopicID
                     WHERE s.chapterID=@cid AND s.isDeleted=0 AND pq.isDeleted=0) AS PracticeQuestions,

                    (SELECT COUNT(*)
                     FROM SelfAssessments sa
                     WHERE sa.chapterID=@cid AND sa.isDeleted=0) AS SelfAssessments
            ", con))
            {
                cmd.Parameters.AddWithValue("@cid", chapterId);

                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        stats.SubtopicCount = Convert.ToInt32(dr["Subtopics"]);
                        stats.PracticeQuestionCount = Convert.ToInt32(dr["PracticeQuestions"]);
                        stats.SelfAssessmentCount = Convert.ToInt32(dr["SelfAssessments"]);
                    }
                }
            }

            return stats;
        }

        private string GetChapterTeachers(string chapterId)
        {
            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                WITH teacher_ids AS (
                    SELECT DISTINCT s.created_by AS tutorID
                    FROM Subtopics s
                    WHERE s.chapterID = @cid AND s.created_by IS NOT NULL AND s.isDeleted = 0

                    UNION

                    SELECT DISTINCT pq.created_by AS tutorID
                    FROM PracticeQuestions pq
                    INNER JOIN Subtopics s ON s.subtopicID = pq.subtopicID
                    WHERE s.chapterID = @cid AND pq.created_by IS NOT NULL AND s.isDeleted = 0 AND pq.isDeleted = 0

                    UNION

                    SELECT DISTINCT sa.created_by AS tutorID
                    FROM SelfAssessments sa
                    WHERE sa.chapterID = @cid AND sa.created_by IS NOT NULL AND sa.isDeleted = 0
                ),
                teacher_names AS (
                    SELECT
                        t.tutorID,
                        CASE
                            WHEN LOWER(LTRIM(RTRIM(ISNULL(u.qualification, '')))) = 'phd'
                                THEN 'Dr. ' + LTRIM(RTRIM(ISNULL(u.fname, ''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname, '')))
                            WHEN LOWER(LTRIM(RTRIM(ISNULL(u.qualification, '')))) = 'master'
                                THEN
                                    LTRIM(RTRIM(ISNULL(u.fname, ''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname, '')))
                                    + ', ' + LTRIM(RTRIM(ISNULL(u.qualification, '')))
                            WHEN LOWER(LTRIM(RTRIM(ISNULL(u.qualification, '')))) = 'degree'
                                THEN
                                    'Tutor ' + LTRIM(RTRIM(ISNULL(u.fname, ''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname, '')))
                            ELSE
                                CASE
                                    WHEN (LTRIM(RTRIM(ISNULL(u.fname,''))) <> '' OR LTRIM(RTRIM(ISNULL(u.lname,''))) <> '')
                                        THEN LTRIM(RTRIM(ISNULL(u.fname, ''))) + ' ' + LTRIM(RTRIM(ISNULL(u.lname, '')))
                                    ELSE LTRIM(RTRIM(ISNULL(u.username, '')))
                                END
                        END AS DisplayName
                    FROM teacher_ids t
                    INNER JOIN Users u ON u.userID = t.tutorID
                )
                SELECT STRING_AGG(DisplayName, ' | ') AS TeacherNames
                FROM teacher_names
            ", con))
            {
                cmd.Parameters.AddWithValue("@cid", chapterId);

                con.Open();
                object val = cmd.ExecuteScalar();
                return (val == null || val == DBNull.Value) ? "" : val.ToString();
            }
        }

        private string GetChapterImage(string chapterId)
        {
            switch (chapterId.ToUpper())
            {
                case "C001": return "~/chp1.png";
                case "C002": return "~/chp2.png";
                case "C003": return "~/chp3.png";
                default: return "~/chp1.png";
            }
        }

        private class CourseRow
        {
            public string ChapterId { get; set; }
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public bool IsLocked { get; set; }
            public string Link { get; set; }
            public int ProgressPercent { get; set; }
            public string TeacherNames { get; set; }
            public int SubtopicCount { get; set; }
            public int PracticeQuestionCount { get; set; }
            public int SelfAssessmentCount { get; set; }
            public string ImageUrl { get; set; }
        }

        private class ChapterStats
        {
            public int SubtopicCount { get; set; }
            public int PracticeQuestionCount { get; set; }
            public int SelfAssessmentCount { get; set; }
        }
    }
}