using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class TutorUpload : System.Web.UI.Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        private string Role => (Session["role"] ?? "").ToString().Trim().ToLower();
        private string CurrentUserId => (Session["userID"] ?? Session["UserID"] ?? "").ToString().Trim();

        private bool IsTutor => Role == "tutor";
        private bool IsAdmin => Role == "admin";

        private const string SelectChapterText = "Select Chapter";
        private const string SelectSubtopicText = "Select Subtopic";
        private const string SelectAnswerText = "Select";

        protected void Page_Load(object sender, EventArgs e)
        {
            GuardTutorOrAdmin();

            if (!IsPostBack)
            {
                SetMode("material");
                BindAll();
                ApplyRoleUi();
            }
            else
            {
                SetMode((ViewState["UPLOAD_MODE"] ?? "material").ToString());
                ApplyRoleUi();
            }
        }

        private void GuardTutorOrAdmin()
        {
            if ((!IsTutor && !IsAdmin) || string.IsNullOrWhiteSpace(CurrentUserId))
            {
                Response.Redirect(ResolveUrl("~/Asm_WebPage/Login.aspx"));
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void ApplyRoleUi()
        {
            pnlMatCreate.Visible = IsTutor;
            pnlPQCreate.Visible = IsTutor;
            pnlAssCreate.Visible = IsTutor;
        }

        private void SetMode(string mode)
        {
            mode = (mode ?? "material").Trim().ToLower();
            if (mode != "material" && mode != "pq" && mode != "ass") mode = "material";
            ViewState["UPLOAD_MODE"] = mode;

            pnlMaterial.Visible = (mode == "material");
            pnlPQ.Visible = (mode == "pq");
            pnlAss.Visible = (mode == "ass");

            btnTabMaterial.CssClass = "tu-tab" + (mode == "material" ? " active" : "");
            btnTabPQ.CssClass = "tu-tab" + (mode == "pq" ? " active" : "");
            btnTabAss.CssClass = "tu-tab" + (mode == "ass" ? " active" : "");
        }

        protected void btnTabMaterial_Click(object sender, EventArgs e)
        {
            SetMode("material");
            ResetMaterialCreateForm();
            BindChapters();
        }

        protected void btnTabPQ_Click(object sender, EventArgs e)
        {
            SetMode("pq");
            ResetPQCreateForm();
            BindPQDropDowns();
            SetupAnswerDropdowns(); 
        }

        protected void btnTabAss_Click(object sender, EventArgs e)
        {
            SetMode("ass");
            ResetAssCreateForm();
            BindAssChapters();
            SetupAnswerDropdowns(); 
        }

        private void BindAll()
        {
            BindChapters();
            BindPQDropDowns();
            BindAssChapters();
            SetupAnswerDropdowns();
        }
        private void ResetMaterialCreateForm()
        {
            if (!IsTutor) return;

            if (ddlChapter != null && ddlChapter.Items.Count > 0) ddlChapter.SelectedIndex = 0;
            if (txtTitle != null) txtTitle.Text = "";
            if (txtContent != null) txtContent.Text = "";
        }

        private void ResetPQCreateForm()
        {
            if (!IsTutor) return;

            if (ddlPQChapter != null && ddlPQChapter.Items.Count > 0) ddlPQChapter.SelectedIndex = 0;

            if (ddlPQSubtopic != null)
            {
                ddlPQSubtopic.Items.Clear();
                ddlPQSubtopic.Items.Add(new ListItem(SelectSubtopicText, ""));
                ddlPQSubtopic.SelectedIndex = 0;
            }

            if (txtPQ_Q != null) txtPQ_Q.Text = "";
            if (txtPQ_A != null) txtPQ_A.Text = "";
            if (txtPQ_B != null) txtPQ_B.Text = "";
            if (txtPQ_C != null) txtPQ_C.Text = "";
            if (txtPQ_D != null) txtPQ_D.Text = "";

            if (ddlPQ_Ans != null && ddlPQ_Ans.Items.Count > 0) ddlPQ_Ans.SelectedIndex = 0;
        }

        private void ResetAssCreateForm()
        {
            if (!IsTutor) return;

            if (ddlAssChapter != null && ddlAssChapter.Items.Count > 0) ddlAssChapter.SelectedIndex = 0;

            if (txtAssQ != null) txtAssQ.Text = "";
            if (txtAssA != null) txtAssA.Text = "";
            if (txtAssB != null) txtAssB.Text = "";
            if (txtAssC != null) txtAssC.Text = "";
            if (txtAssD != null) txtAssD.Text = "";

            if (ddlAssAns != null && ddlAssAns.Items.Count > 0) ddlAssAns.SelectedIndex = 0;
        }

        private void BindChapters()
        {
            ddlChapter.Items.Clear();
            ddlChapter.Items.Add(new ListItem(SelectChapterText, ""));

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand("SELECT chapterID, title FROM Chapters ORDER BY chapterID;", con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string id = dr["chapterID"].ToString().Trim();
                        string title = dr["title"]?.ToString() ?? "";
                        ddlChapter.Items.Add(new ListItem($"{id} • {title}", id));
                    }
                }
            }

            ddlChapter.SelectedIndex = 0;
        }

        private void BindPQDropDowns()
        {
            ddlPQChapter.Items.Clear();
            ddlPQChapter.Items.Add(new ListItem(SelectChapterText, ""));

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand("SELECT chapterID, title FROM Chapters ORDER BY chapterID;", con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string id = dr["chapterID"].ToString().Trim();
                        string title = dr["title"]?.ToString() ?? "";
                        ddlPQChapter.Items.Add(new ListItem($"{id} • {title}", id));
                    }
                }
            }

            ddlPQChapter.SelectedIndex = 0;

            ddlPQSubtopic.Items.Clear();
            ddlPQSubtopic.Items.Add(new ListItem(SelectSubtopicText, ""));
            ddlPQSubtopic.SelectedIndex = 0;
        }

        protected void ddlPQChapter_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetMode("pq");

            string cid = (ddlPQChapter.SelectedValue ?? "").Trim();
            if (string.IsNullOrWhiteSpace(cid))
            {
                ddlPQSubtopic.Items.Clear();
                ddlPQSubtopic.Items.Add(new ListItem(SelectSubtopicText, ""));
                ddlPQSubtopic.SelectedIndex = 0;
                return;
            }

            BindPQSubtopics(cid);
        }

        private void BindPQSubtopics(string chapterId)
        {
            ddlPQSubtopic.Items.Clear();
            ddlPQSubtopic.Items.Add(new ListItem(SelectSubtopicText, ""));

            string sql = IsAdmin
                ? @"SELECT subtopicID, title FROM Subtopics WHERE chapterID=@cid ORDER BY subtopicID;"
                : @"SELECT subtopicID, title FROM Subtopics WHERE chapterID=@cid AND created_by=@uid ORDER BY subtopicID;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@cid", chapterId);
                if (!IsAdmin) cmd.Parameters.AddWithValue("@uid", CurrentUserId);

                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string sid = dr["subtopicID"].ToString().Trim();
                        string title = dr["title"]?.ToString() ?? sid;
                        ddlPQSubtopic.Items.Add(new ListItem($"{sid} • {title}", sid));
                    }
                }
            }

            ddlPQSubtopic.SelectedIndex = 0;

            if (ddlPQSubtopic.Items.Count == 1)
            {
                ddlPQSubtopic.Items.Clear();
                ddlPQSubtopic.Items.Add(new ListItem(
                    IsAdmin ? "No subtopics found in this chapter." : "No subtopics created by you in this chapter.",
                    ""));
                ddlPQSubtopic.SelectedIndex = 0;
            }
        }

        private void BindAssChapters()
        {
            ddlAssChapter.Items.Clear();
            ddlAssChapter.Items.Add(new ListItem(SelectChapterText, ""));

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand("SELECT chapterID, title FROM Chapters ORDER BY chapterID;", con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string id = dr["chapterID"].ToString().Trim();
                        string title = dr["title"]?.ToString() ?? "";
                        ddlAssChapter.Items.Add(new ListItem($"{id} • {title}", id));
                    }
                }
            }

            ddlAssChapter.SelectedIndex = 0;
        }

        private void SetupAnswerDropdowns()
        {
            EnsureSelectItemFirst(ddlPQ_Ans, SelectAnswerText);
            EnsureSelectItemFirst(ddlAssAns, SelectAnswerText);
        }

        private void EnsureSelectItemFirst(DropDownList ddl, string selectText)
        {
            if (ddl == null) return;
            if (ddl.Items.Count == 0)
            {
                ddl.Items.Add(new ListItem(selectText, ""));
                ddl.SelectedIndex = 0;
                return;
            }

            if (ddl.Items[0].Value == "") return;

            ddl.Items.Insert(0, new ListItem(selectText, ""));
            ddl.SelectedIndex = 0;
        }

        private void Toast(string msg, bool ok)
        {
            msg = (msg ?? "").Replace("\\", "\\\\").Replace("'", "\\'");
            ClientScript.RegisterStartupScript(this.GetType(), Guid.NewGuid().ToString(),
                $"showToast('{msg}', {(ok ? "true" : "false")});", true);
        }

        protected void btnCreateSubtopic_Click(object sender, EventArgs e)
        {
            SetMode("material");

            if (!IsTutor) { Toast("Admin cannot upload learning materials.", false); return; }

            Page.Validate("VG_MAT");
            if (!Page.IsValid) return;

            string chapterId = (ddlChapter.SelectedValue ?? "").Trim();
            if (string.IsNullOrWhiteSpace(chapterId))
            {
                Toast("Please select a chapter.", false);
                return;
            }

            string title = (txtTitle.Text ?? "").Trim();
            string content = (txtContent.Text ?? "").Trim();

            try
            {
                using (var con = new SqlConnection(Cs))
                {
                    con.Open();
                    using (var tx = con.BeginTransaction())
                    {
                        string newSubId = GenerateNextId(con, tx, "Subtopics", "subtopicID", "ST");

                        using (var cmd = new SqlCommand(@"
INSERT INTO Subtopics(subtopicID, chapterID, title, content, created_by)
VALUES(@sid, @cid, @t, @c, @by);", con, tx))
                        {
                            cmd.Parameters.AddWithValue("@sid", newSubId);
                            cmd.Parameters.AddWithValue("@cid", chapterId);
                            cmd.Parameters.AddWithValue("@t", title);
                            cmd.Parameters.AddWithValue("@c", string.IsNullOrWhiteSpace(content) ? (object)DBNull.Value : content);
                            cmd.Parameters.AddWithValue("@by", CurrentUserId);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                }

                ResetMaterialCreateForm();
                Toast("Subtopic created ✅", true);
            }
            catch (Exception ex)
            {
                Toast("Error: " + ex.Message, false);
            }
        }

        protected void btnCreatePQ_Click(object sender, EventArgs e)
        {
            SetMode("pq");

            if (!IsTutor) { Toast("Admin cannot upload practice questions.", false); return; }

            Page.Validate("VG_PQ");
            if (!Page.IsValid) return;

            string cid = (ddlPQChapter.SelectedValue ?? "").Trim();
            if (string.IsNullOrWhiteSpace(cid))
            {
                Toast("Please select a chapter.", false);
                return;
            }

            string subId = (ddlPQSubtopic.SelectedValue ?? "").Trim();
            if (string.IsNullOrWhiteSpace(subId))
            {
                Toast("Please select a subtopic.", false);
                return;
            }

            string q = (txtPQ_Q.Text ?? "").Trim();
            string a = (txtPQ_A.Text ?? "").Trim();
            string b = (txtPQ_B.Text ?? "").Trim();
            string c = (txtPQ_C.Text ?? "").Trim();
            string d = (txtPQ_D.Text ?? "").Trim();
            string ans = (ddlPQ_Ans.SelectedValue ?? "").Trim();

            if (string.IsNullOrWhiteSpace(ans))
            {
                Toast("Please select the correct answer (A-D).", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b) ||
                string.IsNullOrWhiteSpace(c) || string.IsNullOrWhiteSpace(d))
            {
                Toast("Please fill Options A-D.", false);
                return;
            }

            try
            {
                using (var con = new SqlConnection(Cs))
                {
                    con.Open();
                    using (var tx = con.BeginTransaction())
                    {
                        string newId = GenerateNextId(con, tx, "PracticeQuestions", "questionID", "PQ");

                        using (var cmd = new SqlCommand(@"
INSERT INTO PracticeQuestions(questionID, subtopicID, question, optionA, optionB, optionC, optionD, answer, created_by)
VALUES(@id, @sid, @q, @a, @b, @c, @d, @ans, @by);", con, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", newId);
                            cmd.Parameters.AddWithValue("@sid", subId);
                            cmd.Parameters.AddWithValue("@q", q);
                            cmd.Parameters.AddWithValue("@a", a);
                            cmd.Parameters.AddWithValue("@b", b);
                            cmd.Parameters.AddWithValue("@c", c);
                            cmd.Parameters.AddWithValue("@d", d);
                            cmd.Parameters.AddWithValue("@ans", ans);
                            cmd.Parameters.AddWithValue("@by", CurrentUserId);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                }

                ResetPQCreateForm();
                Toast("Practice question added ✅", true);
            }
            catch (Exception ex)
            {
                Toast("Error: " + ex.Message, false);
            }
        }
        protected void btnCreateAss_Click(object sender, EventArgs e)
        {
            SetMode("ass");

            if (!IsTutor) { Toast("Admin cannot upload self-assessment.", false); return; }

            Page.Validate("VG_ASS");
            if (!Page.IsValid) return;

            string cid = (ddlAssChapter.SelectedValue ?? "").Trim();
            if (string.IsNullOrWhiteSpace(cid))
            {
                Toast("Please select a chapter.", false);
                return;
            }

            string q = (txtAssQ.Text ?? "").Trim();
            string a = (txtAssA.Text ?? "").Trim();
            string b = (txtAssB.Text ?? "").Trim();
            string c = (txtAssC.Text ?? "").Trim();
            string d = (txtAssD.Text ?? "").Trim();
            string ans = (ddlAssAns.SelectedValue ?? "").Trim();

            if (string.IsNullOrWhiteSpace(ans))
            {
                Toast("Please select the correct answer (A-D).", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b) ||
                string.IsNullOrWhiteSpace(c) || string.IsNullOrWhiteSpace(d))
            {
                Toast("Please fill Options A-D.", false);
                return;
            }

            try
            {
                using (var con = new SqlConnection(Cs))
                {
                    con.Open();
                    using (var tx = con.BeginTransaction())
                    {
                        string newId = GenerateNextId(con, tx, "SelfAssessments", "assID", "SA");

                        using (var cmd = new SqlCommand(@"
INSERT INTO SelfAssessments(assID, chapterID, question, optionA, optionB, optionC, optionD, answer, created_by)
VALUES(@id, @cid, @q, @a, @b, @c, @d, @ans, @by);", con, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", newId);
                            cmd.Parameters.AddWithValue("@cid", cid);
                            cmd.Parameters.AddWithValue("@q", q);
                            cmd.Parameters.AddWithValue("@a", a);
                            cmd.Parameters.AddWithValue("@b", b);
                            cmd.Parameters.AddWithValue("@c", c);
                            cmd.Parameters.AddWithValue("@d", d);
                            cmd.Parameters.AddWithValue("@ans", ans);
                            cmd.Parameters.AddWithValue("@by", CurrentUserId);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                }

                ResetAssCreateForm();
                Toast("Self-assessment added ✅", true);
            }
            catch (Exception ex)
            {
                Toast("Error: " + ex.Message, false);
            }
        }
        private string GenerateNextId(SqlConnection con, SqlTransaction tx, string table, string pkCol, string prefix)
        {
            string sql = $@"
SELECT ISNULL(MAX(CAST(SUBSTRING({pkCol}, {prefix.Length + 1}, 10) AS INT)), 0)
FROM {table}
WHERE {pkCol} LIKE '{prefix}%';";

            int next;
            using (var cmd = new SqlCommand(sql, con, tx))
                next = Convert.ToInt32(cmd.ExecuteScalar()) + 1;

            return prefix + next.ToString("D3");
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(ResolveUrl("~/Asm_WebPage/TutorDashboard.aspx"));
        }
    }
}