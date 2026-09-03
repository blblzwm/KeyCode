using Isopoh.Cryptography.Argon2;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class SignUp : System.Web.UI.Page
    {
        protected void btnRegister_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;

            string username = txtUsername.Text.Trim().ToLower();
            string email = txtEmail.Text.Trim().ToLower();
            string password = txtPassword.Text.Trim();
            string confirm = txtConfirm.Text.Trim();
            string fname = txtFname.Text.Trim();
            string lname = txtLname.Text.Trim();
            DateTime dob;
            string captchaResponse = Request.Form["g-recaptcha-response"];

            // ── Empty field check ────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirm) ||
                string.IsNullOrWhiteSpace(fname) ||
                string.IsNullOrWhiteSpace(lname) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(txtDOB.Text))
            {
                ShowError("ⓘ All fields are required.");
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowError("ⓘ Invalid email format.");
                return;
            }

            if (password != confirm)
            {
                ShowError("ⓘ Passwords do not match.");
                return;
            }

            if (!IsStrongPassword(password))
            {
                ShowError("ⓘ Password must contain at least 8 characters, uppercase, number, and symbol.");
                return;
            }

            // ── Security question validation ─────────────────────────────
            string securityQuestion = ddlSecurityQuestion.SelectedValue.Trim();
            string securityAnswer = (txtSecurityAnswer.Text ?? "").Trim().ToLower();

            if (string.IsNullOrWhiteSpace(securityQuestion))
            {
                ShowError("ⓘ Please select a security question.");
                return;
            }
            if (string.IsNullOrWhiteSpace(securityAnswer))
            {
                ShowError("ⓘ Please provide an answer to your security question.");
                return;
            }
            if (securityAnswer.Length > 100)
            {
                ShowError("ⓘ Security answer is too long.");
                return;
            }

            // ── reCAPTCHA ────────────────────────────────────────────────
            if (string.IsNullOrEmpty(captchaResponse))
            {
                ShowError("Please verify you are not a robot.");
                return;
            }

            string secretKey = ConfigurationManager.AppSettings["RecaptchaSecretKey"];
            using (WebClient client = new WebClient())
            {
                NameValueCollection values = new NameValueCollection
                {
                    ["secret"] = secretKey,
                    ["response"] = captchaResponse
                };
                byte[] response = client.UploadValues(
                    "https://www.google.com/recaptcha/api/siteverify", values);

                string result = System.Text.Encoding.UTF8.GetString(response);
                JavaScriptSerializer js = new JavaScriptSerializer();
                dynamic captchaResult = js.Deserialize<dynamic>(result);

                if (!(bool)captchaResult["success"])
                {
                    ShowError("Captcha verification failed!");
                    return;
                }
            }

            // ── DOB ──────────────────────────────────────────────────────
            if (!DateTime.TryParse(txtDOB.Text, out dob))
            {
                ShowError("ⓘ Invalid date of birth.");
                return;
            }

            // ── Hash answer before touching DB ───────────────────────────
            string hashedAnswer = Argon2.Hash(securityAnswer);

            // ── Database ─────────────────────────────────────────────────
            string cs = ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                using (SqlCommand checkCmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM Users
                    WHERE LOWER(LTRIM(RTRIM(username))) = @u
                       OR LOWER(LTRIM(RTRIM(email)))    = @e", con))
                {
                    checkCmd.Parameters.AddWithValue("@u", username);
                    checkCmd.Parameters.AddWithValue("@e", email);
                    if ((int)checkCmd.ExecuteScalar() > 0)
                    {
                        ShowError("ⓘ Username or email already exists.");
                        return;
                    }
                }

                string role = "Student";
                int nextNumber;
                using (SqlCommand idCmd = new SqlCommand(@"
                    SELECT ISNULL(MAX(
                        TRY_CAST(SUBSTRING(userID,2,3) AS INT)
                    ),0) + 1
                    FROM Users
                    WHERE role   = @role
                    AND   userID LIKE 'S[0-9][0-9][0-9]'", con))
                {
                    idCmd.Parameters.AddWithValue("@role", role);
                    nextNumber = (int)idCmd.ExecuteScalar();
                }

                string userID = "S" + nextNumber.ToString("D3");
                string hashedPassword = Argon2.Hash(password);

                using (SqlCommand insertCmd = new SqlCommand(@"
                    INSERT INTO Users
                        (userID, username, fname, lname, pwd_hash, email, dob,
                         role, status, security_question, security_ans_hash)
                    VALUES
                        (@id, @u, @f, @l, @p, @e, @dob,
                         'Student', 'Active', @sq, @sa)", con))
                {
                    insertCmd.Parameters.AddWithValue("@id", userID);
                    insertCmd.Parameters.AddWithValue("@u", username);
                    insertCmd.Parameters.AddWithValue("@f", fname);
                    insertCmd.Parameters.AddWithValue("@l", lname);
                    insertCmd.Parameters.AddWithValue("@p", hashedPassword);
                    insertCmd.Parameters.AddWithValue("@e", email);
                    insertCmd.Parameters.AddWithValue("@dob", dob);
                    insertCmd.Parameters.AddWithValue("@sq", securityQuestion);
                    insertCmd.Parameters.AddWithValue("@sa", hashedAnswer);
                    insertCmd.ExecuteNonQuery();
                }
            }

            Response.Redirect("Login.aspx");
        }

        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.Visible = true;
        }

        private bool IsStrongPassword(string password)
        {
            return password.Length >= 8 &&
                   Regex.IsMatch(password, @"[A-Z]") &&
                   Regex.IsMatch(password, @"[0-9]") &&
                   Regex.IsMatch(password, @"[\W]");
        }

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod]
        public static bool UsernameExists(string username)
        {
            string cs = ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Users WHERE LOWER(LTRIM(RTRIM(username))) = @u", con))
                {
                    cmd.Parameters.Add("@u", SqlDbType.NVarChar, 50).Value = username.Trim().ToLower();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }
    }
}