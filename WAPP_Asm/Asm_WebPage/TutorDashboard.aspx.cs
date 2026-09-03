using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class TutorDashboard : System.Web.UI.Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        private string TutorId =>
            (Session["UserID"] ?? Session["userID"] ?? "").ToString().Trim();

        protected void Page_Load(object sender, EventArgs e)
        {
            GuardTutor();

            if (!IsPostBack)
            {
                BindHeader();
                BindChapters();

                pnlRecentlyDeleted.Visible = false;
            }
        }

        private void GuardTutor()
        {
            string role = (Session["role"] ?? "").ToString().Trim().ToLower();

            if (role != "tutor" || string.IsNullOrWhiteSpace(TutorId))
            {
                Response.Redirect(ResolveUrl("~/Asm_WebPage/Login.aspx"));
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void BindHeader()
        {
            string username = (Session["username"] ?? "Tutor").ToString().Trim();
            litWelcome.Text = "Welcome back, " + Server.HtmlEncode(username) + "!";
            litSubWelcome.Text = "View learning materials and manage your own content.";
        }

        private void BindChapters()
        {
            var list = new List<ChapterCard>();

            const string sql = @"
SELECT 
    c.chapterID,
    c.title,
    c.description,
    COUNT(DISTINCT s.subtopicID) AS SubtopicCount,
    COUNT(DISTINCT pt.userID) AS UniqueLearners,
    COUNT(pt.progressID) AS TotalCompletions
FROM Chapters c
LEFT JOIN Subtopics s 
    ON s.chapterID = c.chapterID
   AND s.isDeleted = 0
   AND s.created_by = @tid
LEFT JOIN ProgressTracking pt 
    ON pt.subtopicID = s.subtopicID
GROUP BY c.chapterID, c.title, c.description
ORDER BY c.chapterID;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@tid", TutorId);
                con.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string chapterId = dr["chapterID"].ToString().Trim();

                        list.Add(new ChapterCard
                        {
                            ChapterId = chapterId,
                            Title = dr["title"]?.ToString() ?? "",
                            Subtitle = dr["description"]?.ToString() ?? "Open learning materials.",
                            SubtopicCount = SafeToInt(dr["SubtopicCount"]),
                            UniqueLearners = SafeToInt(dr["UniqueLearners"]),
                            TotalCompletions = SafeToInt(dr["TotalCompletions"]),
                            Link = ResolveUrl("~/Asm_WebPage/LearningMaterial.aspx?chapter=" +
                                Server.UrlEncode(chapterId)),
                            ImageUrl = GetChapterImage(chapterId),
                        });
                    }
                }
            }

            pnlEmpty.Visible = (list.Count == 0);
            rptTutorChapters.DataSource = list;
            rptTutorChapters.DataBind();
        }

        private int SafeToInt(object v)
        {
            if (v == null || v == DBNull.Value) return 0;
            int n;
            return int.TryParse(v.ToString(), out n) ? n : 0;
        }

        private string GetChapterImage(string chapterId)
        {
            switch ((chapterId ?? "").ToUpper())
            {
                case "C001": return "~/chp1.png";
                case "C002": return "~/chp2.png";
                case "C003": return "~/chp3.png";
                default: return "~/chp1.png";
            }
        }

        protected void btnRecentlyDeleted_Click(object sender, EventArgs e)
        {
            pnlRecentlyDeleted.Visible = !pnlRecentlyDeleted.Visible;

            if (pnlRecentlyDeleted.Visible)
                BindRecentlyDeleted();
        }

        protected void btnCloseDeleted_Click(object sender, EventArgs e)
        {
            pnlRecentlyDeleted.Visible = false;
        }

        private void BindRecentlyDeleted()
        {
            if (string.IsNullOrWhiteSpace(TutorId)) return;

            const string sql = @"
SELECT subtopicID, title, deleted_at
FROM Subtopics
WHERE created_by=@tid AND isDeleted=1
ORDER BY deleted_at DESC;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@tid", TutorId);
                con.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(dr);
                    gvDeleted.DataSource = dt;
                    gvDeleted.DataBind();
                }
            }
        }

        protected void gvDeleted_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RESTORE") return;

            string sid = (e.CommandArgument ?? "").ToString().Trim();
            if (string.IsNullOrWhiteSpace(sid)) return;

            RestoreSubtopicAsTutor(sid, TutorId);

            BindRecentlyDeleted();
            BindChapters();
        }

        private void RestoreSubtopicAsTutor(string subtopicId, string tutorId)
        {
            using (var con = new SqlConnection(Cs))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(@"
UPDATE Subtopics
SET isDeleted=0, deleted_at=NULL, deleted_by=NULL
WHERE subtopicID=@sid AND created_by=@tid AND isDeleted=1;", con, tx))
                        {
                            cmd.Parameters.AddWithValue("@sid", subtopicId);
                            cmd.Parameters.AddWithValue("@tid", tutorId);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd2 = new SqlCommand(@"
UPDATE PracticeQuestions
SET isDeleted=0, deleted_at=NULL, deleted_by=NULL
WHERE subtopicID=@sid;", con, tx))
                        {
                            cmd2.Parameters.AddWithValue("@sid", subtopicId);
                            cmd2.ExecuteNonQuery();
                        }

                        using (var cmd3 = new SqlCommand(@"
                         UPDATE SelfAssessments
                         SET isDeleted=0, deleted_at=NULL, deleted_by=NULL
                         WHERE subtopicID=@sid;", con, tx))
                        {
                            cmd3.Parameters.AddWithValue("@sid", subtopicId);
                            cmd3.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        protected class ChapterCard
        {
            public string ChapterId { get; set; }
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public int SubtopicCount { get; set; }
            public int UniqueLearners { get; set; }
            public int TotalCompletions { get; set; }
            public string Link { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}