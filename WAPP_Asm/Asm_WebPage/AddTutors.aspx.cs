using Isopoh.Cryptography.Argon2;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Optimization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class AddTutors : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            //security - check session
            if (Session["UserID"] == null || Session["role"] == null)
            {
                Response.Redirect("~/Asm_WebPage/Login.aspx");
                return;
            }

            string role = Session["role"].ToString().ToLower();

            if (role != "admin")
            {
                Response.Redirect("~/Asm_WebPage/AdminDashboard.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Auto-generate next Tutor ID
                GenerateNextTutorID();

                //set max date to 18 years old from today
                DateTime maxDate = DateTime.Today.AddYears(-18);
                txtDOB.Attributes["max"] = maxDate.ToString("yyyy-MM-dd");

                //set min date to 100 years old from now
                DateTime minDate = DateTime.Today.AddYears(-100);
                txtDOB.Attributes["min"] = minDate.ToString("yyyy-MM-dd");
            }
        }

        private void GenerateNextTutorID()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT TOP 1 userID 
                                   FROM Users 
                                   WHERE role = 'Tutor' 
                                   ORDER BY userID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            string lastID = result.ToString();
                            // Extract number from T001, T002, etc.
                            int number = int.Parse(lastID.Substring(1));
                            txtUserID.Text = "T" + (number + 1).ToString("D3");
                        }
                        else
                        {
                            // First tutor
                            txtUserID.Text = "T001";
                        }

                        // Make it readonly so user can't change it
                        txtUserID.ReadOnly = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error generating Tutor ID: " + ex.Message);
            }
        }

        protected void IsUsernameDuplicate(object source, ServerValidateEventArgs args)
        {
            string username = args.Value.Trim();

            SqlConnection conn = new SqlConnection(connectionString);
            string query = "SELECT COUNT(*) FROM Users WHERE username = @username AND status <> 'Deleted'";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            conn.Open();
            int count = (int)cmd.ExecuteScalar();

            args.IsValid = count == 0;  // true if not duplicate
            conn.Close();
        }

        protected void IsEmailDuplicate(object source, ServerValidateEventArgs args)
        {
            string email = args.Value.Trim();
            SqlConnection conn = new SqlConnection(connectionString);
            string query = "SELECT COUNT(*) FROM Users WHERE email = @email AND status <> 'Deleted'";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email.Trim());
            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            args.IsValid = count == 0;
            conn.Close();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                string hashedPassword = Argon2.Hash(txtUsername.Text.Trim().ToLower());

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO Users 
                                   (userID, username, fname, lname, pwd_hash, email, dob, role, qualification, status) 
                                   VALUES 
                                   (@userID, @username, @fname, @lname, @pwd_hash, @email, @dob, @role, @qualification, @status)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userID", txtUserID.Text.Trim());
                        cmd.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@fname", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@lname", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@pwd_hash", hashedPassword);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@dob", txtDOB.Text);
                        cmd.Parameters.AddWithValue("@role", "Tutor");
                        cmd.Parameters.AddWithValue("@qualification", ddlQualification.SelectedValue);
                        cmd.Parameters.AddWithValue("@status", "Active");

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "successMsg",
                                "alert('Tutor created successfully!'); window.location='UserManagement.aspx?role=Tutor';", true);
                        }
                        else
                        {
                            ShowMessage("Failed to create tutor.");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) // Duplicate key error
                {
                    ShowMessage("User ID or Username already exists. Please use a different value.");
                }
                else
                {
                    ShowMessage("Database error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error creating tutor: " + ex.Message);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Asm_WebPage/UserManagement.aspx?role=Tutor", false);
            Context.ApplicationInstance.CompleteRequest();
        }


        private void ShowMessage(string message)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                $"alert('{message.Replace("'", "\\'")}');", true);
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            string returnUrl = Request.QueryString["returnUrl"];

            if (!string.IsNullOrEmpty(returnUrl))
            {
                Response.Redirect(returnUrl);
                return;
            }
            else
            {
                Response.Redirect("~/Asm_WebPage/UserManagement.aspx");
            }
        }
    }
}