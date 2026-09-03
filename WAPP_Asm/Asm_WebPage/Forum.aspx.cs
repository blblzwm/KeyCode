using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class Forum : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetupUIForRole();
                LoadPosts();
            }
        }

        private void SetupUIForRole()
        {
            string role = Session["role"]?.ToString();

            if (role == "admin")
            {
                btnNewPost.Visible = false;
                pnlCommentBox.Visible = false;
            }
            else if (role == "student" || role == "tutor")
            {
                btnNewPost.Visible = true;
                pnlCommentBox.Visible = true;
                pnlAnnouncementOption.Visible = (role == "tutor");
            }
            else
            {
                btnNewPost.Visible = false;
                pnlCommentBox.Visible = false;
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            string role = Session["role"]?.ToString();
            Response.Redirect("~/Asm_WebPage/" + role + "Dashboard.aspx");
        }

        // ========== LOAD POSTS ==========

        private void LoadPosts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                                        p.postID, 
                                        p.userID as postUserID,
                                        p.title, 
                                        p.content, 
                                        p.created_at,
                                        p.is_announcement,
                                        u.username,
                                        u.fname, 
                                        u.lname, 
                                        u.role,
                                        u.upload_profile
                                     FROM ForumPosts p
                                     INNER JOIN Users u ON p.userID = u.userID
                                     ORDER BY p.is_announcement DESC, p.created_at DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptPosts.DataSource = dt;
                            rptPosts.DataBind();
                            pnlNoPosts.Visible = false;
                        }
                        else
                        {
                            rptPosts.DataSource = null;
                            rptPosts.DataBind();
                            pnlNoPosts.Visible = true;
                        }
                    }
                }
            }
            catch { }
        }

        protected void rptPosts_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item &&
                e.Item.ItemType != ListItemType.AlternatingItem) return;

            DataRowView row = (DataRowView)e.Item.DataItem;
            string postID = row["postID"].ToString();
            string postUserID = row["postUserID"].ToString();
            string currentUserID = Session["UserID"]?.ToString();
            string currentRole = Session["role"]?.ToString();

            Label lblCommentCount = (Label)e.Item.FindControl("lblCommentCount");
            if (lblCommentCount != null)
            {
                int count = GetCommentCount(postID);
                lblCommentCount.Text = count + (count == 1 ? " Comment" : " Comments");
            }

            bool isOwner = currentUserID == postUserID;
            bool isAdmin = currentRole == "admin";

            Panel pnlPostDots = (Panel)e.Item.FindControl("pnlPostDots");
            if (pnlPostDots != null)
                pnlPostDots.Visible = isOwner || isAdmin;

            LinkButton btnEditPost = (LinkButton)e.Item.FindControl("btnEditPost");
            if (btnEditPost != null)
                btnEditPost.Visible = isOwner;

            LinkButton btnDeletePost = (LinkButton)e.Item.FindControl("btnDeletePost");
            if (btnDeletePost != null)
                btnDeletePost.Visible = isOwner || isAdmin;
        }

        private int GetCommentCount(string postID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT COUNT(*) FROM ForumComments WHERE postID = @PostID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PostID", postID);
                        conn.Open();
                        return (int)cmd.ExecuteScalar();
                    }
                }
            }
            catch { return 0; }
        }

        // ========== THREAD MODAL ==========

        protected void btnViewComments_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string postID = btn.CommandArgument;

            ViewState["CurrentPostID"] = postID;
            hfCurrentPostID.Value = postID;
            pnlSummary.Visible = false;

            LoadThreadModal(postID);
            ScriptManager.RegisterStartupScript(this, GetType(), "showModal", "showThreadModal();", true);
        }

        private void LoadThreadModal(string postID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string postQuery = @"SELECT 
                                           p.title, p.content, p.created_at,
                                           u.username, u.fname, u.lname, u.role, u.upload_profile
                                         FROM ForumPosts p
                                         INNER JOIN Users u ON p.userID = u.userID
                                         WHERE p.postID = @PostID";

                    using (SqlCommand cmd = new SqlCommand(postQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PostID", postID);
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            lblThreadTitle.Text = reader["title"].ToString();
                            lblOrigPostAuthor.Text = reader["username"].ToString();
                            lblOrigPostRole.Text = GetRoleBadgeHtml(reader["role"].ToString());
                            lblOrigPostTime.Text = GetRelativeTime(reader["created_at"]);
                            lblOrigPostContent.Text = reader["content"].ToString().Replace("\n", "<br>");
                            litOrigPostAvatar.Text = GetAvatarHtml(
                                reader["fname"], reader["lname"],
                                reader["role"], reader["upload_profile"]);
                        }
                        reader.Close();
                    }

                    string commentsQuery = @"SELECT 
                                              c.commentID, c.userID as commentUserID,
                                              c.content, c.created_at,
                                              u.username, u.fname, u.lname, u.role, u.upload_profile
                                            FROM ForumComments c
                                            INNER JOIN Users u ON c.userID = u.userID
                                            WHERE c.postID = @PostID
                                            ORDER BY c.created_at ASC";

                    using (SqlCommand cmd = new SqlCommand(commentsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PostID", postID);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptComments.DataSource = dt;
                            rptComments.DataBind();
                            pnlNoComments.Visible = false;

                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            sb.Append("<div id='commentDataStore'");
                            int i = 1;
                            foreach (DataRow row in dt.Rows)
                            {
                                string author = row["username"].ToString();
                                string content = EscapeForJs(row["content"].ToString());
                                sb.Append($" data-comment-{i}-author='{author}' data-comment-{i}-content='{content}'");
                                i++;
                            }
                            sb.Append($" data-count='{dt.Rows.Count}'></div>");
                            litCommentData.Text = sb.ToString();
                        }
                        else
                        {
                            rptComments.DataSource = null;
                            rptComments.DataBind();
                            pnlNoComments.Visible = true;
                            litCommentData.Text = "<div id='commentDataStore' data-count='0'></div>";
                        }
                    }
                }
            }
            catch { }
        }

        protected void rptComments_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item &&
                e.Item.ItemType != ListItemType.AlternatingItem) return;

            DataRowView row = (DataRowView)e.Item.DataItem;
            string commentUserID = row["commentUserID"].ToString();
            string currentUserID = Session["UserID"]?.ToString();
            string currentRole = Session["role"]?.ToString();

            bool isOwner = currentUserID == commentUserID;
            bool isAdmin = currentRole == "admin";

            Panel pnlCommentDots = (Panel)e.Item.FindControl("pnlCommentDots");
            if (pnlCommentDots != null)
                pnlCommentDots.Visible = isOwner || isAdmin;

            LinkButton btnEditComment = (LinkButton)e.Item.FindControl("btnEditComment");
            if (btnEditComment != null)
                btnEditComment.Visible = isOwner;

            LinkButton btnDeleteComment = (LinkButton)e.Item.FindControl("btnDeleteComment");
            if (btnDeleteComment != null)
                btnDeleteComment.Visible = isOwner || isAdmin;
        }

        // ========== AI SUMMARISE ==========

        protected void btnSummarise_Click(object sender, EventArgs e)
        {
            string postId = hfCurrentPostID.Value;
            if (string.IsNullOrEmpty(postId)) return;

            string threadText = BuildThreadText(postId);
            string summary = GetGroqSummary(threadText);

            lblSummary.Text = summary;
            pnlSummary.Visible = true;

            ScriptManager.RegisterStartupScript(this, GetType(), "showThread", "showThreadModal();", true);
        }

        private string BuildThreadText(string postId)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            SqlConnection con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                string postSql = @"
                    SELECT p.title, p.content, u.username AS author, u.role
                    FROM   ForumPosts p
                    INNER JOIN Users u ON p.userID = u.userID
                    WHERE  p.postID = @postID";

                SqlCommand postCmd = new SqlCommand(postSql, con);
                postCmd.Parameters.AddWithValue("@postID", postId);
                SqlDataReader postDr = postCmd.ExecuteReader();

                if (postDr.Read())
                {
                    sb.AppendLine("POST TITLE: " + postDr["title"]);
                    sb.AppendLine("POST BY: " + postDr["author"] + " (" + postDr["role"] + ")");
                    sb.AppendLine("CONTENT: " + postDr["content"]);
                    sb.AppendLine("---");
                }
                postDr.Close();

                string commentSql = @"
                    SELECT c.content, u.username AS author, u.role
                    FROM   ForumComments c
                    INNER JOIN Users u ON c.userID = u.userID
                    WHERE  c.postID = @postID
                    ORDER BY c.created_at ASC";

                SqlCommand commentCmd = new SqlCommand(commentSql, con);
                commentCmd.Parameters.AddWithValue("@postID", postId);
                SqlDataReader commentDr = commentCmd.ExecuteReader();

                int commentCount = 0;
                while (commentDr.Read())
                {
                    commentCount++;
                    sb.AppendLine($"COMMENT {commentCount} BY: " + commentDr["author"] + " (" + commentDr["role"] + ")");
                    sb.AppendLine("CONTENT: " + commentDr["content"]);
                    sb.AppendLine("---");
                }
                commentDr.Close();

                if (commentCount == 0)
                    sb.AppendLine("(No comments on this post yet)");
            }
            catch { }
            finally { con.Close(); }

            return sb.ToString();
        }

        private string GetGroqSummary(string threadText)
        {
            try
            {
                string apiKey = ConfigurationManager.AppSettings["GroqKey"];
                string url = "https://api.groq.com/openai/v1/chat/completions";

                string prompt = "You are summarising a forum post from KeyCode, an online learning platform. " +
                                "It includes an original post followed by student and tutor comments. " +
                                "Summarise in 3-5 sentences covering: " +
                                "1) The main topic or question raised, " +
                                "2) Key points or answers from the comments, " +
                                "3) Any conclusion or consensus reached. " +
                                "Be concise, neutral, and educational in tone.\n\n" +
                                threadText;

                string requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    model = "llama-3.1-8b-instant",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                });

                var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "Bearer " + apiKey);

                using (var sw = new System.IO.StreamWriter(request.GetRequestStream()))
                    sw.Write(requestBody);

                using (var response = request.GetResponse())
                using (var sr = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    string json = sr.ReadToEnd();
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    return result.choices[0].message.content.ToString();
                }
            }
            catch { return "Summary unavailable."; }
        }

        // ========== PROFANITY CHECK ==========

        private bool ContainsProfanity(string text)
        {
            try
            {
                string url = "https://www.purgomalum.com/service/containsprofanity?text=" + Uri.EscapeDataString(text);
                using (var client = new System.Net.WebClient())
                {
                    string result = client.DownloadString(url);
                    return result.Trim().ToLower() == "true";
                }
            }
            catch { return false; }
        }

        // ========== POST COMMENT ==========

        protected void btnPostComment_Click(object sender, EventArgs e)
        {
            string comment = txtComment.Text.Trim();
            if (string.IsNullOrWhiteSpace(comment)) return;

            //cannot more than 1000 characters
            if (comment.Length > 1000)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "commentLengthError",
                    "document.getElementById('commentLengthError').style.display='block';" +
                    "setTimeout(function(){ document.getElementById('commentLengthError').style.display='none'; }, 3000);" +
                    "showThreadModal();", true);
                return;
            }

            if (ContainsProfanity(comment))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "commentProfanity",
                    "document.getElementById('commentProfanityError').style.display='block';" +
                    "setTimeout(function(){ document.getElementById('commentProfanityError').style.display='none'; }, 3000);" +
                    "showThreadModal();", true);
                return;
            }

            string postID = ViewState["CurrentPostID"]?.ToString();
            string userID = Session["UserID"]?.ToString();

            if (string.IsNullOrEmpty(userID)) return;

            try
            {
                string commentID = GenerateCommentID();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO ForumComments (commentID, postID, userID, content, created_at) 
                                     VALUES (@CommentID, @PostID, @UserID, @Content, @CreatedAt)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CommentID", commentID);
                        cmd.Parameters.AddWithValue("@PostID", postID);
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        cmd.Parameters.AddWithValue("@Content", txtComment.Text.Trim());
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                txtComment.Text = "";
                LoadThreadModal(postID);
                LoadPosts();
                ScriptManager.RegisterStartupScript(this, GetType(), "showModal",
                    "showThreadModal(); alert('Your comment has been posted.');", true);
            }
            catch { }
        }

        // ========== PUBLISH POST ==========

        protected void btnPublishPost_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string title = txtPostTitle.Text.Trim();
            string content = txtPostContent.Text.Trim();

            if (ContainsProfanity(title) || ContainsProfanity(content))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showNewPost",
                    "new bootstrap.Modal(document.getElementById('newPostModal')).show();", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "profanityError",
                    "document.getElementById('profanityError').style.display='block';", true);
                return;
            }

            string userID = Session["UserID"]?.ToString();
            string role = Session["role"]?.ToString();

            if (string.IsNullOrEmpty(userID) || role == "admin") return;

            try
            {
                string postID = GeneratePostID();
                bool isAnnouncement = (role == "tutor" && chkIsAnnouncement.Checked);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO ForumPosts (postID, userID, title, content, is_announcement, created_at) 
                                     VALUES (@PostID, @UserID, @Title, @Content, @IsAnnouncement, @CreatedAt)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PostID", postID);
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        cmd.Parameters.AddWithValue("@Title", txtPostTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@Content", txtPostContent.Text.Trim());
                        cmd.Parameters.AddWithValue("@IsAnnouncement", isAnnouncement);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                txtPostTitle.Text = "";
                txtPostContent.Text = "";
                chkIsAnnouncement.Checked = false;
                LoadPosts();

                ScriptManager.RegisterStartupScript(this, GetType(), "hideModal",
                    "hideNewPostModal(); alert('Your post has been published.');", true);
            }
            catch { }
        }

        // ========== POST ACTIONS ==========

        protected void PostAction_Command(object sender, CommandEventArgs e)
        {
            string postID = e.CommandArgument.ToString();
            string commandName = e.CommandName;
            if (commandName == "EditPost") LoadPostForEdit(postID);
            else if (commandName == "DeletePost") DeletePost(postID);
        }

        private void LoadPostForEdit(string postID)
        {
            try
            {
                string currentRole = Session["role"]?.ToString();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT postID, userID, title, content, is_announcement FROM ForumPosts WHERE postID = @PostID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PostID", postID);
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            if (reader["userID"].ToString() != Session["UserID"]?.ToString())
                                return;

                            hfEditPostID.Value = reader["postID"].ToString();
                            txtEditPostTitle.Text = reader["title"].ToString();
                            txtEditPostContent.Text = reader["content"].ToString();
                            bool isAnnouncement = Convert.ToBoolean(reader["is_announcement"]);

                            pnlEditAnnouncementOption.Visible = (currentRole == "tutor");
                            if (currentRole == "tutor" && isAnnouncement)
                            {
                                ScriptManager.RegisterStartupScript(this, GetType(), "preCheckAnnouncement",
                                    $"document.getElementById('{chkisEditAnnouncement.ClientID}').checked = true;", true);
                            }

                            ScriptManager.RegisterStartupScript(this, GetType(), "showEditModal",
                                "showEditPostModal();", true);
                        }
                        reader.Close();
                    }
                }
            }
            catch { }
        }

        protected void btnSaveEditPost_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showEditModal", "showEditPostModal();", true);
                return;
            }

            string postID = hfEditPostID.Value;
            string currentUserID = Session["UserID"]?.ToString();
            string currentRole = Session["role"]?.ToString();
            if (string.IsNullOrEmpty(postID)) return;

            string title = txtEditPostTitle.Text.Trim();
            string content = txtEditPostContent.Text.Trim();
            bool isAnnouncement = (currentRole == "tutor") && chkisEditAnnouncement.Checked;

            if (ContainsProfanity(title) || ContainsProfanity(content))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showEditModal", "showEditPostModal();", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "editPostProfanity",
                    "document.getElementById('editPostProfanityError').style.display='block';", true);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string checkQuery = "SELECT userID FROM ForumPosts WHERE postID = @PostID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@PostID", postID);
                        conn.Open();
                        if (checkCmd.ExecuteScalar()?.ToString() != currentUserID)
                            return;
                    }

                    string updateQuery = @"UPDATE ForumPosts 
                       SET title           = @Title, 
                           content         = @Content,
                           is_announcement = @IsAnnouncement
                       WHERE postID = @PostID";

                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@Title", title);
                        updateCmd.Parameters.AddWithValue("@Content", content);
                        updateCmd.Parameters.AddWithValue("@IsAnnouncement", isAnnouncement);
                        updateCmd.Parameters.AddWithValue("@PostID", postID);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                LoadPosts();
                ScriptManager.RegisterStartupScript(this, GetType(), "hideEditModal",
                    "hideEditPostModal(); alert('Your post has been updated.');", true);
            }
            catch { }
        }

        private void DeletePost(string postID)
        {
            string currentUserID = Session["UserID"]?.ToString();
            string currentRole = Session["role"]?.ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string checkQuery = "SELECT userID FROM ForumPosts WHERE postID = @PostID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@PostID", postID);
                        string postOwnerID = checkCmd.ExecuteScalar()?.ToString();
                        if (postOwnerID != currentUserID && currentRole != "admin")
                            return;
                    }

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM ForumComments WHERE postID = @PostID", conn))
                    {
                        cmd.Parameters.AddWithValue("@PostID", postID);
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM ForumPosts WHERE postID = @PostID", conn))
                    {
                        cmd.Parameters.AddWithValue("@PostID", postID);
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadPosts();
                ScriptManager.RegisterStartupScript(this, GetType(), "deletePostMsg",
                "alert('Post deleted successfully.');", true);
            }
            catch { }
        }

        // ========== COMMENT ACTIONS ==========

        protected void CommentAction_Command(object sender, CommandEventArgs e)
        {
            string commentID = e.CommandArgument.ToString();
            string commandName = e.CommandName;
            string postID = ViewState["CurrentPostID"]?.ToString();
            if (commandName == "EditComment") LoadCommentForEdit(commentID, postID);
            else if (commandName == "DeleteComment") DeleteComment(commentID, postID);
        }

        private void LoadCommentForEdit(string commentID, string postID)
        {
            string currentUserID = Session["UserID"]?.ToString();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT commentID, userID, content FROM ForumComments WHERE commentID = @CommentID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CommentID", commentID);
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            if (reader["userID"].ToString() != currentUserID)
                                return;

                            hfEditCommentID.Value = reader["commentID"].ToString();
                            hfEditCommentPostID.Value = postID;
                            txtEditComment.Text = reader["content"].ToString();
                            ScriptManager.RegisterStartupScript(this, GetType(), "showEditCommentModal", "showEditCommentModal();", true);
                        }
                        reader.Close();
                    }
                }
            }
            catch { }
        }

        protected void btnSaveEditComment_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showEditCommentModal", "showEditCommentModal();", true);
                return;
            }

            string commentID = hfEditCommentID.Value;
            string postID = hfEditCommentPostID.Value;
            string currentUserID = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(commentID)) return;

            string content = txtEditComment.Text.Trim();

            //cannot more than 1000 characters
            if (content.Length > 1000)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showEditCommentModal", "showEditCommentModal();", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "editCommentLengthError",
                    "document.getElementById('editCommentLengthError').style.display='block';", true);
                return;
            }

            if (ContainsProfanity(content))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showEditCommentModal", "showEditCommentModal();", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "editCommentProfanity",
                    "document.getElementById('editCommentProfanityError').style.display='block';", true);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string checkQuery = "SELECT userID FROM ForumComments WHERE commentID = @CommentID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@CommentID", commentID);
                        conn.Open();
                        if (checkCmd.ExecuteScalar()?.ToString() != currentUserID)
                            return;
                    }

                    string updateQuery = "UPDATE ForumComments SET content = @Content WHERE commentID = @CommentID";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@Content", content);
                        updateCmd.Parameters.AddWithValue("@CommentID", commentID);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                ViewState["CurrentPostID"] = postID;
                LoadThreadModal(postID);
                LoadPosts();
                ScriptManager.RegisterStartupScript(this, GetType(), "afterEditComment",
                    "hideEditCommentModal(); showThreadModal(); alert('Your comment has been updated.');", true);
            }
            catch { }
        }

        private void DeleteComment(string commentID, string postID)
        {
            string currentUserID = Session["UserID"]?.ToString();
            string currentRole = Session["role"]?.ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string checkQuery = "SELECT userID FROM ForumComments WHERE commentID = @CommentID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@CommentID", commentID);
                        string commentOwnerID = checkCmd.ExecuteScalar()?.ToString();
                        if (commentOwnerID != currentUserID && currentRole != "admin")
                            return;
                    }

                    using (SqlCommand deleteCmd = new SqlCommand("DELETE FROM ForumComments WHERE commentID = @CommentID", conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@CommentID", commentID);
                        deleteCmd.ExecuteNonQuery();
                    }
                }

                LoadThreadModal(postID);
                LoadPosts();
                ScriptManager.RegisterStartupScript(this, GetType(), "showModal",
                    "showThreadModal(); alert('Comment deleted successfully.');", true);
            }
            catch { }
        }

        // ========== ID GENERATION ==========

        private string GeneratePostID()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 postID FROM ForumPosts ORDER BY postID DESC", conn))
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                            return "FP" + (int.Parse(result.ToString().Substring(2)) + 1).ToString("D3");
                        return "FP001";
                    }
                }
            }
            catch { return "FP" + DateTime.Now.Ticks.ToString().Substring(0, 6); }
        }

        private string GenerateCommentID()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 commentID FROM ForumComments ORDER BY commentID DESC", conn))
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                            return "FC" + (int.Parse(result.ToString().Substring(2)) + 1).ToString("D3");
                        return "FC001";
                    }
                }
            }
            catch { return "FC" + DateTime.Now.Ticks.ToString().Substring(0, 6); }
        }

        // ========== HELPERS ==========

        protected string GetInitials(object fname, object lname)
        {
            string f = fname?.ToString() ?? "";
            string l = lname?.ToString() ?? "";
            return (f.Length > 0 ? f.Substring(0, 1).ToUpper() : "") +
                   (l.Length > 0 ? l.Substring(0, 1).ToUpper() : "");
        }

        protected string GetAvatarStyle(string role)
        {
            return (role == "tutor" || role == "admin")
                ? "background: linear-gradient(135deg, #1d4ed8, #0ea5e9); color: white;"
                : "background-color: #e9ecef; color: #495057;";
        }

        protected string GetAvatarHtml(object fname, object lname, object role,
                                        object uploadProfile,
                                        string size = "40px", string fontSize = "1.1rem")
        {
            string profilePath = uploadProfile?.ToString();
            string styleSize = $"width:{size}; height:{size}; border-radius:50%; margin-right:12px; flex-shrink:0;";

            if (!string.IsNullOrEmpty(profilePath))
            {
                string resolvedUrl = ResolveUrl(profilePath);
                return $"<img src='{resolvedUrl}' " +
                       $"style='{styleSize} object-fit:cover;' " +
                       $"onerror=\"this.style.display='none'; this.nextElementSibling.style.display='flex';\" />" +
                       $"<div style='display:none; {GetAvatarStyle(role?.ToString())} {styleSize} " +
                       $"font-size:{fontSize}; align-items:center; justify-content:center; font-weight:700;'>" +
                       $"{GetInitials(fname, lname)}</div>";
            }

            return $"<div style='{GetAvatarStyle(role?.ToString())} {styleSize} " +
                   $"font-size:{fontSize}; display:flex; align-items:center; justify-content:center; font-weight:700;'>" +
                   $"{GetInitials(fname, lname)}</div>";
        }

        protected string GetRoleBadgeClass(string role)
        {
            return (role == "tutor" || role == "admin") ? "badge-role badge-tutor" : "badge-role badge-student";
        }

        protected string GetRoleBadgeHtml(string role)
        {
            return $"<span class='{GetRoleBadgeClass(role)}'>{role}</span>";
        }

        protected string GetRelativeTime(object createdDate)
        {
            if (createdDate == null || createdDate == DBNull.Value) return "";
            DateTime date = Convert.ToDateTime(createdDate);
            TimeSpan diff = DateTime.Now - date;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} minute{((int)diff.TotalMinutes != 1 ? "s" : "")} ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hour{((int)diff.TotalHours != 1 ? "s" : "")} ago";
            if (diff.TotalDays < 2) return "Yesterday, " + date.ToString("h:mm tt");
            if (diff.TotalDays < 7) return date.ToString("dddd, h:mm tt");
            return date.ToString("MMM dd, yyyy h:mm tt");
        }

        protected string EscapeForJs(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\"", "&quot;");
        }
    }
}