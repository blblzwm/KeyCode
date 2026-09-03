using Isopoh.Cryptography.Argon2;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Must have either a forced-change session OR a valid token in the URL
            if (Session["ForcePasswordChange"] == null && Request.QueryString["token"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                bool forcedChange = Session["ForcePasswordChange"] != null;

                if (forcedChange)
                {
                    // Tutor first-login UI tweaks
                    lblTitle.Text = "Set Your Password Here";
                    lblInstruction.Text = "For security, you must change your password before continuing. " +
                                         "Please also set a security question for account recovery.";
                    pnlSecuritySetup.Visible = true;
                    pnlBackLink.Visible = false; // can't skip this step
                }
                else
                {
                    string token = Request.QueryString["token"]?.Trim();
                    if (string.IsNullOrEmpty(token))
                        ShowError("ⓘ Invalid or missing reset token.");
                }
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            string token = Request.QueryString["token"]?.Trim();
            bool forcedChange = Session["ForcePasswordChange"] != null;

            if (!forcedChange && string.IsNullOrEmpty(token))
            {
                ShowError("ⓘ Invalid reset request.");
                return;
            }

            if (!string.IsNullOrEmpty(token) && token.Length > 200)
            {
                ShowError("ⓘ Invalid reset request.");
                return;
            }

            string password = txtPassword.Text.Trim();
            string confirm = txtConfirm.Text.Trim();

            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirm))
            {
                ShowError("ⓘ Please fill in both password fields.");
                return;
            }

            if (password != confirm)
            {
                ShowError("ⓘ Passwords do not match.");
                return;
            }

            if (!IsStrongPassword(password))
            {
                ShowError("ⓘ Password must contain at least 8 characters, 1 uppercase letter, 1 number and 1 symbol.");
                return;
            }

            // ── Validate security Q&A if this is a forced (tutor first-login) change ──
            string securityQuestion = null;
            string hashedAnswer = null;

            if (forcedChange)
            {
                securityQuestion = ddlSecurityQuestion.SelectedValue?.Trim();
                string rawAnswer = (txtSecurityAnswer.Text ?? "").Trim().ToLower();

                if (string.IsNullOrWhiteSpace(securityQuestion))
                {
                    ShowError("ⓘ Please select a security question.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(rawAnswer))
                {
                    ShowError("ⓘ Please provide an answer to your security question.");
                    return;
                }
                if (rawAnswer.Length > 100)
                {
                    ShowError("ⓘ Security answer is too long.");
                    return;
                }

                hashedAnswer = Argon2.Hash(rawAnswer);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    object userId = null;

                    if (forcedChange)
                    {
                        userId = Session["UserID"];
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand(@"
                            SELECT userID FROM Users
                            WHERE  reset_token = @Token
                            AND    reset_token_expiry > GETDATE()", conn);
                        cmd.Parameters.AddWithValue("@Token", token);
                        userId = cmd.ExecuteScalar();
                    }

                    if (userId == null)
                    {
                        ShowError("ⓘ Invalid or expired reset token.");
                        return;
                    }

                    string hashedPassword = Argon2.Hash(password);

                    if (forcedChange)
                    {
                        // Update password + save security Q&A in one shot
                        SqlCommand update = new SqlCommand(@"
                            UPDATE Users
                            SET pwd_hash          = @Pwd,
                                reset_token       = NULL,
                                reset_token_expiry= NULL,
                                security_question = @Sq,
                                security_ans_hash = @Sa
                            WHERE userID = @ID", conn);

                        update.Parameters.AddWithValue("@Pwd", hashedPassword);
                        update.Parameters.AddWithValue("@Sq", securityQuestion);
                        update.Parameters.AddWithValue("@Sa", hashedAnswer);
                        update.Parameters.AddWithValue("@ID", userId);
                        update.ExecuteNonQuery();
                    }
                    else
                    {
                        // Normal token-based reset — password only
                        SqlCommand update = new SqlCommand(@"
                            UPDATE Users
                            SET pwd_hash          = @Pwd,
                                reset_token       = NULL,
                                reset_token_expiry= NULL
                            WHERE userID = @ID", conn);

                        update.Parameters.AddWithValue("@Pwd", hashedPassword);
                        update.Parameters.AddWithValue("@ID", userId);
                        update.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Password reset error: " + ex.Message);
                ShowError("Something went wrong! Please try again later.");
                return;
            }

            txtPassword.Text = "";
            txtConfirm.Text = "";

            lblStatus.Visible = true;
            lblStatus.CssClass = "login-success-message";
            lblStatus.Text = "Password reset successful! Redirecting to login...";

            Session.Remove("ForcePasswordChange");

            Response.AddHeader("REFRESH", "2;URL=Login.aspx");
        }

        private void ShowError(string msg)
        {
            lblStatus.Visible = true;
            lblStatus.CssClass = "login-error-message";
            lblStatus.Text = msg;
        }

        private bool IsStrongPassword(string password)
        {
            return password.Length >= 8 &&
                   Regex.IsMatch(password, @"[A-Z]") &&
                   Regex.IsMatch(password, @"[0-9]") &&
                   Regex.IsMatch(password, @"[\W]");
        }
    }
}