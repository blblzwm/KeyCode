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


        private string reportDashboardHtml;
        private string ReportDashboardHtml()
        {
            if (reportDashboardHtml == null) reportDashboardHtml = RenderReportAnalyticsCore();
            return reportDashboardHtml;
        }
        protected string RenderReportMetrics()
        {
            string html = ReportDashboardHtml();
            int start = html.IndexOf("<div class='report-metrics'>", StringComparison.Ordinal);
            int end = html.IndexOf("<div class='report-breakdowns'>", StringComparison.Ordinal);
            return start >= 0 && end > start ? html.Substring(start, end - start) : "";
        }
        protected string RenderReportAnalytics()
        {
            string html = ReportDashboardHtml();
            string metrics = RenderReportMetrics();
            return string.IsNullOrEmpty(metrics) ? html : html.Replace(metrics, "");
        }
        private string RenderReportAnalyticsCore()
        {
            if (Session["UserID"] == null || !string.Equals(Convert.ToString(Session["role"]), "admin", StringComparison.OrdinalIgnoreCase)) return "";
            string sql = @"
                SELECT COUNT(*) AS TotalReports,
                    COALESCE(SUM(CASE WHEN status='Pending' THEN 1 ELSE 0 END),0) AS PendingReports,
                    COUNT(DISTINCT CASE WHEN target_type='post' THEN target_id END) AS ReportedPosts,
                    COUNT(DISTINCT CASE WHEN target_type='comment' THEN target_id END) AS ReportedComments
                FROM dbo.ForumReports;
                SELECT status,COUNT(*) AS Total FROM dbo.ForumReports GROUP BY status ORDER BY status;
                SELECT reason,COUNT(*) AS Total FROM dbo.ForumReports GROUP BY reason ORDER BY Total DESC,reason;
                SELECT TOP (10) reportID,target_type,target_id,reason,status,created_at,content_snapshot
                FROM dbo.ForumReports ORDER BY created_at DESC,reportID DESC;";
            var output = new StringBuilder("<section class='report-analytics'>");
            try
            {
                using (SqlConnection con = new SqlConnection(_connStr))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        r.Read();
                        string[] columns = { "TotalReports", "PendingReports", "ReportedPosts", "ReportedComments" };
                        string[] labels = { "TOTAL REPORTS", "PENDING REVIEW", "POSTS REPORTED", "COMMENTS REPORTED" };
                        output.Append("<div class='report-metrics'>");
                        for (int i = 0; i < columns.Length; i++) output.Append("<div><strong>" + r[columns[i]] + "</strong><span>" + labels[i] + "</span></div>");
                        output.Append("</div><div class='report-breakdowns'>");
                        string[] headings = { "Reports by status", "Reports by reason" };
                        for (int i = 0; i < 2; i++)
                        {
                            r.NextResult();
                            var chartLabels = new System.Collections.Generic.List<string>();
                            var chartCounts = new System.Collections.Generic.List<int>();
                            var table = new StringBuilder("<table><thead><tr><th>Category</th><th>Reports</th></tr></thead><tbody>");
                            while (r.Read())
                            {
                                string label = Convert.ToString(r[0]);
                                if (label == "ContentRemoved") label = "Content removed";
                                if (label == "Deleted") label = "Deleted by review";
                                int count = Convert.ToInt32(r["Total"]);
                                chartLabels.Add(label); chartCounts.Add(count);
                                table.Append("<tr><td>" + Server.HtmlEncode(label) + "</td><td>" + count + "</td></tr>");
                            }
                            output.Append("<div class='report-chart-card analytics-card'><div class='chart-title'>" + headings[i] + "</div>");
                            if (chartCounts.Count == 0) output.Append("<p class='report-chart-empty'>No reports yet.</p>");
                            else
                            {
                                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                output.Append("<div class='report-canvas-wrap'><canvas class='report-chart' role='img' aria-label='" + headings[i] + "' data-kind='" + (i == 0 ? "doughnut" : "bar") +
                                    "' data-labels='" + System.Web.HttpUtility.HtmlAttributeEncode(serializer.Serialize(chartLabels)) +
                                    "' data-counts='" + System.Web.HttpUtility.HtmlAttributeEncode(serializer.Serialize(chartCounts)) + "'></canvas></div>");
                                output.Append("<details class='report-data'><summary>View data table</summary>" + table + "</tbody></table></details>");
                            }
                            output.Append("</div>");
                        }
                        output.Append("</div><div class='analytics-card report-recent-card'><div class='chart-title'>Latest 10 reports</div><div class='report-table-scroll' tabindex='0' role='region' aria-label='Recent forum reports'><table><thead><tr><th>ID</th><th>Target</th><th>Reason</th><th>Status</th><th>Submitted</th><th>Content snapshot</th></tr></thead><tbody>");
                        r.NextResult(); bool rows = false;
                        while (r.Read())
                        {
                            rows = true;
                            string snapshot = Convert.ToString(r["content_snapshot"]);
                            if (snapshot.Length > 180) snapshot = snapshot.Substring(0, 180) + "…";
                            output.Append("<tr><td>#" + r["reportID"] + "</td><td>" + Server.HtmlEncode(Convert.ToString(r["target_type"]) + " " + Convert.ToString(r["target_id"])) +
                                "</td><td>" + Server.HtmlEncode(Convert.ToString(r["reason"])) + "</td><td>" + Server.HtmlEncode(Convert.ToString(r["status"])) +
                                "</td><td>" + Convert.ToDateTime(r["created_at"]).ToString("yyyy-MM-dd HH:mm") + "</td><td>" + Server.HtmlEncode(snapshot) + "</td></tr>");
                        }
                        if (!rows) output.Append("<tr><td colspan='6'>No reports have been submitted.</td></tr>");
                        output.Append("</tbody></table></div></div>");
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceError("Report analytics failed: {0}", ex.Number);
                return "<section class='analytics-card'><h3>Forum Reports</h3><p>Report analytics are temporarily unavailable.</p></section>";
            }
            return output.Append("</section>").ToString();
        }

        protected string RenderChapterAttempts()
        {
            if (Session["UserID"] == null || !string.Equals(Convert.ToString(Session["role"]), "admin", StringComparison.OrdinalIgnoreCase)) return "";
            var html = new StringBuilder("<section class='analytics-card'><div class='chart-title'>Self-assessment attempts by chapter</div><p>Recorded student submissions, including repeat attempts.</p><div class='report-table-scroll'><table><thead><tr><th>Chapter</th><th>Attempts</th><th>Students</th><th>Attempts / student</th></tr></thead><tbody>");
            try
            {
                using (var con = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(@"SELECT c.title, COUNT(r.resultID) attempts,COUNT(DISTINCT r.userID) students
                    FROM Chapters c LEFT JOIN AssessmentResults r ON r.chapterID=c.chapterID
                    AND r.userID IN (SELECT userID FROM Users WHERE role='Student')
                    GROUP BY c.chapterID,c.title ORDER BY c.chapterID", con))
                {
                    con.Open(); using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int attempts = Convert.ToInt32(r["attempts"]), students = Convert.ToInt32(r["students"]);
                            html.Append("<tr><td>" + Server.HtmlEncode(Convert.ToString(r["title"])) + "</td><td>" + attempts + "</td><td>" + students + "</td><td>" + (students == 0 ? "—" : ((double)attempts / students).ToString("0.0")) + "</td></tr>");
                        }
                    }
                }
            }
            catch (SqlException ex) { System.Diagnostics.Trace.TraceError("Attempt analytics failed: {0}", ex.Number); html.Append("<tr><td colspan='4'>Attempt data is temporarily unavailable.</td></tr>"); }
            return html.Append("</tbody></table></div></section>").ToString();
        }

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
