using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class SelfAssessment : System.Web.UI.Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        // ================= HELPERS / PROPERTIES =================
        private string CurrentUserId => Session["UserID"]?.ToString();
        private string Role => Session["Role"]?.ToString()?.ToLower();
        private string CurrentChapterId => Request.QueryString["chapter"] ?? "C001";

        private bool IsInEditMode
        {
            get => ViewState["IsInEditMode"] != null && (bool)ViewState["IsInEditMode"];
            set => ViewState["IsInEditMode"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(CurrentUserId)) { Response.Redirect("Login.aspx"); return; }

                string chapterId = CurrentChapterId;

                // Titles Logic
                litBigTitle.Text = "Self Assessment";
                litSubTitle.Text = (chapterId == "C002") ? "Chapter 2" : (chapterId == "C003") ? "Chapter 3" : "Chapter 1";

                // 1. Role-based Visibility
                bool isStaff = (Role == "tutor" || Role == "admin");
                phNormalMode.Visible = isStaff;
                pnlEditControls.Visible = false;

                if (Role == "tutor")
                {
                    // DASHBOARD BUTTON: No alert for tutors
                    btnBackToDash.OnClientClick = "";

                    if (!TutorHasQuestionsInChapter(chapterId, CurrentUserId))
                    {
                        lnkEditMode.Enabled = false;
                        btnSubmit.Visible = false;
                        lnkEditMode.Style.Add("opacity", "0.5");
                        lnkEditMode.ToolTip = "Read-only: You haven't created any questions here.";
                    }
                }
                else if (Role == "admin")
                {
                    // DASHBOARD BUTTON: No alert for admins
                    btnBackToDash.OnClientClick = "";
                }
                else if (Role == "student")
                {
                    // DASHBOARD BUTTON: Show alert ONLY for students
                    btnBackToDash.OnClientClick = "return confirm('Do you want to exit? Your progress will be lost.');";

                    // 2. Student Previous Score Logic
                    int score = GetPreviousScore(CurrentUserId, chapterId);
                    if (score != -1)
                    {
                        ShowCompletedState(score, chapterId);
                    }
                    else
                    {
                        btnSubmit.Visible = true;
                    }
                }

                BindQuestions(chapterId);
            }
        }

        private bool TutorHasQuestionsInChapter(string chapterId, string userId)
        {
            using (SqlConnection con = new SqlConnection(Cs))
            {
                string sql = "SELECT COUNT(*) FROM SelfAssessments WHERE chapterID = @cid AND created_by = @uid AND isDeleted = 0";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@cid", chapterId);
                cmd.Parameters.AddWithValue("@uid", userId);
                con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private void BindQuestions(string chapterId)
        {
            using (SqlConnection con = new SqlConnection(Cs))
            {
                string sql = @"SELECT assID, question, optionA, optionB, optionC, optionD, answer, created_by 
                               FROM SelfAssessments 
                               WHERE chapterID = @cid AND isDeleted = 0 
                               ORDER BY assID ASC";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@cid", chapterId);

                DataTable dt = new DataTable();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);

                rptAssessment.DataSource = dt;
                rptAssessment.DataBind();
            }
        }

        protected void rptAssessment_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // 1. Find Placeholders
                PlaceHolder phAdminDelete = (PlaceHolder)e.Item.FindControl("phAdminDelete");
                PlaceHolder phInputs = (PlaceHolder)e.Item.FindControl("phInputs");
                PlaceHolder phPlainText = (PlaceHolder)e.Item.FindControl("phPlainText");

                // 2. Control General Visibility
                phAdminDelete.Visible = (Role != "student" && IsInEditMode);
                phInputs.Visible = !IsInEditMode;
                phPlainText.Visible = IsInEditMode;

                // 3. Handle Buttons inside phInputs (Individual Edit/Delete)
                if (phInputs.Visible)
                {
                    Button btnEdit = (Button)e.Item.FindControl("btnEditSA");
                    Button btnDelete = (Button)e.Item.FindControl("btnDeleteSA");
                    string createdBy = DataBinder.Eval(e.Item.DataItem, "created_by").ToString();

                    if (Role == "student")
                    {
                        if (btnEdit != null) btnEdit.Visible = false;
                        if (btnDelete != null) btnDelete.Visible = false;
                    }
                    else if (Role == "tutor")
                    {
                        bool isOwner = (createdBy == CurrentUserId);
                        if (btnEdit != null) btnEdit.Visible = isOwner;
                        if (btnDelete != null) btnDelete.Visible = isOwner;
                    }
                    else if (Role == "admin")
                    {
                        if (btnEdit != null) btnEdit.Visible = false;
                        if (btnDelete != null) btnDelete.Visible = true;
                    }

                    // 4. Fill RadioButtonList for Students
                    RadioButtonList rbl = (RadioButtonList)e.Item.FindControl("rblOptions");
                    if (rbl != null)
                    {
                        rbl.Items.Clear();
                        rbl.Items.Add(new ListItem(DataBinder.Eval(e.Item.DataItem, "optionA").ToString(), "A"));
                        rbl.Items.Add(new ListItem(DataBinder.Eval(e.Item.DataItem, "optionB").ToString(), "B"));
                        rbl.Items.Add(new ListItem(DataBinder.Eval(e.Item.DataItem, "optionC").ToString(), "C"));
                        rbl.Items.Add(new ListItem(DataBinder.Eval(e.Item.DataItem, "optionD").ToString(), "D"));

                        // Lock if student already submitted
                        if (Role == "student" && GetPreviousScore(CurrentUserId, CurrentChapterId) != -1)
                        {
                            rbl.Enabled = false;
                        }
                    }
                }

                // 5. Global Submit Button Visibility
                // Hide submit button if in Edit Mode OR if the user is not a student
                btnSubmit.Visible = (Role == "student" && !IsInEditMode);

                // If student already finished, hide it too
                if (Role == "student" && GetPreviousScore(CurrentUserId, CurrentChapterId) != -1)
                {
                    btnSubmit.Visible = false;
                }
            }
        }

        // ================= GLOBAL ACTIONS (NEW) =================

        protected void lnkEditMode_Click(object sender, EventArgs e)
        {
            // 1. SET THE STATE (This is the most important part)
            IsInEditMode = true;

            // 2. Switch Top-Bar Visibility
            phNormalMode.Visible = false;      // Hide "Enter Edit Mode" button
            pnlEditControls.Visible = true;     // Show the full Management Bar
            btnSubmit.Visible = false;          // Hide Student Submit button

            if (Role == "admin")
            {
                btnSaveAll.Visible = false;
            }

            if (Role == "tutor")
            {
                // Check if this tutor actually owns questions here
                bool hasOwnership = TutorHasQuestionsInChapter(CurrentChapterId, CurrentUserId);

                // Disable bulk actions if they don't own questions
                btnDeleteSelected.Enabled = hasOwnership;
                btnSaveAll.Enabled = hasOwnership; // Added this so they can't save if they own nothing

                if (!hasOwnership)
                {
                    btnDeleteSelected.Style.Add("opacity", "0.4");
                    btnDeleteSelected.Style.Add("cursor", "not-allowed");
                    btnDeleteSelected.ToolTip = "You can only delete items you created.";
                }
            }

            // 3. Re-bind to trigger ItemDataBound logic
            BindQuestions(CurrentChapterId);
        }

        protected void btnSaveAll_Click(object sender, EventArgs e)
        {
            // 1. Double-check Role: Admin can't save changes (only delete)
            if (Role == "admin")
            {
                Response.Redirect(Request.RawUrl);
                return;
            }

            using (SqlConnection con = new SqlConnection(Cs))
            {
                con.Open();
                foreach (RepeaterItem item in rptAssessment.Items)
                {
                    // IMPORTANT: Changed "phEdit" to "phPlainText" to match your ASPX
                    PlaceHolder phPlainText = (PlaceHolder)item.FindControl("phPlainText");

                    // Only update if the Edit Mode placeholder exists and is visible
                    if (phPlainText != null && phPlainText.Visible)
                    {
                        // Extract values from the textboxes and dropdown
                        string id = ((HiddenField)item.FindControl("hfAssID")).Value;
                        string q = ((TextBox)item.FindControl("txtEditQ")).Text.Trim();
                        string a = ((TextBox)item.FindControl("txtA")).Text.Trim();
                        string b = ((TextBox)item.FindControl("txtB")).Text.Trim();
                        string c = ((TextBox)item.FindControl("txtC")).Text.Trim();
                        string d = ((TextBox)item.FindControl("txtD")).Text.Trim();
                        string ans = ((DropDownList)item.FindControl("ddlAns")).SelectedValue;

                        // 2. Safety Check: Only update if this tutor is the owner
                        // This prevents malicious users from spoofing IDs
                        string sql = (Role == "tutor")
                            ? @"UPDATE SelfAssessments 
                        SET question=@q, optionA=@a, optionB=@b, optionC=@c, optionD=@d, answer=@ans 
                        WHERE assID=@id AND created_by=@uid"
                            : @"UPDATE SelfAssessments 
                        SET question=@q, optionA=@a, optionB=@b, optionC=@c, optionD=@d, answer=@ans 
                        WHERE assID=@id";

                        using (SqlCommand cmd = new SqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@q", q);
                            cmd.Parameters.AddWithValue("@a", a);
                            cmd.Parameters.AddWithValue("@b", b);
                            cmd.Parameters.AddWithValue("@c", c);
                            cmd.Parameters.AddWithValue("@d", d);
                            cmd.Parameters.AddWithValue("@ans", ans);

                            if (Role == "tutor")
                                cmd.Parameters.AddWithValue("@uid", CurrentUserId);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            // Reset Edit Mode after saving
            IsInEditMode = false;
            Response.Redirect("selfassessment.aspx?chapter=" + CurrentChapterId);
        }

        protected void btnBulkDelete_Click(object sender, EventArgs e)
        {
            // Use your existing helper property CurrentUserId
            string currentUid = CurrentUserId;
            string chapterId = CurrentChapterId;

            using (SqlConnection con = new SqlConnection(Cs))
            {
                con.Open();
                foreach (RepeaterItem item in rptAssessment.Items)
                {
                    // Find the checkbox control
                    var chk = item.FindControl("chkDelete") as HtmlInputCheckBox;

                    if (chk != null && chk.Checked)
                    {
                        // Find the HiddenField for the ID
                        HiddenField hf = (HiddenField)item.FindControl("hfAssID");
                        string qId = hf.Value;

                        string sql = (Role == "admin")
                            ? "UPDATE SelfAssessments SET isDeleted = 1, deleted_at = GETDATE() WHERE assID = @id"
                            : "UPDATE SelfAssessments SET isDeleted = 1, deleted_at = GETDATE() WHERE assID = @id AND created_by = @uid";

                        using (SqlCommand cmd = new SqlCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@id", qId);
                            if (Role != "admin") cmd.Parameters.AddWithValue("@uid", currentUid);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            Response.Redirect("selfassessment.aspx?chapter=" + chapterId);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            // Switch back to Normal State
            phNormalMode.Visible = true;
            pnlEditControls.Visible = false;

            // Show submit button again for students if they haven't finished
            if (Role != "tutor" && Role != "admin")
            {
                btnSubmit.Visible = (GetPreviousScore(CurrentUserId, CurrentChapterId) == -1);
            }

            BindQuestions(CurrentChapterId);
        }
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int score = 0;
            bool allAnswered = true;

            using (SqlConnection con = new SqlConnection(Cs))
            {
                con.Open();
                foreach (RepeaterItem item in rptAssessment.Items)
                {
                    RadioButtonList rbl = (RadioButtonList)item.FindControl("rblOptions");
                    HiddenField hf = (HiddenField)item.FindControl("hfAssID");

                    // CHECK: If any RadioButtonList has no selection, stop the process
                    if (rbl != null && string.IsNullOrEmpty(rbl.SelectedValue))
                    {
                        allAnswered = false;
                        break;
                    }

                    if (rbl != null && hf != null)
                    {
                        SqlCommand cmd = new SqlCommand("SELECT answer FROM SelfAssessments WHERE assID = @id", con);
                        cmd.Parameters.AddWithValue("@id", hf.Value);
                        string correctAnswer = cmd.ExecuteScalar()?.ToString()?.Trim();

                        if (rbl.SelectedValue == correctAnswer)
                        {
                            score++;
                        }
                    }
                }
            }

            if (!allAnswered)
            {
                // This is a backup alert in case JavaScript is disabled
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Please answer all questions before submitting.');", true);
                return;
            }

            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                SaveResult(score, CurrentUserId, CurrentChapterId);
            }
            Response.Redirect(Request.RawUrl);
        }

        private void SaveResult(int score, string uid, string cid)
        {
            using (SqlConnection con = new SqlConnection(Cs))
            {
                con.Open();
                string nextID = GenerateResultID(con);
                string sql = "INSERT INTO AssessmentResults (resultID, chapterID, userID, score) VALUES (@rid, @cid, @uid, @score)";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@rid", nextID);
                    cmd.Parameters.AddWithValue("@cid", cid);
                    cmd.Parameters.AddWithValue("@uid", uid);
                    cmd.Parameters.AddWithValue("@score", score);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GenerateResultID(SqlConnection con)
        {
            string sql = "SELECT TOP 1 resultID FROM AssessmentResults ORDER BY resultID DESC";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) return "AR001";
                int num = int.Parse(result.ToString().Replace("AR", ""));
                return "AR" + (num + 1).ToString("D3");
            }
        }

        private int GetPreviousScore(string uid, string cid)
        {
            using (SqlConnection con = new SqlConnection(Cs))
            {
                string sql = "SELECT MAX(score) FROM AssessmentResults WHERE userID = @uid AND chapterID = @cid";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@uid", uid);
                cmd.Parameters.AddWithValue("@cid", cid);
                con.Open();
                object res = cmd.ExecuteScalar();
                return (res != null && res != DBNull.Value) ? Convert.ToInt32(res) : -1;
            }
        }

        private void ShowCompletedState(int score, string cid)
        {
            // Ensure the Literal exists in ASPX to avoid the "Context" error
            if (litReviewStatus != null)
            {
                litReviewStatus.Text = $@"
            <div class='pill' style='width:100%; justify-content:center; margin-bottom:20px; height:auto; padding:20px; display:block; text-align:center; background:#eff6ff; border-radius:12px;'>
                <div class='pm-name' style='color:#1d4ed8; font-weight:bold;'>{cid} Completed</div>
                <div class='pm-role' style='font-size:18px;'>Best recorded score: {score}</div>
                <p style='margin-top:10px; font-size:13px; color:#64748b;'>You have already submitted this chapter.</p>
            </div>";
            }

            btnSubmit.Visible = false;

            // Hide all Edit UI for students who have finished
            phNormalMode.Visible = false;
            pnlEditControls.Visible = false;
        }

        protected void btnBackDash_Click(object sender, EventArgs e)
        {
            if (Role == "tutor") Response.Redirect("TutorDashboard.aspx");
            else if (Role == "admin") Response.Redirect("AdminDashboard.aspx");
            else Response.Redirect("StudentDashboard.aspx");
        }
    }
}