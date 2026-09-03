using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class UserManagement : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        public override void VerifyRenderingInServerForm(Control control)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string role = Request.QueryString["role"];
                if (string.IsNullOrEmpty(role)) role = "Student";

                ViewState["CurrentRole"] = role;
                ViewState["SelectedUserID"] = null;
                ViewState["SelectedStatus"] = null;

                LoadUserData(role);
            }
        }

        private void LoadUserData(string role)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                                        userID as UserID, 
                                        username as Username, 
                                        fname as FirstName, 
                                        lname as LastName, 
                                        email as Email, 
                                        dob as DOB, 
                                        qualification as Qualification, 
                                        status as Status
                                     FROM Users 
                                     WHERE role = @Role 
                                     ORDER BY userID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Role", role);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ViewState["GridData"] = dt;

                        ConfigureGridColumns(role);
                        gvUsers.DataSource = dt;
                        gvUsers.DataBind();
                    }
                }

                UpdateUIForRole(role);
                SyncActionButtons();
            }
            catch (Exception ex)
            {
            }
        }

        private void RebindGrid()
        {
            DataTable dt = ViewState["GridData"] as DataTable;
            if (dt != null)
            {
                string role = ViewState["CurrentRole"]?.ToString() ?? "Student";
                ConfigureGridColumns(role);
                gvUsers.DataSource = dt;
                gvUsers.DataBind();
            }
        }

        private void ConfigureGridColumns(string role)
        {
            gvUsers.Columns.Clear();

            gvUsers.Columns.Add(new BoundField { DataField = "UserID", HeaderText = "ID" });
            gvUsers.Columns.Add(new BoundField { DataField = "Username", HeaderText = "Username" });
            gvUsers.Columns.Add(new BoundField { DataField = "FirstName", HeaderText = "First Name" });
            gvUsers.Columns.Add(new BoundField { DataField = "LastName", HeaderText = "Last Name" });
            gvUsers.Columns.Add(new BoundField { DataField = "Email", HeaderText = "Email" });
            gvUsers.Columns.Add(new BoundField { DataField = "DOB", HeaderText = "DOB", DataFormatString = "{0:yyyy-MM-dd}" });

            if (role == "Tutor")
            {
                gvUsers.Columns.Add(new BoundField { DataField = "Qualification", HeaderText = "Qualification" });
            }

            TemplateField statusField = new TemplateField();
            statusField.HeaderText = "Status";
            statusField.ItemTemplate = new StatusTemplate();
            gvUsers.Columns.Add(statusField);
        }

        private void UpdateUIForRole(string role)
        {
            lblPageTitle.Text = "Registered " + role + "s";
            btnStudent.CssClass = (role == "Student") ? "nav-link-custom active" : "nav-link-custom";
            btnTutor.CssClass = (role == "Tutor") ? "nav-link-custom active" : "nav-link-custom";
            btnCreate.Visible = (role == "Tutor");
            btnEdit.Visible = (role == "Tutor");

            btnSuspend.Visible = true;
            btnDelete.Visible = true;
        }

        protected void btnStudent_Click(object sender, EventArgs e)
        {
            ViewState["CurrentRole"] = "Student";
            ViewState["SelectedUserID"] = null;
            ViewState["SelectedStatus"] = null;
            txtSearch.Text = "";

            LoadUserData("Student");
        }

        protected void btnTutor_Click(object sender, EventArgs e)
        {
            ViewState["CurrentRole"] = "Tutor";
            ViewState["SelectedUserID"] = null;
            ViewState["SelectedStatus"] = null;
            txtSearch.Text = "";

            LoadUserData("Tutor");
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            string selectedUserID = ViewState["SelectedUserID"]?.ToString();
            if (!string.IsNullOrEmpty(selectedUserID))
            {
                Response.Redirect($"Profile.aspx?id={selectedUserID}&returnUrl=UserManagement.aspx?role=Tutor");
            }
        }

        protected void gvUsers_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvUsers, "Select$" + e.Row.RowIndex);
                e.Row.Style["cursor"] = "pointer";
            }
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string userID = DataBinder.Eval(e.Row.DataItem, "UserID").ToString();
                string selectedUserID = ViewState["SelectedUserID"]?.ToString();

                // Highlight if this row is selected
                if (!string.IsNullOrEmpty(selectedUserID) && selectedUserID == userID)
                {
                    e.Row.CssClass = "selected-row";
                }
            }
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);

                if (rowIndex < 0 || rowIndex >= gvUsers.DataKeys.Count)
                    return;

                DataTable dt = ViewState["GridData"] as DataTable;
                if (dt == null || rowIndex >= dt.Rows.Count)
                    return;

                string userID = gvUsers.DataKeys[rowIndex].Value.ToString();
                string status = dt.Rows[rowIndex]["Status"].ToString();
                string currentSelected = ViewState["SelectedUserID"]?.ToString();

                if (currentSelected == userID)
                {
                    // Deselect
                    ViewState["SelectedUserID"] = null;
                    ViewState["SelectedStatus"] = null;

                }
                else
                {
                    // Select new row
                    ViewState["SelectedUserID"] = userID;
                    ViewState["SelectedStatus"] = status;

                }

                RebindGrid();
                SyncActionButtons();
            }
        }
        private void SyncActionButtons()
        {
            string selectedUserID = ViewState["SelectedUserID"]?.ToString();
            string selectedStatus = ViewState["SelectedStatus"]?.ToString();

            if (string.IsNullOrEmpty(selectedUserID) || string.IsNullOrEmpty(selectedStatus))
            {
                btnSuspend.Enabled = false;
                btnDelete.Enabled = false;
                btnEdit.Enabled = false;
                return;
            }

            btnSuspend.Enabled = (selectedStatus == "Active");

            btnDelete.Enabled = (selectedStatus == "Active" || selectedStatus == "Suspended");

            string currentRole = ViewState["CurrentRole"]?.ToString();
            btnEdit.Enabled = (currentRole == "Tutor" && selectedStatus != "Deleted");
        }

        protected void btnSuspend_Click(object sender, EventArgs e)
        {
            string selectedUserID = ViewState["SelectedUserID"]?.ToString();
            string selectedStatus = ViewState["SelectedStatus"]?.ToString();

            if (!string.IsNullOrEmpty(selectedUserID))
            {
                UpdateUserStatus(selectedUserID, "Suspended");
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            string selectedUserID = ViewState["SelectedUserID"]?.ToString();
            string selectedStatus = ViewState["SelectedStatus"]?.ToString();

            if (!string.IsNullOrEmpty(selectedUserID))
            {
                UpdateUserStatus(selectedUserID, "Deleted");
            }
        }

        private void UpdateUserStatus(string userID, string newStatus)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Users SET status = @Status WHERE userID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", newStatus);
                        cmd.Parameters.AddWithValue("@UserID", userID);

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            LoadUserData(ViewState["CurrentRole"].ToString());
                            ViewState["SelectedUserID"] = null;
                            ViewState["SelectedStatus"] = null;
                            SyncActionButtons();

                            string msg = newStatus == "Suspended" ? "User has been suspended successfully."
                                       : newStatus == "Deleted" ? "User has been deleted successfully."
                                       : $"User status updated to {newStatus} successfully.";

                            ScriptManager.RegisterStartupScript(this, GetType(), "statusMsg",
                                $"alert('{msg}');", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            Response.Redirect($"AddTutors.aspx?returnUrl=UserManagement.aspx?role=Tutor");
        }

        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            string role = ViewState["CurrentRole"]?.ToString() ?? "Student";

            ViewState["SelectedUserID"] = null;
            ViewState["SelectedStatus"] = null;


            if (string.IsNullOrEmpty(searchTerm))
            {
                pnlNoUser.Visible = false;
                LoadUserData(role);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                                        userID as UserID, 
                                        username as Username, 
                                        fname as FirstName, 
                                        lname as LastName, 
                                        email as Email, 
                                        dob as DOB, 
                                        qualification as Qualification, 
                                        status as Status
                                     FROM Users 
                                     WHERE role = @Role 
                                     AND (userID LIKE @Search 
                                          OR username LIKE @Search 
                                          OR fname LIKE @Search 
                                          OR lname LIKE @Search)
                                     ORDER BY userID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ViewState["GridData"] = dt;

                        ConfigureGridColumns(role);
                        gvUsers.DataSource = dt;
                        gvUsers.DataBind();

                        if (dt.Rows.Count == 0)
                        {
                            HideActionButtons();
                            pnlNoUser.Visible = true;
                        }
                        else
                        {
                            pnlNoUser.Visible = false;
                        }
                    }
                }
                SyncActionButtons();
            }
            catch (Exception ex)
            {
            }
        }
        private void HideActionButtons()
        {
            btnEdit.Visible = false;
            btnSuspend.Visible = false;
            btnDelete.Visible = false;
            btnCreate.Visible = false;
        }

        private class StatusTemplate : ITemplate
        {
            public void InstantiateIn(Control container)
            {
                Literal lit = new Literal();
                lit.DataBinding += new EventHandler(lit_DataBinding);
                container.Controls.Add(lit);
            }

            private void lit_DataBinding(object sender, EventArgs e)
            {
                Literal lit = (Literal)sender;
                GridViewRow row = (GridViewRow)lit.NamingContainer;

                object statusObj = DataBinder.Eval(row.DataItem, "Status");
                if (statusObj == null)
                    return;

                string status = statusObj.ToString();

                string cssClass = "text-status";
                if (status == "Active")
                    cssClass += " status-active";
                else if (status == "Suspended")
                    cssClass += " status-suspended";
                else if (status == "Deleted")
                    cssClass += " status-deleted";

                lit.Text = $"<span class='{cssClass}'>{status}</span>";
            }
        }
    }
}