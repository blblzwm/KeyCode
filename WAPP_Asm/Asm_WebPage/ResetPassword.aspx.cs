using Isopoh.Cryptography.Argon2;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        private readonly string cs =
            ConfigurationManager
                .ConnectionStrings["KeyCodeDB"]
                .ConnectionString;

        protected void Page_Init(object sender, EventArgs e)
        {
            if (Session != null)
            {
                ViewStateUserKey = Session.SessionID;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            DisableBrowserCaching();

            if (IsPostBack)
                return;

            /*
             * A new Tutor using a temporary password
             * proceeds directly to Step 3.
             */
            if (HasForcedPasswordChangeSession())
            {
                Session["ResetMode"] = "Forced";

                Session["ResetVerifiedUserID"] =
                    Session["UserID"].ToString();

                Session["ResetVerifiedAt"] =
                    DateTime.UtcNow;

                lblTitle.Text = "Set Your Password";

                lblInstruction.Text =
                    "Create a new password before continuing.";

                pnlBackLink.Visible = false;

                ShowStep(3);
                return;
            }

            ClearResetSession();
            ShowStep(1);
        }

        protected void btnSendOtp_Click(
            object sender,
            EventArgs e)
        {
            HideMessage();
            string email =
                (txtEmail.Text ?? "")
                    .Trim()
                    .ToLowerInvariant();

            if (!IsValidEmail(email))
            {
                ShowError(
                    "ⓘ Please enter a valid email address."
                );

                ShowStep(1);
                return;
            }

            /*
             * Do NOT auto-redirect suspended accounts here.
             * IssueOtp() already checks the account status and,
             * for a Suspended account, shows an error message on
             * this page plus a "Request Reactivation" link
             * (btnReactivate) instead of navigating away.
             * Redirecting straight to ReactivationRequest.aspx from
             * this click handler would bypass that UX entirely.
             */
            IssueOtp(email);
        }

        protected void btnResendOtp_Click(
            object sender,
            EventArgs e)
        {
            string email =
                Session["ResetRequestedEmail"]
                    as string;

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError(
                    "ⓘ Please enter your email address again."
                );

                ShowStep(1);
                return;
            }

            IssueOtp(email);
        }

        private void IssueOtp(string requestedEmail)
        {
            HideMessage();

            DateTime lastSent;

            if (TryReadSessionDate(
                    "ResetOtpLastSentAt",
                    out lastSent) &&
                DateTime.UtcNow.Subtract(lastSent)
                    .TotalSeconds < 60)
            {
                ShowError(
                    "ⓘ Please wait 60 seconds before " +
                    "requesting another OTP."
                );

                ShowStep(2);
                return;
            }

            ClearOtpAuthorization();

            Session["ResetRequestedEmail"] =
                requestedEmail;

            Session["ResetOtpAttempts"] = 0;

            string userId = null;
            string recipientEmail = null;
            string accountStatus = null;

            try
            {
                using (SqlConnection connection =
                    new SqlConnection(cs))
                using (SqlCommand command =
                    new SqlCommand(@"
                        SELECT TOP (1)
                            userID,
                            email, status
                        FROM dbo.Users
                        WHERE LOWER(LTRIM(RTRIM(email))) = @email
                          AND role IN ('Student', 'Tutor', 'Admin');",
                        connection))
                {
                    command.Parameters.Add(
                        "@email",
                        SqlDbType.NVarChar,
                        256
                    ).Value = requestedEmail;

                    connection.Open();

                    using (SqlDataReader reader =
                        command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            accountStatus = Convert.ToString(reader["status"]).Trim();
                            userId =
                                reader["userID"]
                                    .ToString()
                                    .Trim();

                            /*
                             * The recipient comes from the
                             * database—not Web.config.
                             */
                            recipientEmail =
                                reader["email"]
                                    .ToString()
                                    .Trim();
                        }
                    }
                }

                if (string.Equals(accountStatus, "Suspended", StringComparison.OrdinalIgnoreCase))
                {
                    /*
                     * btnReactivate stays hidden (Visible="false" in
                     * markup) — it exists only so it has a UniqueID
                     * that we can wire a postback to. The visible
                     * "link" is the anchor embedded below, inside the
                     * "reactivation appeal" phrase, rather than a
                     * separate standalone link.
                     */
                    string reactivateHref =
                        Page.ClientScript.GetPostBackClientHyperlink(
                            btnReactivate,
                            string.Empty
                        );

                    ShowError(
                        "Your account is suspended. Please submit a " +
                        "<a href=\"" + reactivateHref + "\" class=\"reactivate-link\">reactivation appeal</a>" +
                        " before resetting your password."
                    );

                    ShowStep(1);
                    return;
                }
                if (string.Equals(accountStatus, "Deleted", StringComparison.OrdinalIgnoreCase))
                {
                    ShowError("This account has been deleted and cannot reset its password. Please contact the administrator for assistance.");
                    ShowStep(1);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(userId) &&
                    !string.IsNullOrWhiteSpace(recipientEmail))
                {
                    string otp =
                        GenerateOtp();

                    string otpHash =
                        ComputeOtpHash(
                            userId,
                            otp
                        );

                    DateTime expiry =
                        DateTime.UtcNow.AddMinutes(10);

                    using (SqlConnection connection =
                        new SqlConnection(cs))
                    using (SqlCommand command =
                        new SqlCommand(@"
                            UPDATE dbo.Users
                            SET reset_token = @otpHash,
                                reset_token_expiry = @expiry
                            WHERE userID = @userID
                              AND role IN ('Student', 'Tutor', 'Admin')
                              AND status = 'Active';",
                            connection))
                    {
                        command.Parameters.Add(
                            "@otpHash",
                            SqlDbType.NVarChar,
                            100
                        ).Value = otpHash;

                        command.Parameters.Add(
                            "@expiry",
                            SqlDbType.DateTime
                        ).Value = expiry;

                        command.Parameters.Add(
                            "@userID",
                            SqlDbType.NVarChar,
                            50
                        ).Value = userId;

                        connection.Open();

                        if (command.ExecuteNonQuery() == 1)
                        {
                            SendOtpEmail(
                                recipientEmail,
                                otp
                            );

                            Session["ResetOtpUserID"] =
                                userId;
                        }
                    }
                }

                Session["ResetOtpLastSentAt"] =
                    DateTime.UtcNow;

                lblInstruction.Text =
                    "Enter the verification code sent " +
                    "to your registered email.";

                /*
                 * Keep this message generic so that people
                 * cannot discover registered email addresses.
                 */
                ShowSuccess(
                    "If this email belongs to an active account, an OTP has been sent. Please check your inbox and Junk folder."
                );

                ShowStep(2);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "OTP sending error: " +
                    exception.Message
                );

                ShowError(
                    "ⓘ The OTP could not be sent right now. " +
                    "Please try again later."
                );

                ShowStep(1);
            }
        }

        protected void btnVerifyOtp_Click(
            object sender,
            EventArgs e)
        {

            string otp =
                (txtOtp.Text ?? "").Trim();

            if (!Regex.IsMatch(otp, @"^\d{6}$"))
            {
                ShowError(
                    "ⓘ Please enter a valid six-digit OTP."
                );

                ShowStep(2);
                return;
            }

            int attempts =
                Session["ResetOtpAttempts"] == null
                    ? 0
                    : Convert.ToInt32(
                        Session["ResetOtpAttempts"]
                    );

            if (attempts >= 5)
            {
                ClearOtpAuthorization();

                ShowError(
                    "ⓘ Too many incorrect attempts. " +
                    "Please request a new OTP."
                );

                ShowStep(1);
                return;
            }

            Session["ResetOtpAttempts"] =
                attempts + 1;

            string userId =
                Session["ResetOtpUserID"]
                    as string;

            if (string.IsNullOrWhiteSpace(userId))
            {
                ShowError(
                    "ⓘ Invalid or expired OTP."
                );

                ShowStep(2);
                return;
            }

            string otpHash =
                ComputeOtpHash(
                    userId,
                    otp
                );

            try
            {
                /*
                 * This UPDATE verifies and consumes the OTP
                 * in one operation, making it single-use.
                 */
                using (SqlConnection connection =
                    new SqlConnection(cs))
                using (SqlCommand command =
                    new SqlCommand(@"
                        UPDATE dbo.Users
                        SET reset_token = NULL,
                            reset_token_expiry = NULL
                        WHERE userID = @userID
                          AND reset_token = @otpHash
                          AND reset_token_expiry > GETUTCDATE()
                          AND role IN ('Student', 'Tutor', 'Admin')
                          AND status = 'Active';",
                        connection))
                {
                    command.Parameters.Add(
                        "@userID",
                        SqlDbType.NVarChar,
                        50
                    ).Value = userId;

                    command.Parameters.Add(
                        "@otpHash",
                        SqlDbType.NVarChar,
                        100
                    ).Value = otpHash;

                    connection.Open();

                    if (command.ExecuteNonQuery() != 1)
                    {
                        ShowError(
                            "ⓘ Invalid or expired OTP."
                        );

                        ShowStep(2);
                        return;
                    }
                }

                Session["ResetMode"] = "OTP";

                Session["ResetVerifiedUserID"] =
                    userId;

                Session["ResetVerifiedAt"] =
                    DateTime.UtcNow;

                Session.Remove("ResetOtpUserID");
                Session.Remove("ResetOtpAttempts");
                Session.Remove("ResetRequestedEmail");
                Session.Remove("ResetOtpLastSentAt");

                lblInstruction.Text =
                    "Email verified. Enter your new password.";

                HideMessage();
                ShowStep(3);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "OTP verification error: " +
                    exception.Message
                );

                ShowError(
                    "ⓘ OTP verification is temporarily unavailable."
                );

                ShowStep(2);
            }
        }

        protected void btnReset_Click(
            object sender,
            EventArgs e)
        {
            HideMessage();

            string userId =
                Session["ResetVerifiedUserID"]
                    as string;

            DateTime verifiedAt;

            if (string.IsNullOrWhiteSpace(userId) ||
                !TryReadSessionDate(
                    "ResetVerifiedAt",
                    out verifiedAt) ||
                DateTime.UtcNow.Subtract(verifiedAt)
                    .TotalMinutes > 10)
            {
                ClearResetSession();

                ShowError(
                    "ⓘ Your password-reset session expired. " +
                    "Please request another OTP."
                );

                ShowStep(1);
                return;
            }

            string mode =
                Session["ResetMode"] as string;

            if (mode == "Forced")
            {
                if (!HasForcedPasswordChangeSession() ||
                    !string.Equals(
                        Session["UserID"].ToString(),
                        userId,
                        StringComparison.Ordinal))
                {
                    ClearResetSession();

                    ShowError(
                        "ⓘ Your password-change session expired."
                    );

                    ShowStep(1);
                    return;
                }
            }
            else if (mode != "OTP")
            {
                ClearResetSession();

                ShowError(
                    "ⓘ Please verify your email first."
                );

                ShowStep(1);
                return;
            }

            string password =
                txtPassword.Text ?? "";

            string confirmation =
                txtConfirm.Text ?? "";

            if (string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmation))
            {
                ShowError(
                    "ⓘ Please fill in both password fields."
                );

                ShowStep(3);
                return;
            }

            if (!string.Equals(
                password,
                confirmation,
                StringComparison.Ordinal))
            {
                ShowError(
                    "ⓘ Passwords do not match."
                );

                ShowStep(3);
                return;
            }

            if (!IsStrongPassword(password) ||
                password.Length > 128)
            {
                ShowError(
                    "ⓘ Password must have 8–128 characters, " +
                    "1 uppercase letter, 1 number and 1 symbol."
                );

                ShowStep(3);
                return;
            }

            try
            {
                string passwordHash =
                    Argon2.Hash(password);

                using (SqlConnection connection =
                    new SqlConnection(cs))
                using (SqlCommand command =
                    new SqlCommand(@"
                        UPDATE dbo.Users
                        SET pwd_hash = @passwordHash,
                            reset_token = NULL,
                            reset_token_expiry = NULL
                        WHERE userID = @userID
                          AND role IN ('Student', 'Tutor', 'Admin')
                          AND status = 'Active';",
                        connection))
                {
                    command.Parameters.Add(
                        "@passwordHash",
                        SqlDbType.NVarChar,
                        255
                    ).Value = passwordHash;

                    command.Parameters.Add(
                        "@userID",
                        SqlDbType.NVarChar,
                        50
                    ).Value = userId;

                    connection.Open();

                    if (command.ExecuteNonQuery() != 1)
                    {
                        ShowError(
                            "ⓘ The password could not be updated."
                        );

                        ShowStep(3);
                        return;
                    }
                }

                Session.Remove("ForcePasswordChange");
                ClearResetSession();

                ShowStep(0);

                ShowSuccess(
                    "Password reset successful! " +
                    "Redirecting to login..."
                );

                Response.AddHeader(
                    "REFRESH",
                    "2;URL=Login.aspx"
                );
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Password reset error: " +
                    exception.Message
                );

                ShowError(
                    "ⓘ Password reset is temporarily unavailable."
                );

                ShowStep(3);
            }
        }

        protected void btnReactivate_Click(object sender, EventArgs e)
        {
            string typedEmail =
                (txtEmail.Text ?? "")
                    .Trim()
                    .ToLowerInvariant();

            if (IsValidEmail(typedEmail) &&
                TryRedirectSuspendedAccountToReactivation(typedEmail))
            {
                return;
            }

            ShowError(
                "ⓘ We couldn't verify a suspended account for that email. " +
                "Please re-check the email address."
            );

            ShowStep(1);
        }

        /// <summary>
        /// Looks up the currently-typed email against dbo.Users. If a
        /// matching account exists and its status is 'Suspended', the
        /// account's userID is stashed in Session and the user is sent
        /// to ReactivationRequest.aspx instead of continuing the OTP
        /// verification flow. Returns true if a redirect was issued.
        /// </summary>
        private bool TryRedirectSuspendedAccountToReactivation(
            string email)
        {
            string userId = null;
            string status = null;

            try
            {
                using (SqlConnection connection =
                    new SqlConnection(cs))
                using (SqlCommand command =
                    new SqlCommand(@"
                        SELECT TOP (1)
                            userID,
                            status
                        FROM dbo.Users
                        WHERE LOWER(LTRIM(RTRIM(email))) = @email
                          AND role IN ('Student', 'Tutor', 'Admin');",
                        connection))
                {
                    command.Parameters.Add(
                        "@email",
                        SqlDbType.NVarChar,
                        256
                    ).Value = email;

                    connection.Open();

                    using (SqlDataReader reader =
                        command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userId =
                                reader["userID"]
                                    .ToString()
                                    .Trim();

                            status =
                                Convert.ToString(reader["status"])
                                    .Trim();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Reactivation lookup error: " +
                    exception.Message
                );

                return false;
            }

            if (string.IsNullOrWhiteSpace(userId) ||
                !string.Equals(
                    status,
                    "Suspended",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }


            ClearResetSession();

            Session["ReactivationUserID"] = userId;

            Response.Redirect(
                ResolveUrl("~/Asm_WebPage/ReactivationRequest.aspx"),
                false
            );

            Context.ApplicationInstance.CompleteRequest();

            return true;
        }

        private void SendOtpEmail(
            string recipientEmail,
            string otp)
        {
            /*
             * Fixed KEYCODE system sender.
             */
            string senderEmail =
                (ConfigurationManager
                    .AppSettings["PasswordResetFromEmail"] ?? "")
                .Trim();

            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                throw new ConfigurationErrorsException(
                    "PasswordResetFromEmail is missing."
                );
            }

            /*
             * recipientEmail is dynamic and comes from
             * dbo.Users.email.
             */
            using (MailMessage message =
                new MailMessage(
                    senderEmail,
                    recipientEmail))
            {
                message.Subject =
                    "Your KEYCODE password reset OTP";

                message.Body =
                    "Your KEYCODE verification code is: " +
                    otp +
                    Environment.NewLine +
                    Environment.NewLine +
                    "This code expires in 10 minutes." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "If you did not request a password reset, " +
                    "you can ignore this email.";

                message.IsBodyHtml = false;

                using (SmtpClient smtp =
                    new SmtpClient())
                {
                    smtp.Send(message);
                }
            }
        }

        private static string GenerateOtp()
        {
            byte[] randomBytes = new byte[4];

            using (RandomNumberGenerator generator =
                RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
            }

            uint value =
                BitConverter.ToUInt32(
                    randomBytes,
                    0
                );

            int otp =
                100000 +
                (int)(value % 900000);

            return otp.ToString("D6");
        }

        private static string ComputeOtpHash(
            string userId,
            string otp)
        {
            string pepper =
                (ConfigurationManager
                    .AppSettings["OtpPepper"] ?? "")
                .Trim();

            if (string.IsNullOrWhiteSpace(pepper))
            {
                throw new ConfigurationErrorsException(
                    "OtpPepper is missing."
                );
            }

            byte[] key =
                Encoding.UTF8.GetBytes(pepper);

            byte[] input =
                Encoding.UTF8.GetBytes(
                    userId + ":" + otp
                );

            using (HMACSHA256 hmac =
                new HMACSHA256(key))
            {
                byte[] hash =
                    hmac.ComputeHash(input);

                StringBuilder result =
                    new StringBuilder(hash.Length * 2);

                foreach (byte value in hash)
                {
                    result.Append(
                        value.ToString("x2")
                    );
                }

                return result.ToString();
            }
        }

        private bool HasForcedPasswordChangeSession()
        {
            if (Session["ForcePasswordChange"] == null ||
                Session["UserID"] == null ||
                Session["role"] == null)
            {
                return false;
            }

            string role =
                Session["role"]
                    .ToString()
                    .Trim()
                    .ToLowerInvariant();

            return role == "tutor" ||
                   role == "admin";
        }

        private void ShowStep(int step)
        {
            pnlEmailStep.Visible = step == 1;
            pnlOtpStep.Visible = step == 2;
            pnlPasswordStep.Visible = step == 3;

            dotStep1.CssClass =
                step == 1
                    ? "step-dot active"
                    : step > 1
                        ? "step-dot done"
                        : "step-dot";

            dotStep2.CssClass =
                step == 2
                    ? "step-dot active"
                    : step > 2
                        ? "step-dot done"
                        : "step-dot";

            dotStep3.CssClass =
                step == 3
                    ? "step-dot active"
                    : "step-dot";
        }

        private void ClearOtpAuthorization()
        {
            Session.Remove("ResetOtpUserID");
            Session.Remove("ResetOtpAttempts");
            Session.Remove("ResetVerifiedUserID");
            Session.Remove("ResetVerifiedAt");
            Session.Remove("ResetMode");
        }

        private void ClearResetSession()
        {
            ClearOtpAuthorization();

            Session.Remove("ResetRequestedEmail");
            Session.Remove("ResetOtpLastSentAt");
        }

        private static bool TryReadSessionDate(
            string key,
            out DateTime value)
        {
            value = DateTime.MinValue;

            HttpContext context =
                HttpContext.Current;

            if (context == null ||
                context.Session[key] == null)
            {
                return false;
            }

            object stored =
                context.Session[key];

            if (stored is DateTime)
            {
                value =
                    ((DateTime)stored)
                        .ToUniversalTime();

                return true;
            }

            return DateTime.TryParse(
                stored.ToString(),
                out value
            );
        }

        private static bool IsValidEmail(
            string email)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                email.Length > 256)
            {
                return false;
            }

            try
            {
                MailAddress parsed =
                    new MailAddress(email);

                return string.Equals(
                    parsed.Address,
                    email,
                    StringComparison.OrdinalIgnoreCase
                );
            }
            catch
            {
                return false;
            }
        }

        private static bool IsStrongPassword(
            string password)
        {
            return password.Length >= 8 &&
                   Regex.IsMatch(password, @"[A-Z]") &&
                   Regex.IsMatch(password, @"[0-9]") &&
                   Regex.IsMatch(
                       password,
                       @"[^a-zA-Z0-9]"
                   );
        }

        private void ShowError(string message)
        {
            lblStatus.Text = message;
            lblStatus.CssClass =
                "login-error-message";

            lblStatus.Visible = true;
        }

        private void ShowSuccess(string message)
        {
            lblStatus.Text = message;
            lblStatus.CssClass =
                "login-success-message";

            lblStatus.Visible = true;
        }

        private void HideMessage()
        {
            lblStatus.Text = "";
            lblStatus.Visible = false;
        }

        private void DisableBrowserCaching()
        {
            Response.Cache.SetCacheability(
                HttpCacheability.NoCache
            );

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddMinutes(-1)
            );

            Response.Cache.SetRevalidation(
                HttpCacheRevalidation.AllCaches
            );
        }
    }
}