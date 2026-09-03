using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class RecentlyDeleted : System.Web.UI.Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        private string Role => (Session["role"] ?? "").ToString().Trim().ToLower();

        private string UserId =>
            (Session["UserID"] ?? Session["userID"] ?? "").ToString().Trim();

        protected void Page_Load(object sender, EventArgs e)
        {
            Guard();

            if (!IsPostBack)
            {
                BindAll();
            }
        }

        private void Guard()
        {
            if ((Role != "tutor" && Role != "admin") || string.IsNullOrWhiteSpace(UserId))
            {
                Response.Redirect(ResolveUrl("~/Asm_WebPage/Login.aspx"));
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e) => BindAll();

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            BindAll();
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            if (Role == "admin")
            {
                Response.Redirect("~/Asm_WebPage/AdminDashboard.aspx");
            }
            else
            {
                Response.Redirect("~/Asm_WebPage/TutorDashboard.aspx");
            }
        }

        private void BindAll()
        {
            BindDeletedSubtopics();
            BindDeletedPQ();
            BindDeletedSA();
        }

        private void BindDeletedSubtopics()
        {
            string q = (txtSearch.Text ?? "").Trim();

            string sqlTutor = @"
SELECT s.subtopicID, s.title, s.deleted_at
FROM Subtopics s
WHERE s.isDeleted=1
  AND s.deleted_at >= DATEADD(day,-30,GETDATE())
  AND s.created_by = @uid
  AND (@q='' OR s.subtopicID LIKE '%' + @q + '%' OR s.title LIKE '%' + @q + '%')
ORDER BY s.deleted_at DESC;";

            string sqlAdmin = @"
SELECT s.subtopicID, s.title, s.deleted_at
FROM Subtopics s
WHERE s.isDeleted=1
  AND s.deleted_at >= DATEADD(day,-30,GETDATE())
  AND (@q='' OR s.subtopicID LIKE '%' + @q + '%' OR s.title LIKE '%' + @q + '%')
ORDER BY s.deleted_at DESC;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(Role == "admin" ? sqlAdmin : sqlTutor, con))
            {
                cmd.Parameters.AddWithValue("@uid", UserId);
                cmd.Parameters.AddWithValue("@q", q);
                con.Open();

                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                gvSubtopics.DataSource = dt;
                gvSubtopics.DataBind();
            }
        }

        private void BindDeletedPQ()
        {
            string q = (txtSearch.Text ?? "").Trim();

            string sqlTutor = @"
SELECT pq.subtopicID, pq.question, pq.deleted_at
FROM PracticeQuestions pq
INNER JOIN Subtopics s ON s.subtopicID = pq.subtopicID
WHERE pq.isDeleted=1
  AND pq.deleted_at >= DATEADD(day,-30,GETDATE())
  AND s.created_by=@uid
  AND (@q='' OR pq.subtopicID LIKE '%' + @q + '%' OR pq.question LIKE '%' + @q + '%')
ORDER BY pq.deleted_at DESC;";

            string sqlAdmin = @"
SELECT pq.subtopicID, pq.question, pq.deleted_at
FROM PracticeQuestions pq
WHERE pq.isDeleted=1
  AND pq.deleted_at >= DATEADD(day,-30,GETDATE())
  AND (@q='' OR pq.subtopicID LIKE '%' + @q + '%' OR pq.question LIKE '%' + @q + '%')
ORDER BY pq.deleted_at DESC;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(Role == "admin" ? sqlAdmin : sqlTutor, con))
            {
                cmd.Parameters.AddWithValue("@uid", UserId);
                cmd.Parameters.AddWithValue("@q", q);
                con.Open();

                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                gvPQ.DataSource = dt;
                gvPQ.DataBind();
            }
        }

        private void BindDeletedSA()
        {
            string q = (txtSearch.Text ?? "").Trim();

            string sqlTutor = @"
        SELECT sa.assID, sa.question, sa.deleted_at
        FROM SelfAssessments sa
        WHERE sa.isDeleted=1
          AND sa.deleted_at >= DATEADD(day,-30,GETDATE())
          AND sa.created_by = @uid
          AND (@q='' OR sa.assID LIKE '%' + @q + '%' OR sa.question LIKE '%' + @q + '%')
        ORDER BY sa.deleted_at DESC;";

            string sqlAdmin = @"
        SELECT sa.assID, sa.question, sa.deleted_at
        FROM SelfAssessments sa
        WHERE sa.isDeleted=1
          AND sa.deleted_at >= DATEADD(day,-30,GETDATE())
          AND (@q='' OR sa.assID LIKE '%' + @q + '%' OR sa.question LIKE '%' + @q + '%')
        ORDER BY sa.deleted_at DESC;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(Role == "admin" ? sqlAdmin : sqlTutor, con))
            {
                cmd.Parameters.AddWithValue("@uid", UserId);
                cmd.Parameters.AddWithValue("@q", q);
                con.Open();

                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                gvSA.DataSource = dt;
                gvSA.DataBind();
            }
        }

        protected void gvSubtopics_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RESTORE_SUB") return;
            string sid = (e.CommandArgument ?? "").ToString().Trim();
            if (string.IsNullOrWhiteSpace(sid)) return;

            if (Role == "admin") RestoreSubtopicOnly_Admin(sid);
            else RestoreSubtopicOnly_Tutor(sid, UserId);

            BindAll();
        }

        protected void gvPQ_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RESTORE_PQ") return;
            string sid = (e.CommandArgument ?? "").ToString().Trim();
            if (string.IsNullOrWhiteSpace(sid)) return;

            if (Role == "admin") RestorePQOnly(sid);
            else
            {
                if (!TutorOwnsSubtopicEvenIfDeleted(sid, UserId)) return;
                RestorePQOnly(sid);
            }

            BindAll();
        }

        protected void gvSA_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RESTORE_SA") return;
            string aid = (e.CommandArgument ?? "").ToString().Trim();
            if (string.IsNullOrWhiteSpace(aid)) return;

            RestoreSA(aid);
            BindAll();
        }

        private void RestoreSA(string assID)
        {
            // Make sure this matches your Session variable perfectly!
            string currentUid = (Session["UserID"] ?? Session["userID"] ?? "").ToString().Trim();

            string sql = @"
        UPDATE SelfAssessments
        SET isDeleted=0, deleted_at=NULL
        WHERE assID=@aid " + (Role == "admin" ? "" : " AND created_by=@uid") + ";";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@aid", assID);
                if (Role != "admin") cmd.Parameters.AddWithValue("@uid", currentUid);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private bool TutorOwnsSubtopicEvenIfDeleted(string subtopicId, string tutorId)
        {
            const string sql = @"SELECT COUNT(1) FROM Subtopics WHERE subtopicID=@sid AND created_by=@tid;";
            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                cmd.Parameters.AddWithValue("@tid", tutorId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
        private void RestoreSubtopicOnly_Tutor(string subtopicId, string tutorId)
        {
            const string sql = @"
UPDATE Subtopics
SET isDeleted=0, deleted_at=NULL, deleted_by=NULL
WHERE subtopicID=@sid AND created_by=@tid AND isDeleted=1;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                cmd.Parameters.AddWithValue("@tid", tutorId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void RestoreSubtopicOnly_Admin(string subtopicId)
        {
            const string sql = @"
UPDATE Subtopics
SET isDeleted=0, deleted_at=NULL, deleted_by=NULL
WHERE subtopicID=@sid AND isDeleted=1;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void RestorePQOnly(string subtopicId)
        {
            const string sql = @"
UPDATE PracticeQuestions
SET isDeleted=0, deleted_at=NULL
WHERE subtopicID=@sid AND isDeleted=1;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}