using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Web.Optimization;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class LearningMaterial : System.Web.UI.Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        protected string CurrentChapterId = "C001";
        protected string CurrentSubtopicId = "";

        private string Role => (Session["role"] ?? "nonreg_student").ToString().Trim().ToLower();

        private string CurrentUserId =>
            (Session["UserID"] ?? Session["userID"] ?? Session["studentID"] ?? "").ToString().Trim();

        protected void Page_Load(object sender, EventArgs e)
        {
            CurrentChapterId = (Request.QueryString["chapter"] ?? "C001").Trim();
            CurrentSubtopicId = (Request.QueryString["sub"] ?? "").Trim();

            pnlTutorActions.Visible = (Role == "tutor");
            pnlAdminDelete.Visible = (Role == "admin");

            if (!AllowAccessOrRedirect(CurrentChapterId)) return;

            if (string.IsNullOrEmpty(CurrentSubtopicId))
                CurrentSubtopicId = GetFirstSubtopicId(CurrentChapterId);

            if (!IsSubtopicInChapter(CurrentChapterId, CurrentSubtopicId))
                CurrentSubtopicId = GetFirstSubtopicId(CurrentChapterId);

            lnkSelf.NavigateUrl = ResolveUrl("~/Asm_WebPage/SelfAssessment.aspx?chapter=" +
                Server.UrlEncode(CurrentChapterId));

            if (!IsPostBack)
            {
                BindChapterTitle(CurrentChapterId);
                BindSidebarSubtopics(CurrentChapterId, CurrentSubtopicId);
                BindSubtopicContent(CurrentSubtopicId);

                BindPracticeQuestion(CurrentSubtopicId);
                BindPrevNext(CurrentChapterId, CurrentSubtopicId);

                pnlEdit.Visible = false;
                pnlDisplay.Visible = true;
            }

            ApplyTutorOwnershipUiGuard();
            RegisterTryPanelScript();
        }

        //private void BindPrevNext(string chapterId, string currentSubId)
        //{
        //    lnkPrev.Visible = false;
        //    lnkNext.Visible = false;

        //    chapterId = (chapterId ?? "").Trim();
        //    currentSubId = (currentSubId ?? "").Trim();


        //    var ids = GetSubtopicIdsForChapter(chapterId);
        //    int idx = ids.FindIndex(x => string.Equals(x, currentSubId, StringComparison.OrdinalIgnoreCase));

        //    if (idx < 0) idx = 0;

        //    if (idx > 0)
        //    {
        //        string prevId = ids[idx - 1];
        //        lnkPrev.Visible = true;
        //        lnkPrev.NavigateUrl = BuildLessonUrl(chapterId, prevId);
        //        lnkPrev.Text = "&lt;";
        //    }

        //    lnkNext.Visible = true;
        //    if (idx < ids.Count - 1)
        //    {
        //        // Normal case: Proceed to next subtopic
        //        lnkNext.NavigateUrl = BuildLessonUrl(chapterId, ids[idx + 1]);
        //    }
        //    else
        //    {
        //        // Final case: Check if they can enter the Self-Assessment
        //        if (IsPracticeComplete(currentSubId, CurrentUserId))
        //        {
        //            // ACCESS GRANTED
        //            lnkNext.NavigateUrl = BuildSelfAssessmentUrl(chapterId);
        //            lnkNext.Attributes.Remove("style");
        //        }
        //        else
        //        {
        //            // ACCESS DENIED
        //            lnkNext.NavigateUrl = "javascript:alert('Finish all practice questions to unlock!');";
        //            lnkNext.Attributes["style"] = "opacity: 0.5; cursor: not-allowed;";
        //        }
        //    }
        //}

        //private void BindPrevNext(string chapterId, string currentSubId)
        //{
        //    // 1. Get IDs and current User
        //    var ids = GetSubtopicIdsForChapter(chapterId);
        //    int idx = ids.FindIndex(x => string.Equals(x, currentSubId, StringComparison.OrdinalIgnoreCase));
        //    string uid = CurrentUserId;

        //    // 2. Determine if Practice is Complete for the current page
        //    bool isComplete = IsPracticeComplete(currentSubId, CurrentUserId);

        //    // --- LOGIC FOR TOP BAR SELF-ASSESSMENT LINK ---
        //    // We only unlock this if the student is on the LAST subtopic AND finished it
        //    bool isLastSubtopic = (idx == ids.Count - 1);

        //    if (isLastSubtopic && isComplete)
        //    {
        //        lnkSelf.NavigateUrl = BuildSelfAssessmentUrl(chapterId);
        //        lnkSelf.Attributes.Remove("style");
        //        lnkSelf.ToolTip = "Unlock: Start Assessment";
        //    }
        //    else
        //    {
        //        lnkSelf.NavigateUrl = "javascript:alert('You must complete all subtopics and practice questions to unlock the Self-Assessment!');";
        //        lnkSelf.Attributes["style"] = "opacity: 0.5; cursor: not-allowed;";
        //        lnkSelf.ToolTip = "Locked: Complete all practice questions first";
        //    }

        //    // --- LOGIC FOR THE ">" NEXT BUTTON ---
        //    lnkNext.Visible = true;
        //    if (idx < ids.Count - 1)
        //    {
        //        // Go to next subtopic
        //        lnkNext.NavigateUrl = BuildLessonUrl(chapterId, ids[idx + 1]);
        //        lnkNext.Attributes.Remove("style");
        //        lnkNext.Text = "&gt;";
        //    }
        //    else
        //    {
        //        // On the last subtopic, it points to Self-Assessment
        //        if (isComplete)
        //        {
        //            lnkNext.NavigateUrl = BuildSelfAssessmentUrl(chapterId);
        //            lnkNext.Attributes.Remove("style");
        //            lnkNext.Text = "&gt;";
        //        }
        //        else
        //        {
        //            lnkNext.NavigateUrl = "javascript:alert('Finish all practice questions correctly to unlock!');";
        //            lnkNext.Attributes["style"] = "opacity: 0.5; cursor: not-allowed;";
        //            lnkNext.Text = "🔒"; 
        //        }
        //    }

        //    // Handle Previous Button (Usually always unlocked)
        //    if (idx > 0)
        //    {
        //        lnkPrev.Visible = true;
        //        lnkPrev.NavigateUrl = BuildLessonUrl(chapterId, ids[idx - 1]);
        //    }
        //}

        //private void BindPrevNext(string chapterId, string currentSubId)
        //{
        //    var ids = GetSubtopicIdsForChapter(chapterId);
        //    int idx = ids.FindIndex(x => string.Equals(x, currentSubId, StringComparison.OrdinalIgnoreCase));
        //    string uid = CurrentUserId;
        //    bool isComplete = IsPracticeComplete(chapterId, CurrentUserId); // Checks ProgressTracking table

        //    // --- 1. Handle the TOP BAR Self-Assessment Link ---
        //    bool isOnLastSubtopic = (idx == ids.Count - 1);

        //    if (isOnLastSubtopic && isComplete)
        //    {
        //        // Unlocked only if they are at the end AND finished practice
        //        lnkSelf.NavigateUrl = BuildSelfAssessmentUrl(chapterId);
        //        lnkSelf.Attributes.Remove("style");
        //        lnkSelf.ToolTip = "Start Self-Assessment";
        //    }
        //    else
        //    {
        //        // Locked
        //        lnkSelf.NavigateUrl = "javascript:alert('Finish all practice questions on the final subtopic to unlock!');";
        //        lnkSelf.Attributes["style"] = "opacity: 0.5; cursor: not-allowed;";
        //        lnkSelf.ToolTip = "Locked";
        //    }

        //    // --- 2. Handle the NEXT (>) Button ---
        //    lnkNext.Visible = true;
        //    if (idx < ids.Count - 1)
        //    {
        //        // Case: There is another subtopic. 
        //        // We usually allow moving to the next subtopic freely.
        //        lnkNext.NavigateUrl = BuildLessonUrl(chapterId, ids[idx + 1]);
        //        lnkNext.Text = "&gt;";
        //        lnkNext.Attributes.Remove("style");
        //    }
        //    else
        //    {
        //        // Case: This is the LAST subtopic. Pointing to Self-Assessment.
        //        if (isComplete)
        //        {
        //            lnkNext.NavigateUrl = BuildSelfAssessmentUrl(chapterId);
        //            lnkNext.Text = "&gt;";
        //            lnkNext.Attributes.Remove("style");
        //        }
        //        else
        //        {
        //            lnkNext.NavigateUrl = "javascript:alert('You must complete the practice questions to unlock the Self-Assessment!');";
        //            lnkNext.Text = "🔒";
        //            lnkNext.Attributes["style"] = "opacity: 0.5; cursor: not-allowed;";
        //        }
        //    }

        //    // --- 3. Handle PREVIOUS Button ---
        //    if (idx > 0)
        //    {
        //        lnkPrev.Visible = true;
        //        lnkPrev.NavigateUrl = BuildLessonUrl(chapterId, ids[idx - 1]);
        //    }
        //}

        private void BindPrevNext(string chapterId, string currentSubId)
        {
            var ids = GetSubtopicIdsForChapter(chapterId);
            int idx = ids.FindIndex(x => string.Equals(x, currentSubId, StringComparison.OrdinalIgnoreCase));

            // Check if the WHOLE chapter is done for this student
            bool isChapterDone = IsPracticeComplete(chapterId, CurrentUserId);

            string userRole = Session["role"]?.ToString();
            if (userRole == "admin" || userRole == "tutor")
            {
                isChapterDone = true;
            }

            //string assessmentUrl = BuildSelfAssessmentUrl(chapterId);

            // --- 1. Top Bar Link (Self Assessment) ---
            if (isChapterDone)
            {
                lnkSelf.NavigateUrl = BuildSelfAssessmentUrl(chapterId);
                lnkSelf.Attributes.Remove("style");
                lnkSelf.ToolTip = "Start Assessment";
            }
            else
            {
                lnkSelf.NavigateUrl = "javascript:alert('Finish all practice questions in all subtopics to unlock!');";
                lnkSelf.Attributes["style"] = "opacity: 0.5; cursor: not-allowed; text-decoration: none;";
            }

            // --- 2. Next Button Logic ---
            lnkNext.Visible = true;
            if (idx < ids.Count - 1)
            {
                // Not on the last subtopic: Just move to the next subtopic (1.1 -> 1.2)
                lnkNext.NavigateUrl = BuildLessonUrl(chapterId, ids[idx + 1]);
                lnkNext.Text = "&gt;";
                lnkNext.Attributes.Remove("style");
            }
            else
            {
                // ON THE LAST SUBTOPIC: Next button points to Self-Assessment
                if (isChapterDone)
                {
                    lnkNext.NavigateUrl = BuildSelfAssessmentUrl(chapterId);
                    lnkNext.Text = "&gt;";
                    lnkNext.Attributes.Remove("style");
                }
                else
                {
                    // LOCKED state for the final gate
                    lnkNext.NavigateUrl = "javascript:alert('Complete the final practice question to unlock the Self-Assessment!');";
                    lnkNext.Text = "🔒";
                    lnkNext.Attributes["style"] = "opacity: 0.5; cursor: not-allowed;";
                }
            }

            // --- 3. Handle PREVIOUS Button ---
            if (idx > 0)
            {
                lnkPrev.Visible = true;
                lnkPrev.NavigateUrl = BuildLessonUrl(chapterId, ids[idx - 1]);
            }
        }

        //private bool IsPracticeComplete(string CurrentSubtopicId, string CurrentUserId)
        //{
        //    string sid = CurrentSubtopicId;
        //    string uid = CurrentUserId;

        //    if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(uid)) return false;

        //    using (SqlConnection con = new SqlConnection(Cs))
        //    {
        //        // Logic: Total active questions vs. how many the student has 'tracked'
        //        string sql = @"
        //    SELECT 
        //        (SELECT COUNT(*) FROM PracticeQuestions WHERE subtopicID = @sid AND isDeleted = 0) as TotalNeeded,
        //        (SELECT COUNT(DISTINCT progressID) FROM ProgressTracking WHERE userID = @uid AND subtopicID = @sid) as CompletedCount";

        //        using (SqlCommand cmd = new SqlCommand(sql, con))
        //        {
        //            cmd.Parameters.AddWithValue("@sid", sid);
        //            cmd.Parameters.AddWithValue("@uid", uid);
        //            con.Open();

        //            using (SqlDataReader rdr = cmd.ExecuteReader())
        //            {
        //                if (rdr.Read())
        //                {
        //                    int total = Convert.ToInt32(rdr["TotalNeeded"]);
        //                    int completed = Convert.ToInt32(rdr["CompletedCount"]);

        //                    // If there are no questions at all, it's unlocked (total == 0)
        //                    // Otherwise, they must have a record for every question
        //                    return (total > 0) ? (completed >= total) : true;
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}

        //private bool IsPracticeComplete(string CurrentSubtopicId, string CurrentUserId)
        //{

        //    string sid = CurrentSubtopicId;
        //    string uid = CurrentUserId;
        //    using (SqlConnection con = new SqlConnection(Cs))
        //    {
        //        string sql = @"
        //    SELECT 
        //        (SELECT COUNT(*) FROM PracticeQuestions WHERE subtopicID = @sid AND isDeleted = 0) as TotalNeeded,
        //        (SELECT COUNT(DISTINCT progressID) FROM ProgressTracking WHERE userID = @uid AND subtopicID = @sid) as CompletedCount";

        //        SqlCommand cmd = new SqlCommand(sql, con);
        //        cmd.Parameters.AddWithValue("@sid", CurrentSubtopicId);
        //        cmd.Parameters.AddWithValue("@uid", CurrentUserId);
        //        con.Open();
        //        using (SqlDataReader rdr = cmd.ExecuteReader())
        //        {
        //            if (rdr.Read())
        //            {
        //                int total = Convert.ToInt32(rdr["TotalNeeded"]);
        //                int completed = Convert.ToInt32(rdr["CompletedCount"]);

        //                // If no questions exist, it's considered complete.
        //                if (total == 0) return true;

        //                return completed >= total;
        //            }
        //        }
        //    }
        //    return false;
        //}

        //private bool IsPracticeComplete(string CurrentSubtopicId, string CurrentUserId)
        //{
        //    if (string.IsNullOrEmpty(CurrentSubtopicId) || string.IsNullOrEmpty(CurrentUserId))
        //        return false;

        //    using (SqlConnection con = new SqlConnection(Cs))
        //    {
        //        // We just check if a record exists for this user and subtopic
        //        string sql = "SELECT COUNT(*) FROM ProgressTracking WHERE userID = @uid AND subtopicID = @sid";

        //        SqlCommand cmd = new SqlCommand(sql, con);
        //        cmd.Parameters.AddWithValue("@sid", CurrentSubtopicId);
        //        cmd.Parameters.AddWithValue("@uid", CurrentUserId);

        //        con.Open();
        //        int count = (int)cmd.ExecuteScalar();

        //        // If count > 0, they have finished the practice for this subtopic
        //        return count > 0;
        //    }
        //}

        private bool IsPracticeComplete(string chapterId, string userId)
        {

            if (string.IsNullOrEmpty(chapterId) || string.IsNullOrEmpty(userId))
                return false;
            using (SqlConnection con = new SqlConnection(Cs))
            {
                // SQL Logic:
                // 1. TotalSubtopics: Count all subtopics in this chapter.
                // 2. UserFinishedSubtopics: Count rows in ProgressTracking where the subtopic belongs to this chapter.
                string sql = @"
            SELECT 
                (SELECT COUNT(*) FROM Subtopics WHERE chapterID = @cid) as TotalSubtopics,
                (SELECT COUNT(DISTINCT pt.subtopicID) 
                 FROM ProgressTracking pt
                 INNER JOIN Subtopics s ON pt.subtopicID = s.subtopicID
                 WHERE s.chapterID = @cid AND pt.userID = @uid) as UserFinishedSubtopics";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@cid", chapterId);
                cmd.Parameters.AddWithValue("@uid", userId);

                con.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        int total = Convert.ToInt32(rdr["TotalSubtopics"]);
                        int finished = Convert.ToInt32(rdr["UserFinishedSubtopics"]);

                        // If Chapter 1 has 3 subtopics, user must have all 3 in ProgressTracking
                        return (total > 0) && (finished >= total);
                    }
                }
            }
            return false;
        }

        private List<string> GetSubtopicIdsForChapter(string chapterId)
        {
            var list = new List<string>();
            const string sql = @"SELECT subtopicID
                                 FROM Subtopics
                                 WHERE chapterID=@cid AND isDeleted=0
                                 ORDER BY subtopicID;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@cid", chapterId);
                con.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        list.Add(dr["subtopicID"].ToString().Trim());
                }
            }

            return list;
        }

        private string BuildLessonUrl(string chapterId, string subtopicId)
        {
            return ResolveUrl("~/Asm_WebPage/LearningMaterial.aspx?chapter=" +
                              Server.UrlEncode(chapterId) +
                              "&sub=" + Server.UrlEncode(subtopicId));
        }

        private string BuildSelfAssessmentUrl(string chapterId)
        {
            return ResolveUrl("~/Asm_WebPage/SelfAssessment.aspx?chapter=" +
                              Server.UrlEncode(chapterId));
        }

        private bool AllowAccessOrRedirect(string chapterId)
        {
            if (Role == "nonreg_student" &&
                !string.Equals(chapterId, "C001", StringComparison.OrdinalIgnoreCase))
            {
                ClientScript.RegisterStartupScript(
                    this.GetType(),
                    "signup",
                    "alert('Please sign up to unlock remaining chapters.'); window.location='" +
                    ResolveUrl("~/Asm_WebPage/StudentDashboard.aspx") + "';",
                    true
                );

                Context.ApplicationInstance.CompleteRequest();
                return false;
            }

            return true;
        }

        private string GetFirstSubtopicId(string chapterId)
        {
            const string sql = @"SELECT TOP 1 subtopicID
                                 FROM Subtopics
                                 WHERE chapterID=@cid AND isDeleted=0
                                 ORDER BY subtopicID;";
            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@cid", chapterId);
                con.Open();
                object r = cmd.ExecuteScalar();
                return r == null ? "" : r.ToString().Trim();
            }
        }

        private bool IsSubtopicInChapter(string chapterId, string subtopicId)
        {
            if (string.IsNullOrEmpty(subtopicId)) return false;

            const string sql = @"SELECT COUNT(1)
                                 FROM Subtopics
                                 WHERE chapterID=@cid AND subtopicID=@sid AND isDeleted=0;";
            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@cid", chapterId);
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                con.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private void BindChapterTitle(string chapterId)
        {
            const string sql = @"SELECT title FROM Chapters WHERE chapterID=@cid;";
            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@cid", chapterId);
                con.Open();
                object r = cmd.ExecuteScalar();
                lblChapterTitle.Text = (r == null) ? chapterId : r.ToString().ToUpper();
            }
        }

        private void BindSidebarSubtopics(string chapterId, string activeSubtopicId)
        {
            const string sql = @"SELECT subtopicID, title
                                 FROM Subtopics
                                 WHERE chapterID=@cid AND isDeleted=0
                                 ORDER BY subtopicID;";

            var list = new List<SubNav>();

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@cid", chapterId);
                con.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string sid = dr["subtopicID"].ToString().Trim();
                        string rawTitle = dr["title"].ToString();
                        string clean = StripLeadingNumber(rawTitle);

                        list.Add(new SubNav
                        {
                            ChapterId = chapterId,
                            SubtopicId = sid,
                            DisplayNo = ExtractLeadingNumber(rawTitle),
                            CleanTitle = clean,
                            IsActive = string.Equals(sid, activeSubtopicId, StringComparison.OrdinalIgnoreCase)
                        });
                    }
                }
            }

            rptSubtopics.DataSource = list;
            rptSubtopics.DataBind();
        }

        private void BindSubtopicContent(string subtopicId)
        {
            if (string.IsNullOrEmpty(subtopicId))
            {
                lblSubtopicTitle.Text = "No subtopic selected";
                litContent.Text = "No content.";
                ViewState["RAW_CONTENT"] = "";
                return;
            }

            const string sql = @"SELECT title, content
                                 FROM Subtopics
                                 WHERE subtopicID=@sid AND isDeleted=0;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                con.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        lblSubtopicTitle.Text = "Subtopic Not Found";
                        litContent.Text = "No content available.";
                        ViewState["RAW_CONTENT"] = "";
                        return;
                    }

                    string title = StripLeadingNumber(dr["title"].ToString());
                    lblSubtopicTitle.Text = title;

                    string raw = dr["content"] == DBNull.Value ? "" : dr["content"].ToString();

                    string h3Title1 = $"<h3>{title}</h3>";
                    string h3Title2 = $"<h3>{Server.HtmlEncode(title)}</h3>";

                    if (raw.TrimStart().StartsWith(h3Title1, StringComparison.OrdinalIgnoreCase))
                        raw = raw.TrimStart().Substring(h3Title1.Length).TrimStart();
                    else if (raw.TrimStart().StartsWith(h3Title2, StringComparison.OrdinalIgnoreCase))
                        raw = raw.TrimStart().Substring(h3Title2.Length).TrimStart();

                    litContent.Text = string.IsNullOrWhiteSpace(raw) ? "<p>No content yet.</p>" : raw;
                    ViewState["RAW_CONTENT"] = raw;
                }
            }
        }

        private bool IsGuestRole()
        {
            return Role == "nonreg_student" || Role == "guest";
        }

        private void BindPracticeQuestion(string subtopicId)
        {
            pnlPractice.Visible = false;

            if (lblResult != null)
            {
                lblResult.Text = "";
                lblResult.CssClass = "result-label";
            }
            rblOptions.Items.Clear();
            ViewState["ANS"] = "";

            if (Role != "student" && !IsGuestRole() && Role != "tutor" && Role != "admin") return;

            const string sql = @"SELECT TOP 1 questionID, question, optionA, optionB, optionC, optionD, answer
                         FROM PracticeQuestions
                         WHERE subtopicID=@sid AND isDeleted=0
                         ORDER BY questionID;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                con.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return;

                    pnlPractice.Visible = true;

                    lblQ.Text = dr["question"].ToString();
                    ViewState["ANS"] = dr["answer"].ToString().Trim();

                    AddOption("A", dr["optionA"]?.ToString());
                    AddOption("B", dr["optionB"]?.ToString());
                    AddOption("C", dr["optionC"]?.ToString());
                    AddOption("D", dr["optionD"]?.ToString());

                    rblOptions.ClearSelection();

                    btnSubmit.Visible = true;
                }
            }
        }

        private void AddOption(string key, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            rblOptions.Items.Add(new ListItem(text, key));
        }
        
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            pnlPractice.Visible = true;

            string correct = (ViewState["ANS"] ?? "").ToString().Trim();
            if (string.IsNullOrWhiteSpace(correct))
            {
                lblResult.Text = "No answer configured.";
                lblResult.CssClass = "result-label warn";
                BindPrevNext(CurrentChapterId, CurrentSubtopicId);
                return;
            }

            if (string.IsNullOrEmpty(rblOptions.SelectedValue))
            {
                lblResult.Text = "Please choose an option.";
                lblResult.CssClass = "result-label warn";
                BindPrevNext(CurrentChapterId, CurrentSubtopicId);
                return;
            }

            string selected = rblOptions.SelectedValue.Trim();

            if (string.Equals(selected, correct, StringComparison.OrdinalIgnoreCase))
            {
                lblResult.Text = "✅ Correct!";
                lblResult.CssClass = "result-label correct";

                // 1. SAVE to database
                UpdateProgressTracking(CurrentSubtopicId);

                // 2. RE-CHECK status and UNLOCK links if this was the last question
                BindPrevNext(CurrentChapterId, CurrentSubtopicId);
            }
            else
            {
                lblResult.Text = "❌ Incorrect. Try again.";
                lblResult.CssClass = "result-label incorrect";

                // Even on incorrect, we call BindPrevNext to ensure 
                // the button stays locked if they haven't finished yet.
                BindPrevNext(CurrentChapterId, CurrentSubtopicId);
            }
        }

        private void UpdateProgressTracking(string subtopicId)
        {
            if (Role != "student") return;
            if (string.IsNullOrWhiteSpace(CurrentUserId)) return;
            if (string.IsNullOrWhiteSpace(subtopicId)) return;

            const string sql = @"
IF EXISTS (SELECT 1 FROM ProgressTracking WHERE userID=@uid AND subtopicID=@sid)
BEGIN
    UPDATE ProgressTracking
    SET updated_at = GETDATE()
    WHERE userID=@uid AND subtopicID=@sid;
END
ELSE
BEGIN
    DECLARE @nextId VARCHAR(10);

    SELECT @nextId =
        'PT' + RIGHT('000' + CAST(
            ISNULL(MAX(CAST(SUBSTRING(progressID, 3, 10) AS INT)), 0) + 1
        AS VARCHAR(10)), 3)
    FROM ProgressTracking
    WHERE progressID LIKE 'PT%';

    INSERT INTO ProgressTracking(progressID, userID, subtopicID, updated_at)
    VALUES(@nextId, @uid, @sid, GETDATE());
END;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@uid", CurrentUserId);
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        protected void btnBackDash_Click(object sender, EventArgs e)
        {
            if (Role == "tutor")
                Response.Redirect(ResolveUrl("~/Asm_WebPage/TutorDashboard.aspx"));
            else if (Role =="admin")
                Response.Redirect(ResolveUrl("~/Asm_WebPage/AdminDashboard.aspx"));
            else
                Response.Redirect(ResolveUrl("~/Asm_WebPage/StudentDashboard.aspx"));
        }

        private const string VS_PQ_ID = "PQ_ID";

        protected void btnEdit_Click(object sender, EventArgs e) => EnterEditMode();

        private void EnterEditMode()
        {
            if (Role == "tutor")
            {
                if (!TutorOwnsSubtopic(CurrentSubtopicId))
                {
                    pnlEdit.Visible = false;
                    pnlDisplay.Visible = true;
                    return;
                }
            }
            else if (Role != "admin")
            {
                return;
            }

            pnlEdit.Visible = true;
            pnlDisplay.Visible = false;

            pnlPractice.Visible = false;
            upPractice.Update();

            if (lblEditError != null)
            {
                lblEditError.Visible = false;
                lblEditError.Text = "";
            }

            string raw = (ViewState["RAW_CONTENT"] ?? "").ToString();
            txtEditContent.Text = raw;

            pnlPQEdit.Visible = false;
            ViewState[VS_PQ_ID] = "";

            const string pqSql = @"
                SELECT TOP 1 questionID, question, optionA, optionB, optionC, optionD, answer
                FROM PracticeQuestions
                WHERE subtopicID=@sid AND isDeleted=0
                ORDER BY questionID;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(pqSql, con))
            {
                cmd.Parameters.AddWithValue("@sid", CurrentSubtopicId);
                con.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        pnlPQEdit.Visible = true;

                        ViewState[VS_PQ_ID] = dr["questionID"].ToString().Trim();

                        txtPQQuestion.Text = dr["question"]?.ToString() ?? "";
                        txtPQA.Text = dr["optionA"]?.ToString() ?? "";
                        txtPQB.Text = dr["optionB"]?.ToString() ?? "";
                        txtPQC.Text = dr["optionC"]?.ToString() ?? "";
                        txtPQD.Text = dr["optionD"]?.ToString() ?? "";

                        var ans = (dr["answer"]?.ToString() ?? "").Trim().ToUpper();
                        if (ans == "A" || ans == "B" || ans == "C" || ans == "D")
                        {
                            ddlPQAns.ClearSelection();
                            var item = ddlPQAns.Items.FindByValue(ans);
                            if (item != null) item.Selected = true;
                        }
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (Role == "tutor" && !TutorOwnsSubtopic(CurrentSubtopicId)) return;
            if (Role != "tutor" && Role != "admin") return;

            lblEditError.Visible = false;
            lblEditError.Text = "";

            string newContent = (txtEditContent.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(newContent))
            {
                lblEditError.Text = "❌ Content cannot be empty.";
                lblEditError.Visible = true;

                pnlEdit.Visible = true;
                pnlDisplay.Visible = false;

                pnlPractice.Visible = false;
                upPractice.Update();
                return;
            }

            if (pnlPQEdit.Visible)
            {
                string q = (txtPQQuestion.Text ?? "").Trim();
                string a = (txtPQA.Text ?? "").Trim();
                string b = (txtPQB.Text ?? "").Trim();
                string c = (txtPQC.Text ?? "").Trim();
                string d = (txtPQD.Text ?? "").Trim();
                string ans = (ddlPQAns.SelectedValue ?? "").Trim().ToUpper();

                if (string.IsNullOrWhiteSpace(q) ||
                    string.IsNullOrWhiteSpace(a) ||
                    string.IsNullOrWhiteSpace(b) ||
                    string.IsNullOrWhiteSpace(c) ||
                    string.IsNullOrWhiteSpace(d))
                {
                    lblEditError.Text = "❌ All fields are required !";
                    lblEditError.Visible = true;

                    pnlEdit.Visible = true;
                    pnlDisplay.Visible = false;

                    pnlPractice.Visible = false;
                    upPractice.Update();
                    return;
                }

                if (ans != "A" && ans != "B" && ans != "C" && ans != "D")
                {
                    lblEditError.Text = "❌ Please choose correct answer A / B / C / D.";
                    lblEditError.Visible = true;

                    pnlEdit.Visible = true;
                    pnlDisplay.Visible = false;

                    pnlPractice.Visible = false;
                    upPractice.Update();
                    return;
                }
            }

            const string sql = @"
                UPDATE Subtopics
                SET content = @c
                WHERE subtopicID=@sid AND isDeleted=0;";

            using (var con = new SqlConnection(Cs))
            {
                con.Open();

                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@sid", CurrentSubtopicId);
                    cmd.Parameters.AddWithValue("@c", newContent);
                    cmd.ExecuteNonQuery();
                }

                if (pnlPQEdit.Visible)
                {
                    string pqId = (ViewState[VS_PQ_ID] ?? "").ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(pqId))
                    {
                        string q = (txtPQQuestion.Text ?? "").Trim();
                        string a = (txtPQA.Text ?? "").Trim();
                        string b = (txtPQB.Text ?? "").Trim();
                        string c = (txtPQC.Text ?? "").Trim();
                        string d = (txtPQD.Text ?? "").Trim();
                        string ans = (ddlPQAns.SelectedValue ?? "").Trim().ToUpper();

                        const string pqUp = @"
                            UPDATE PracticeQuestions
                            SET question=@q, optionA=@a, optionB=@b, optionC=@c, optionD=@d, answer=@ans
                            WHERE questionID=@qid AND subtopicID=@sid AND isDeleted=0;";

                        using (var cmd2 = new SqlCommand(pqUp, con))
                        {
                            cmd2.Parameters.AddWithValue("@qid", pqId);
                            cmd2.Parameters.AddWithValue("@sid", CurrentSubtopicId);
                            cmd2.Parameters.AddWithValue("@q", q);
                            cmd2.Parameters.AddWithValue("@a", a);
                            cmd2.Parameters.AddWithValue("@b", b);
                            cmd2.Parameters.AddWithValue("@c", c);
                            cmd2.Parameters.AddWithValue("@d", d);
                            cmd2.Parameters.AddWithValue("@ans", ans);
                            cmd2.ExecuteNonQuery();
                        }
                    }
                }
            }

            Response.Redirect(BuildLessonUrl(CurrentChapterId, CurrentSubtopicId));
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlEdit.Visible = false;
            pnlDisplay.Visible = true;

            BindPracticeQuestion(CurrentSubtopicId);
            upPractice.Update();

            ApplyTutorOwnershipUiGuard();
        }

        protected void btnDeleteSubtopic_Click(object sender, EventArgs e)
        {
            if (Role != "tutor") return;
            if (!TutorOwnsSubtopic(CurrentSubtopicId)) return;

            string nextSub = GetNextSubtopicAfterDelete(CurrentChapterId, CurrentSubtopicId);
            SoftDeleteSubtopicOnly(CurrentSubtopicId, CurrentUserId);

            if (!string.IsNullOrWhiteSpace(nextSub))
                Response.Redirect(BuildLessonUrl(CurrentChapterId, nextSub));
            else
                Response.Redirect(ResolveUrl("~/Asm_WebPage/TutorDashboard.aspx"));
        }

        protected void btnAdminDeleteSubtopic_Click(object sender, EventArgs e)
        {
            if (Role != "admin") return;

            string nextSub = GetNextSubtopicAfterDelete(CurrentChapterId, CurrentSubtopicId);
            SoftDeleteSubtopicOnly_Admin(CurrentSubtopicId);

            if (!string.IsNullOrWhiteSpace(nextSub))
                Response.Redirect(BuildLessonUrl(CurrentChapterId, nextSub));
            else
                Response.Redirect(ResolveUrl("~/Asm_WebPage/StudentDashboard.aspx"));
        }

        private void SoftDeleteSubtopicOnly(string subtopicId, string tutorId)
        {
            const string sql = @"
UPDATE Subtopics
SET isDeleted=1, deleted_at=GETDATE(), deleted_by=@tid
WHERE subtopicID=@sid AND created_by=@tid AND isDeleted=0;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                cmd.Parameters.AddWithValue("@tid", tutorId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void SoftDeleteSubtopicOnly_Admin(string subtopicId)
        {
            const string sql = @"
UPDATE Subtopics
SET isDeleted=1, deleted_at=GETDATE(), deleted_by='admin'
WHERE subtopicID=@sid AND isDeleted=0;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        protected void btnDeletePQ_Click(object sender, EventArgs e)
        {
            if (Role != "tutor") return;
            if (!TutorOwnsSubtopic(CurrentSubtopicId)) return;

            SoftDeletePQOnly(CurrentSubtopicId);
            Response.Redirect(BuildLessonUrl(CurrentChapterId, CurrentSubtopicId));
        }

        protected void btnAdminDeletePQ_Click(object sender, EventArgs e)
        {
            if (Role != "admin") return;

            SoftDeletePQOnly(CurrentSubtopicId);
            Response.Redirect(BuildLessonUrl(CurrentChapterId, CurrentSubtopicId));
        }

        private void SoftDeletePQOnly(string subtopicId)
        {
            const string sql = @"
UPDATE PracticeQuestions
SET isDeleted=1, deleted_at=GETDATE()
WHERE subtopicID=@sid AND isDeleted=0;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string GetNextSubtopicAfterDelete(string chapterId, string subtopicId)
        {
            var ids = GetSubtopicIdsForChapter(chapterId);
            if (ids.Count == 0) return "";

            int idx = ids.FindIndex(x => x.Equals(subtopicId, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return ids[0];

            if (idx < ids.Count - 1) return ids[idx + 1];
            if (idx > 0) return ids[idx - 1];
            return "";
        }
        private bool TutorOwnsSubtopic(string subtopicId)
        {
            if (string.IsNullOrWhiteSpace(subtopicId)) return false;
            if (string.IsNullOrWhiteSpace(CurrentUserId)) return false;

            const string sql = @"SELECT COUNT(1)
                                 FROM Subtopics
                                 WHERE subtopicID=@sid AND created_by=@tid AND isDeleted=0;";

            using (var con = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@sid", subtopicId);
                cmd.Parameters.AddWithValue("@tid", CurrentUserId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void ApplyTutorOwnershipUiGuard()
        {
            if (Role != "tutor") return;

            bool isOwner = TutorOwnsSubtopic(CurrentSubtopicId);

            btnEdit.Enabled = isOwner;
            btnDeletePQ.Enabled = isOwner;
            btnDeleteSubtopic.Enabled = isOwner;
        }

        private class SubNav
        {
            public string ChapterId { get; set; }
            public string SubtopicId { get; set; }
            public string DisplayNo { get; set; }
            public string CleanTitle { get; set; }
            public bool IsActive { get; set; }
        }

        private string StripLeadingNumber(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return "";
            return Regex.Replace(title.Trim(), @"^\s*\d+(\.\d+)*\s*[\)\.\-]?\s*", "");
        }

        private string ExtractLeadingNumber(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return "";
            var m = Regex.Match(title.Trim(), @"^\s*\d+(\.\d+)*");
            return m.Success ? m.Value : "";
        }

        private void RegisterTryPanelScript()
        {
            string script = @"
<script>
(function () {
    if (window.__TRY_PANEL_INIT__) return;
    window.__TRY_PANEL_INIT__ = true;

    var pyodide = null;
    var pyReady = false;
    var cm = null;

    async function initPyodideOnce() {
        if (pyReady) return;
        pyodide = await loadPyodide();
        pyReady = true;
        var s = document.getElementById('pyStatus');
        if (s) s.textContent = 'Python Ready';
    }

    function lockCodeMirrorScroll(editor) {
        if (!editor) return;
        editor.setOption('lineWrapping', true);
        editor.setOption('scrollbarStyle', 'null');
        var scroller = editor.getScrollerElement();
        if (scroller) {
            scroller.style.overflow = 'hidden';
            scroller.addEventListener('wheel', function (e) {
                e.preventDefault();
            }, { passive: false });
        }
        var wrapper = editor.getWrapperElement();
        if (wrapper) wrapper.style.overflow = 'hidden';
    }

    function initCodeMirrorOnce() {
        if (cm) return;
        var ta = document.getElementById('pyCode');
        if (!ta || typeof CodeMirror === 'undefined') return;

        cm = CodeMirror.fromTextArea(ta, {
            mode: 'python',
            lineNumbers: true,
            indentUnit: 4,
            tabSize: 4,
            indentWithTabs: false,
            viewportMargin: Infinity,
            lineWrapping: true,
            scrollbarStyle: 'null'
        });

        cm.setValue('');
        lockCodeMirrorScroll(cm);
    }

    function setConsole(text) {
        var out = document.getElementById('pyOut');
        if (out) out.textContent = text || '';
    }

    function clearAI() {
        var aiContent = document.getElementById('aiContent');
        if (aiContent) aiContent.innerHTML = '';
        var count = document.getElementById('aiCount');
        if (count) count.textContent = '0';
    }

    function decodeSnippet(raw) {
        if (!raw) return '';
        var s = String(raw);

        s = s.replace(/\r\n/g, '\n');

        s = s.replace(/\\\\n/g, '__BSN__')
             .replace(/\\\\t/g, '__BST__')
             .replace(/\\\\r/g, '__BSR__');

        s = s.replace(/\\n/g, '\n')
             .replace(/\\t/g, '\t')
             .replace(/\\r/g, '\r');

        s = s.replace(/__BSN__/g, '\\n')
             .replace(/__BST__/g, '\\t')
             .replace(/__BSR__/g, '\\r');

        return s;
    }

    async function runCode() {
        var runBtn = document.getElementById('btnRun');
        if (runBtn) runBtn.disabled = true;

        clearAI();
        switchTab('console');

        try {
            initCodeMirrorOnce();
            if (!cm) {
                setConsole('Editor not ready.');
                return;
            }

            var code = cm.getValue() || '';
            setConsole('');

            if (!pyReady) await initPyodideOnce();

            pyodide.globals.set('user_code', code);

            var wrapper = [
              'import sys, io, traceback, builtins',
              'from js import prompt',
              '',
              'def _input(p=""""):',
              '    r = prompt(p)',
              '    if r is None:',
              '        raise EOFError(""Input cancelled"")',
              '    return str(r)',
              '',
              'builtins.input = _input',
              '',
              '_stdout = io.StringIO()',
              '_stderr = io.StringIO()',
              '_old_out, _old_err = sys.stdout, sys.stderr',
              'sys.stdout, sys.stderr = _stdout, _stderr',
              'try:',
              '    g = globals()',
              '    exec(user_code, g, g)',
              'except Exception:',
              '    traceback.print_exc()',
              'finally:',
              '    sys.stdout, sys.stderr = _old_out, _old_err',
              '_stdout.getvalue() + _stderr.getvalue()'
            ].join('\n');

            var result = await pyodide.runPythonAsync(wrapper);
            setConsole(result || '');
        }
        catch (err) {
            setConsole(String(err));
        }
        finally {
            var runBtn2 = document.getElementById('btnRun');
            if (runBtn2) runBtn2.disabled = false;
        }
    }

    function switchTab(name) {
        var consoleBox = document.getElementById('pyOut');
        var aiPanel = document.getElementById('aiPanel');
        var tabs = document.querySelectorAll('.ide-tab');
        tabs.forEach(function (t) { t.classList.remove('active'); });

        var q = String.fromCharCode(39);
        var sel = '[data-tab=' + q + name + q + ']';
        var tabBtn = document.querySelector(sel);
        if (tabBtn) tabBtn.classList.add('active');

        if (!consoleBox || !aiPanel) return;

        if (name === 'console') {
            consoleBox.style.display = 'block';
            aiPanel.style.display = 'none';
        } else {
            consoleBox.style.display = 'none';
            aiPanel.style.display = 'block';
        }
    }

    async function askAI() {
        var askBtn = document.getElementById('btnAskAI');
        if (askBtn) askBtn.disabled = true;

        try {
            initCodeMirrorOnce();
            if (!cm) return;

            var outEl = document.getElementById('pyOut');
            var output = outEl ? (outEl.textContent || '') : '';
            var code = cm.getValue() || '';

            switchTab('ai');

            var aiContent = document.getElementById('aiContent');
            if (!aiContent) return;

            var msg = document.createElement('div');
            msg.className = 'ai-message';
            msg.textContent = '';              
            aiContent.appendChild(msg);

            var countEl = document.getElementById('aiCount');
            if (countEl) countEl.textContent = aiContent.children.length;

            var res = await fetch('AskAI.ashx', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    code: code,
                    output: output,
                    question: ''
                })
            });

            if (!res.ok) {
                msg.textContent = 'HTTP error: ' + res.status;
                return;
            }

            var ct = (res.headers.get('content-type') || '').toLowerCase();
            if (ct.indexOf('text/event-stream') === -1) {
                var txt = await res.text();
                msg.textContent = 'Server did not return SSE. Status=' + res.status + '. ' + (txt || '').slice(0, 220);
                return;
            }

            var reader = res.body.getReader();
            var decoder = new TextDecoder('utf-8');
            var buffer = '';

            while (true) {
                var read = await reader.read();
                if (read.done) break;

                buffer += decoder.decode(read.value, { stream: true });

                var parts = buffer.split('\n\n');
                buffer = parts.pop() || '';

                for (var i = 0; i < parts.length; i++) {
                    var block = parts[i];
                    var lines = block.split('\n');

                    var evt = '';
                    var data = '';

                    for (var j = 0; j < lines.length; j++) {
                        var line = lines[j];
                        if (line.indexOf('event:') === 0) evt = line.slice(6).trim();
                        if (line.indexOf('data:') === 0) data += line.slice(5).trim();
                    }

                    if (evt === 'chunk') {
                        // data 是 Groq 的 json chunk: { choices:[{delta:{content:""...""}}] }
                        try {
                            var obj = JSON.parse(data);
                            var delta = obj && obj.choices && obj.choices[0] && obj.choices[0].delta
                                ? (obj.choices[0].delta.content || '')
                                : '';

                            if (delta) msg.textContent += delta;
                        } catch (e) {
                        }
                    }

                    if (evt === 'error') {
                        msg.textContent += '\n\n[Error] ' + data;
                    }

                    if (evt === 'done') {
                        return;
                    }
                }
            }
        }
        catch (err) {
            var aiContent2 = document.getElementById('aiContent');
            if (aiContent2) {
                var m = document.createElement('div');
                m.className = 'ai-message';
                m.textContent = 'Ask AI failed: ' + String(err);
                aiContent2.appendChild(m);

                var count2 = document.getElementById('aiCount');
                if (count2) count2.textContent = aiContent2.children.length;
            }
        }
        finally {
            if (askBtn) askBtn.disabled = false;
        }
    }

    function resetEditor() {
        initCodeMirrorOnce();
        if (cm) cm.setValue('');
        setConsole('');
        clearAI();              
        switchTab('console');   
        if (cm) cm.focus();
        if (cm) cm.scrollTo(null, 0);
    }

    function fillEditorFromSnippet(btn) {
        initCodeMirrorOnce();
        if (!cm) return;

        var code = '';

        var exp = btn.closest('.lm-exp');
        var codeEl = exp ? exp.querySelector('.lm-code pre code') : null;
        if (codeEl) {
            code = codeEl.textContent || '';
        } else {
            code = btn.getAttribute('data-code') || '';
        }

        code = String(code).replace(/\r\n/g, '\n');

        cm.setValue(code);
        cm.focus();
        cm.scrollTo(null, 0);
    }

    document.addEventListener('DOMContentLoaded', function () {
        initCodeMirrorOnce();

        var runBtn = document.getElementById('btnRun');
        var resetBtn = document.getElementById('btnReset');
        var askBtn = document.getElementById('btnAskAI');

        if (runBtn) runBtn.addEventListener('click', runCode);
        if (resetBtn) resetBtn.addEventListener('click', resetEditor);
        if (askBtn) askBtn.addEventListener('click', askAI);

        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.js-tryit');
            if (!btn) return;
            e.preventDefault();
            fillEditorFromSnippet(btn);
        });

        initPyodideOnce();

        document.querySelectorAll('.ide-tab').forEach(function (tab) {
            tab.addEventListener('click', function () {
                switchTab(tab.dataset.tab);
            });
        });

        switchTab('console');
    });
})();
</script>";

            ClientScript.RegisterStartupScript(this.GetType(), "tryPanelScriptV4", script);
        }
    }
}