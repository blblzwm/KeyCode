using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace WAPP_Asm
{
    public partial class StudentDashboard : System.Web.UI.Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        protected int OverallProgressPercent = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDashboard();
            }
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