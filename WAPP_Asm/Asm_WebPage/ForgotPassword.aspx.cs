using Isopoh.Cryptography.Argon2;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using WAPP_Asm.Asm_WebPage;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class ForgotPassword : Page
    {
        string cs = ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) { }

        // ── STEP 1: Look up username, show their security question ────────
        protected void btnFindAccount_Click(object sender, EventArgs e)
        {
            string username = (txtUsername.Text ?? "").Trim().ToLower();

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("ⓘ Please enter your username.");
                return;
            }

            string question = null;

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT security_question
                    FROM   Users
                    WHERE  LOWER(LTRIM(RTRIM(username))) = @u
                    AND    status = 'Active'", conn);
                cmd.Parameters.AddWithValue("@u", username);

                object result = cmd.ExecuteScalar();
                question = result?.ToString();
            }

            // Store regardless — prevents user enumeration
            Session["ResetUsername"] = username;

            // Always move to Step 2 — same UI whether user exists or not
            ShowPanel(2);

            // Show real question if found, decoy if not
            litQuestion.Text = !string.IsNullOrEmpty(question)
                ? System.Web.HttpUtility.HtmlEncode(question)
                : "What was the name of your first pet?"; // decoy
        }

        // ── STEP 2: Verify DOB + security answer ─────────────────────────
        protected void btnVerifyIdentity_Click(object sender, EventArgs e)
        {
            string username = Session["ResetUsername"]?.ToString();

            if (string.IsNullOrEmpty(username))
            {
                Response.Redirect("ForgotPassword.aspx");
                return;
            }

            string enteredDobStr = (txtDOB.Text ?? "").Trim();
            string enteredAnswer = (txtAnswer.Text ?? "").Trim().ToLower();

            // Always the same generic error — never reveal which field failed
            const string genericError = "ⓘ Verification failed. Please check your details.";

            if (!DateTime.TryParse(enteredDobStr, out DateTime enteredDob))
            {
                ShowPanel(2);
                ShowError(genericError);
                return;
            }

            if (string.IsNullOrWhiteSpace(enteredAnswer))
            {
                ShowPanel(2);
                ShowError(genericError);
                return;
            }

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT userID, dob, security_ans_hash
                    FROM   Users
                    WHERE  LOWER(LTRIM(RTRIM(username))) = @u
                    AND    status = 'Active'", conn);
                cmd.Parameters.AddWithValue("@u", username);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        ShowPanel(2);
                        ShowError(genericError);
                        return;
                    }

                    string userId = dr["userID"].ToString();
                    DateTime storedDob = Convert.ToDateTime(dr["dob"]);
                    string storedHash = dr["security_ans_hash"]?.ToString();

                    // Check 1: Date of birth
                    if (enteredDob.Date != storedDob.Date)
                    {
                        ShowPanel(2);
                        ShowError(genericError);
                        return;
                    }

                    // Check 2: Security answer (Argon2)
                    bool answerOk = false;
                    try
                    {
                        if (!string.IsNullOrEmpty(storedHash))
                            answerOk = Argon2.Verify(storedHash, enteredAnswer);
                    }
                    catch { answerOk = false; }

                    if (!answerOk)
                    {
                        ShowPanel(2);
                        ShowError(genericError);
                        return;
                    }

                    // ✅ Both passed — store verified user, move to Step 3
                    Session["ResetVerifiedUserID"] = userId;
                    Session["ResetVerifiedAt"] = DateTime.UtcNow.ToString("o");
                    Session.Remove("ResetUsername");
                }
            }

            ShowPanel(3);
        }

        // ── STEP 3: Save new password ─────────────────────────────────────
        protected void btnReset_Click(object sender, EventArgs e)
        {
            // Guard — must have come through Step 2
            object userId = Session["ResetVerifiedUserID"];
            if (userId == null)
            {
                Response.Redirect("ForgotPassword.aspx");
                return;
            }

            // Enforce 15-minute window
            if (Session["ResetVerifiedAt"] != null)
            {
                DateTime verifiedAt = DateTime.Parse(Session["ResetVerifiedAt"].ToString());
                if ((DateTime.UtcNow - verifiedAt).TotalMinutes > 15)
                {
                    Session.Remove("ResetVerifiedUserID");
                    Session.Remove("ResetVerifiedAt");
                    ShowPanel(1);
                    ShowError("ⓘ Session expired. Please start again.");
                    return;
                }
            }

            string password = (txtPassword.Text ?? "").Trim();
            string confirm = (txtConfirm.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirm))
            {
                ShowPanel(3);
                ShowError("ⓘ Please fill in both password fields.");
                return;
            }
            if (password != confirm)
            {
                ShowPanel(3);
                ShowError("ⓘ Passwords do not match.");
                return;
            }
            if (!IsStrongPassword(password))
            {
                ShowPanel(3);
                ShowError("ⓘ Password must have 8+ chars, 1 uppercase, 1 number, 1 symbol.");
                return;
            }

            try
            {
                string hashedPassword = Argon2.Hash(password);

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        UPDATE Users
                        SET    pwd_hash = @Pwd
                        WHERE  userID   = @ID", conn);
                    cmd.Parameters.AddWithValue("@Pwd", hashedPassword);
                    cmd.Parameters.AddWithValue("@ID", userId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Reset error: " + ex.Message);
                ShowPanel(3);
                ShowError("Something went wrong. Please try again.");
                return;
            }

            // Clear all reset sessions
            Session.Remove("ResetVerifiedUserID");
            Session.Remove("ResetVerifiedAt");

            // Show success
            ShowPanel(0); // hide all panels
            lblMessage.CssClass = "login-success-message";
            lblMessage.Text = "Password reset successful! Redirecting to login...";
            lblMessage.Visible = true;

            Response.AddHeader("REFRESH", "2;URL=Login.aspx");
        }

        // ── Helpers ───────────────────────────────────────────────────────

        // Shows only the requested step panel (pass 0 to hide all)
        private void ShowPanel(int step)
        {
            lblMessage.Visible = false;

            pnlStep1.Visible = (step == 1);
            pnlStep2.Visible = (step == 2);
            pnlStep3.Visible = (step == 3);
        }

        private void ShowError(string msg)
        {
            lblMessage.CssClass = "login-error-message";
            lblMessage.Text = msg;
            lblMessage.Visible = true;
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
