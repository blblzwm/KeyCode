using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class ReactivationRequest : System.Web.UI.Page
    {
        private readonly string _connStr =
            ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        public override void VerifyRenderingInServerForm(Control control) { }

        private string CurrentCategory
        {
            get { return ViewState["Category"] != null ? ViewState["Category"].ToString() : "Student"; }
            set { ViewState["Category"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string role = Session["role"]?.ToString() ?? "";
            string status = Session["status"]?.ToString() ?? "";
            string userID = Session["UserID"]?.ToString() ?? "";

            bool isSuspended = string.Equals(status, "Suspended", StringComparison.OrdinalIgnoreCase);

            // Unauthenticated path: arrived here from ResetPassword.aspx after
            // the typed email matched a Suspended account. There is no login
            // session yet, so the account is looked up directly from the DB
            // rather than trusting Session["role"]/Session["status"].
            string reactivationUserID = Session["ReactivationUserID"] as string;

            if (string.IsNullOrEmpty(role) && !string.IsNullOrEmpty(reactivationUserID))
            {
                string dbStatus;

                if (!TryGetUserStatus(reactivationUserID, out dbStatus) ||
                    !string.Equals(dbStatus, "Suspended", StringComparison.OrdinalIgnoreCase))
                {
                    // Account no longer exists / is no longer suspended.
                    Session.Remove("ReactivationUserID");
                    Response.Redirect("~/Asm_WebPage/Login.aspx");
                    return;
                }

                pnlUser.Visible = true;
                pnlAdmin.Visible = false;

                if (!IsPostBack)
                {
                    txtUserID.Text = reactivationUserID;

                    if (HasPendingAppeal(reactivationUserID))
                    {
                        pnlAppealForm.Visible = false;
                        ScriptManager.RegisterStartupScript(this, GetType(), "pendingMsg",
                            "alert('You already have a pending appeal. Please wait for admin review.');", true);
                    }
                    else
                    {
                        pnlAppealForm.Visible = true;
                    }
                }

                return;
            }

            // No usable session at all -> back to Login.
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userID))
            {
                Response.Redirect("~/Asm_WebPage/Login.aspx");
                return;
            }

            // A suspended user (student, tutor, OR admin) always lands on the
            // appeal form. This must be checked BEFORE the role branches below,
            // otherwise a suspended admin gets routed into the admin dashboard
            // instead of being able to appeal their own suspension.
            if (isSuspended)
            {
                pnlUser.Visible = true;
                pnlAdmin.Visible = false;

                if (!IsPostBack)
                {
                    txtUserID.Text = userID;

                    if (HasPendingAppeal(userID))
                    {
                        pnlAppealForm.Visible = false;
                        ScriptManager.RegisterStartupScript(this, GetType(), "pendingMsg",
                            "alert('You already have a pending appeal. Please wait for admin review.');", true);
                    }
                    else
                    {
                        pnlAppealForm.Visible = true;
                    }
                }

                return;
            }

            if (role == "admin")
            {
                pnlAdmin.Visible = true;
                pnlUser.Visible = false;

                if (!IsPostBack)
                    BindGrid("Student");
            }
            else if (role == "student" || role == "tutor")
            {
                pnlUser.Visible = true;
                pnlAdmin.Visible = false;

                if (!IsPostBack)
                {
                    txtUserID.Text = userID;
                    pnlAppealForm.Visible = true;
                }
            }
            else
            {
                Response.Redirect("~/Asm_WebPage/Login.aspx");
            }
        }

        // Resolves the userID the appeal is being filed for from trusted
        // server-side session state, never from postback data. txtUserID is
        // ReadOnly, but a ReadOnly TextBox's value can still be edited in a
        // raw POST, so it must never be trusted for the DB write.
        private string GetTrustedAppealUserID()
        {
            string sessionUserID = Session["UserID"]?.ToString();
            if (!string.IsNullOrEmpty(sessionUserID)) return sessionUserID;

            return Session["ReactivationUserID"] as string;
        }

        // USER PANEL
        protected void btnSubmitAppeal_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string trustedUserID = GetTrustedAppealUserID();
            if (string.IsNullOrEmpty(trustedUserID))
            {
                Response.Redirect("~/Asm_WebPage/Login.aspx");
                return;
            }

            string attachmentPath = null;

            if (fuProof.HasFile)
            {
                string ext = System.IO.Path.GetExtension(fuProof.FileName).ToLower();

                if (ext != ".pdf" && ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    lblFileError.Text = "Only PDF, JPG or PNG files are allowed.";
                    lblFileError.Visible = true;
                    return;
                }

                if (fuProof.PostedFile.ContentLength > 5 * 1024 * 1024)
                {
                    lblFileError.Text = "File size must not exceed 5MB.";
                    lblFileError.Visible = true;
                    return;
                }

                string uploadFolder = Server.MapPath("~/Uploads/Reactivation/");
                if (!System.IO.Directory.Exists(uploadFolder))
                    System.IO.Directory.CreateDirectory(uploadFolder);

                string fileName = trustedUserID + "_" + DateTime.Now.Ticks + ext;
                string fullPath = System.IO.Path.Combine(uploadFolder, fileName);
                fuProof.SaveAs(fullPath);

                attachmentPath = "~/Uploads/Reactivation/" + fileName;
            }

            string sql = @"INSERT INTO ReactivationRequests (userID, reason, requestDate, status, attachmentPath)
           VALUES (@userID, @reason, GETDATE(), 'Pending', @attachmentPath)";

            SqlConnection con = new SqlConnection(_connStr);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@userID", trustedUserID);
                cmd.Parameters.AddWithValue("@reason", txtReason.Text.Trim());
                cmd.Parameters.AddWithValue("@attachmentPath", (object)attachmentPath ?? DBNull.Value);
                cmd.ExecuteNonQuery();

                pnlAppealForm.Visible = false;
                ScriptManager.RegisterStartupScript(this, GetType(), "appealSuccess",
                    "alert('Appeal submitted successfully! Admin will review your request.');", true);
            }
            catch
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "appealError",
                    "alert('Failed to submit appeal. Please try again.');", true);
            }
            finally { con.Close(); }
        }

        private bool TryGetUserStatus(string userId, out string status)
        {
            status = null;

            string sql = "SELECT status FROM Users WHERE userID = @userID";

            SqlConnection con = new SqlConnection(_connStr);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@userID", userId);

                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) return false;

                status = result.ToString().Trim();
                return true;
            }
            catch { return false; }
            finally { con.Close(); }
        }

        private bool HasPendingAppeal(string userId)
        {
            string sql = @"SELECT COUNT(1) FROM ReactivationRequests
                           WHERE userID = @userID AND status = 'Pending'";

            SqlConnection con = new SqlConnection(_connStr);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@userID", userId);
                return (int)cmd.ExecuteScalar() > 0;
            }
            catch { return false; }
            finally { con.Close(); }
        }

        // ADMIN PANEL — SIDEBAR
        protected void btnStudents_Click(object sender, EventArgs e)
        {
            SetActiveTab("Student");
        }

        protected void btnTutors_Click(object sender, EventArgs e)
        {
            SetActiveTab("Tutor");
        }

        private void SetActiveTab(string category)
        {
            btnStudents.CssClass = category == "Student" ? "nav-link-custom active" : "nav-link-custom";
            btnTutors.CssClass = category == "Tutor" ? "nav-link-custom active" : "nav-link-custom";
            lblCategoryTitle.Text = category;
            CurrentCategory = category;
            TextBox searchBox = pnlAdmin.FindControl("txtRequestSearch") as TextBox;
            if (searchBox != null) searchBox.Text = "";

            ViewState["SelectedRequestId"] = null;
            ViewState["SelectedUserId"] = null;
            ViewState["SelectedStatus"] = null;
            SyncActionButtons();
            BindGrid(category);
        }

        protected void RequestSearch_Changed(object sender, EventArgs e)
        {
            ViewState["SelectedRequestID"] = null;
            ViewState["SelectedUserID"] = null;
            ViewState["SelectedStatus"] = null;

            SyncActionButtons();
            BindGrid(CurrentCategory);
        }

        private void BindGrid(string category)
        {
            string sql = @"
                SELECT r.requestID, r.userID, r.requestDate, r.status, r.reason, r.attachmentPath
                FROM   ReactivationRequests r
                INNER JOIN Users u ON r.userID = u.userID
                WHERE  u.role = @role
                AND (@search = '' OR CONVERT(nvarchar(50), r.requestID) LIKE @pattern OR r.userID LIKE @pattern)
                ORDER BY
                    CASE WHEN r.status = 'Pending' THEN 0 ELSE 1 END,
                    r.requestDate DESC";

            SqlConnection con = new SqlConnection(_connStr);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                cmd.Parameters.AddWithValue("@role", category);
                TextBox searchBox = pnlAdmin.FindControl("txtRequestSearch") as TextBox;
                string search = searchBox == null ? "" : searchBox.Text.Trim();
                cmd.Parameters.Add("@search", SqlDbType.NVarChar, -1).Value = search;
                cmd.Parameters.Add("@pattern", SqlDbType.NVarChar, -1).Value = "%" +
                    search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";

                DataTable dt = new DataTable();
                da.Fill(dt);

                ViewState["GridData"] = dt;
                gvRequests.DataSource = dt;
                gvRequests.DataBind();
            }
            catch { }
            finally { con.Close(); }
        }

        private void RebindGrid()
        {
            DataTable dt = ViewState["GridData"] as DataTable;
            if (dt == null) return;
            gvRequests.DataSource = dt;
            gvRequests.DataBind();
        }

        protected void gvRequests_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["onclick"] =
                    Page.ClientScript.GetPostBackClientHyperlink(gvRequests, "Select$" + e.Row.RowIndex);
                e.Row.Style["cursor"] = "pointer";
            }
        }

        protected void gvRequests_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Select") return;

            int rowIndex = Convert.ToInt32(e.CommandArgument);
            DataTable dt = ViewState["GridData"] as DataTable;
            if (dt == null || rowIndex >= dt.Rows.Count) return;

            string requestId = dt.Rows[rowIndex]["requestID"].ToString();
            string userId = dt.Rows[rowIndex]["userID"].ToString();
            string status = dt.Rows[rowIndex]["status"].ToString();
            string current = ViewState["SelectedRequestID"]?.ToString();

            if (current == requestId)
            {
                ViewState["SelectedRequestID"] = null;
                ViewState["SelectedUserID"] = null;
                ViewState["SelectedStatus"] = null;
            }
            else
            {
                ViewState["SelectedRequestID"] = requestId;
                ViewState["SelectedUserID"] = userId;
                ViewState["SelectedStatus"] = status;
            }

            RebindGrid();
            SyncActionButtons();
        }

        protected void gvRequests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView row = (DataRowView)e.Row.DataItem;
            string status = row["status"].ToString();
            string requestId = row["requestID"].ToString();
            string selectedId = ViewState["SelectedRequestID"]?.ToString();

            if (!string.IsNullOrEmpty(selectedId) && selectedId == requestId)
                e.Row.CssClass = "selected-row";

            Label lblStatus = (Label)e.Row.FindControl("lblStatus");
            lblStatus.Text = $"<span class='badge-status status-{status.ToLower()}'>{status}</span>";
        }

        private void SyncActionButtons()
        {
            string status = ViewState["SelectedStatus"]?.ToString();
            bool isPendingSelected = status == "Pending";

            btnApprove.Visible = true;
            btnReject.Visible = true;

            btnApprove.Enabled = isPendingSelected;
            btnReject.Enabled = isPendingSelected;
        }

        // ADMIN PANEL — APPROVE / REJECT
        protected void btnApprove_Click(object sender, EventArgs e)
        {
            string requestId = ViewState["SelectedRequestID"]?.ToString();
            string userId = ViewState["SelectedUserID"]?.ToString();
            if (string.IsNullOrEmpty(requestId)) return;

            hdnRequestId.Value = requestId;
            hdnUserId.Value = userId;

            ProcessAppeal("Approved");
            UpdateUserStatus(userId, "Active");
            ClearSelection();
            BindGrid(CurrentCategory);
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            string requestId = ViewState["SelectedRequestID"]?.ToString();
            string userId = ViewState["SelectedUserID"]?.ToString();
            if (string.IsNullOrEmpty(requestId)) return;

            hdnRequestId.Value = requestId;
            ClearSelection();
            ProcessAppeal("Rejected");
            UpdateUserStatus(userId, "Deleted");
            BindGrid(CurrentCategory);
        }

        private void ClearSelection()
        {
            ViewState["SelectedRequestID"] = null;
            ViewState["SelectedUserID"] = null;
            ViewState["SelectedStatus"] = null;
            SyncActionButtons();
        }

        private void ProcessAppeal(string newStatus)
        {
            if (!int.TryParse(hdnRequestId.Value, out int requestId)) return;

            string sql = @"UPDATE ReactivationRequests
                           SET    status        = @status,
                                  processedBy   = @userID,
                                  processedDate = GETDATE()
                           WHERE  requestID     = @requestID";

            SqlConnection con = new SqlConnection(_connStr);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@status", newStatus);
                cmd.Parameters.AddWithValue("@userID", Session["UserID"].ToString());
                cmd.Parameters.AddWithValue("@requestID", requestId);
                cmd.ExecuteNonQuery();

                if (newStatus == "Approved")
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "approveMsg",
                        "alert('Appeal approved and user reactivated.');", true);
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "rejectMsg",
                        "alert('Appeal rejected.');", true);
                }
            }
            catch { }
            finally { con.Close(); }
        }

        private void UpdateUserStatus(string userId, string status)
        {
            SqlConnection con = new SqlConnection(_connStr);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Users SET status = @status WHERE userID = @userID", con);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@userID", userId);
                cmd.ExecuteNonQuery();
            }
            catch { }
            finally { con.Close(); }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Asm_WebPage/Login.aspx");
        }
    }
}