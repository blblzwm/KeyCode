using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class Analytics : System.Web.UI.Page
    {
        private readonly string _connStr =
            ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["role"]?.ToString() != "admin")
                Response.Redirect("~/Asm_WebPage/Login.aspx");

            if (!IsPostBack)
            {
                // Assessment
                LoadAssessmentSummaryCards();
                LoadAvgScorePerChapter();
                LoadScoreDistribution();
                LoadPassFail();
                LoadTopStudents();

                // Forum
                LoadForumSummaryCards();
                LoadForumActivity();
                LoadPostsByRole();
                LoadMostActiveUsers();
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Asm_WebPage/AdminDashboard.aspx");
        }

        // ASSESSMENT

        private void LoadAssessmentSummaryCards()
        {
            string sql = @"
                SELECT
                    COUNT(DISTINCT userID) AS totalStudents,
                    SUM(CASE WHEN CAST(score AS FLOAT) / 20 * 100 >= 60 THEN 1 ELSE 0 END) * 100
                        / COUNT(*) AS passRate
                FROM AssessmentResults";

            using (SqlConnection con = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        litTotalStudents.Text = dr["totalStudents"].ToString();
                        litPassRate.Text = dr["passRate"].ToString();
                    }
                }
            }
        }

        private void LoadAvgScorePerChapter()
        {
            string sql = @"
                SELECT c.title,
                       CAST(AVG(CAST(ar.score AS FLOAT)/20*100) AS DECIMAL(5,1)) AS avgScore
                FROM   AssessmentResults ar
                INNER JOIN Chapters c ON ar.chapterID = c.chapterID
                GROUP BY c.chapterID, c.title
                ORDER BY c.chapterID";

            StringBuilder labels = new StringBuilder();
            StringBuilder scores = new StringBuilder();

            using (SqlConnection con = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        labels.Append($"'{dr["title"]}',");
                        scores.Append($"{dr["avgScore"]},");
                    }
                }
            }

            litBarLabels.Text = labels.ToString().TrimEnd(',');
            litBarScores.Text = scores.ToString().TrimEnd(',');
        }

        private void LoadScoreDistribution()
        {
            string sql = @"
                SELECT
                    SUM(CASE WHEN (CAST(score AS FLOAT)/20*100) BETWEEN 0  AND 20  THEN 1 ELSE 0 END) AS b1,
                    SUM(CASE WHEN (CAST(score AS FLOAT)/20*100) BETWEEN 21 AND 40  THEN 1 ELSE 0 END) AS b2,
                    SUM(CASE WHEN (CAST(score AS FLOAT)/20*100) BETWEEN 41 AND 60  THEN 1 ELSE 0 END) AS b3,
                    SUM(CASE WHEN (CAST(score AS FLOAT)/20*100) BETWEEN 61 AND 80  THEN 1 ELSE 0 END) AS b4,
                    SUM(CASE WHEN (CAST(score AS FLOAT)/20*100) BETWEEN 81 AND 100 THEN 1 ELSE 0 END) AS b5
                FROM AssessmentResults";

            using (SqlConnection con = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        litDistribution.Text =
                            $"{dr["b1"]},{dr["b2"]},{dr["b3"]},{dr["b4"]},{dr["b5"]}";
                }
            }
        }

        private void LoadPassFail()
        {
            string sql = @"
                SELECT
                    SUM(CASE WHEN CAST(score AS FLOAT)/20*100 >= 60 THEN 1 ELSE 0 END) AS passed,
                    SUM(CASE WHEN CAST(score AS FLOAT)/20*100 <  60 THEN 1 ELSE 0 END) AS failed
                FROM AssessmentResults";

            using (SqlConnection con = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        litPassFail.Text = $"{dr["passed"]},{dr["failed"]}";
                }
            }
        }

        private void LoadTopStudents()
        {
            string sql = @"
                SELECT TOP 5
                    u.username,
                    CAST(AVG(CAST(ar.score AS FLOAT)/20*100) AS DECIMAL(5,1)) AS avgScore
                FROM   AssessmentResults ar
                INNER JOIN Users u ON ar.userID = u.userID
                WHERE  u.role = 'Student'
                GROUP BY u.userID, u.username
                ORDER BY avgScore DESC";

            StringBuilder names = new StringBuilder();
            StringBuilder scores = new StringBuilder();

            using (SqlConnection con = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        names.Append($"'{dr["username"]}',");
                        scores.Append($"{dr["avgScore"]},");
                    }
                }
            }

            litTopNames.Text = names.ToString().TrimEnd(',');
            litTopScores.Text = scores.ToString().TrimEnd(',');
        }

        // FORUM

        private void LoadForumSummaryCards()
        {
            string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM ForumPosts)                         AS totalPosts,
                    (SELECT COUNT(*) FROM ForumComments)                      AS totalComments,
                    (SELECT COUNT(*) FROM ForumPosts WHERE is_announcement=1) AS totalAnnouncements,
                    (SELECT COUNT(DISTINCT userID) FROM (
                        SELECT userID FROM ForumPosts
                        UNION
                        SELECT userID FROM ForumComments
                    ) AS combined)                                            AS activeUsers";

            using (SqlConnection con = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        litTotalPosts.Text = dr["totalPosts"].ToString();
                        litTotalComments.Text = dr["totalComments"].ToString();
                        litAnnouncements.Text = dr["totalAnnouncements"].ToString();
                        litActiveUsers.Text = dr["activeUsers"].ToString();
                    }
                }
            }
        }

        private void LoadForumActivity()
        {
            string sql = @"
                SELECT theDate, SUM(posts) AS totalPosts, SUM(comments) AS totalComments
                FROM (
                    SELECT CAST(created_at AS DATE) AS theDate, COUNT(*) AS posts, 0 AS comments
                    FROM ForumPosts GROUP BY CAST(created_at AS DATE)
                    UNION ALL
                    SELECT CAST(created_at AS DATE) AS theDate, 0 AS posts, COUNT(*) AS comments
                    FROM ForumComments GROUP BY CAST(created_at AS DATE)
                ) AS x
                GROUP BY theDate ORDER BY theDate";

            StringBuilder dates = new StringBuilder();
            StringBuilder posts = new StringBuilder();
            StringBuilder comments = new StringBuilder();

            using (SqlConnection con = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        dates.Append($"'{Convert.ToDateTime(dr["theDate"]):yyyy-MM-dd}',");
                        posts.Append($"{dr["totalPosts"]},");
                        comments.Append($"{dr["totalComments"]},");
                    }
                }
            }

            litLineLabels.Text = dates.ToString().TrimEnd(',');
            litLinePosts.Text = posts.ToString().TrimEnd(',');
            litLineComments.Text = comments.ToString().TrimEnd(',');
        }

        private void LoadPostsByRole()
        {
            string sql = @"
                SELECT u.role, COUNT(*) AS total
                FROM ForumPosts p INNER JOIN Users u ON p.userID = u.userID
                GROUP BY u.role";

            int studentCount = 0, tutorCount = 0;

            using (SqlConnection con = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string role = dr["role"].ToString().ToLower();
                        if (role == "student") studentCount = (int)dr["total"];
                        else if (role == "tutor") tutorCount = (int)dr["total"];
                    }
                }
            }

            litPostsByRole.Text = $"{studentCount},{tutorCount}";
        }

        private void LoadMostActiveUsers()
        {
            string sql = @"
                SELECT TOP 5 u.username, COUNT(*) AS total
                FROM (
                    SELECT userID FROM ForumPosts
                    UNION ALL
                    SELECT userID FROM ForumComments
                ) AS x
                INNER JOIN Users u ON x.userID = u.userID
                GROUP BY u.userID, u.username
                ORDER BY total DESC";

            StringBuilder names = new StringBuilder();
            StringBuilder totals = new StringBuilder();

            using (SqlConnection con = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        names.Append($"'{dr["username"]}',");
                        totals.Append($"{dr["total"]},");
                    }
                }
            }

            litActiveNames.Text = names.ToString().TrimEnd(',');
            litActiveTotals.Text = totals.ToString().TrimEnd(',');
        }
    }
}