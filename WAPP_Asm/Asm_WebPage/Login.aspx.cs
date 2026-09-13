using Isopoh.Cryptography.Argon2;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class Login : System.Web.UI.Page
    {
        protected string RecaptchaSiteKey
        {
            get
            {
                return (
                    ConfigurationManager.AppSettings["RecaptchaSiteKey"] ?? ""
                ).Trim();
            }
        }

        protected string GoogleClientId
        {
            get
            {
                return (
                    ConfigurationManager.AppSettings["GoogleClientId"] ?? ""
                ).Trim();
            }
        }

        protected string GoogleAuthUri
        {
            get
            {
                return (
                    ConfigurationManager
                        .AppSettings["GoogleRedirectUri"] ?? ""
                ).Trim();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            DisableBrowserCaching();

            if (IsPostBack)
                return;

            ShowGoogleAuthenticationError();

            if (string.IsNullOrWhiteSpace(GoogleClientId))
            {
                ShowError(
                    "ⓘ Google Sign-In is not configured. " +
                    "Please contact the administrator."
                );
            }

            RedirectExistingAuthenticatedUser();
        }

        protected void BtnLogin_Click(object sender, EventArgs e)
        {

            lblMessage.Visible = false;
            btnReactivate.Visible = false;

            Page.Validate("LocalLogin");

            if (!Page.IsValid)
                return;

            string captchaResponse =
                Request.Form["g-recaptcha-response"];

            if (!VerifyRecaptcha())
                return;
            HideMessages();

            string username = (txtUsername.Text ?? "").Trim();

            // Do not trim passwords because spaces may be intentional.
            string password = txtPassword.Text ?? "";

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(password))
            {
                ShowError(
                    "ⓘ You must fill in both username and password."
                );
                return;
            }

            AuthenticateLocalUser(username, password);
        }

        private void AuthenticateLocalUser(
            string username,
            string password)
        {
            string connectionString =
                ConfigurationManager
                    .ConnectionStrings["KeyCodeDB"]
                    .ConnectionString;

            try
            {
                using (SqlConnection connection =
                    new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(@"
                    SELECT TOP (1)
                        userID,
                        username,
                        role,
                        pwd_hash,
                        status,
                        upload_profile
                    FROM dbo.Users
                    WHERE username = @username;", connection))
                {
                    command.Parameters.Add(
                        "@username",
                        SqlDbType.NVarChar,
                        50
                    ).Value = username;

                    connection.Open();

                    string userId;
                    string storedUsername;
                    string role;
                    string passwordHash;
                    string status;
                    string profile;

                    using (SqlDataReader reader =
                        command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            ShowInvalidCredentials();
                            return;
                        }

                        userId =
                            reader["userID"].ToString().Trim();

                        storedUsername =
                            reader["username"].ToString().Trim();

                        role =
                            reader["role"].ToString()
                                .Trim()
                                .ToLowerInvariant();

                        passwordHash =
                            reader["pwd_hash"].ToString();

                        status =
                            reader["status"].ToString().Trim();

                        profile =
                            reader["upload_profile"] == DBNull.Value
                                ? ""
                                : reader["upload_profile"].ToString();
                    }

                    if (!VerifyPassword(passwordHash, password))
                    {
                        ShowInvalidCredentials();
                        return;
                    }

                    if (status.Equals(
                        "Deleted",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        ShowInvalidCredentials();
                        return;
                    }

                    if (role.Equals(
                        "student",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        ShowError(
                            "ⓘ Students must use the " +
                            "Sign in with Google button."
                        );
                        return;
                    }

                    /*
                     * Do not permit unknown database roles.
                     */
                    if (!role.Equals(
                            "tutor",
                            StringComparison.OrdinalIgnoreCase) &&
                        !role.Equals(
                            "admin",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        ShowError(
                            "ⓘ This account is not allowed to use " +
                            "username and password login."
                        );
                        return;
                    }

                    if (status.Equals(
                        "Suspended",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        SetUserSession(
                            userId,
                            storedUsername,
                            role,
                            status,
                            profile
                        );

                        ShowError(
                            "ⓘ Your account has been suspended."
                        );

                        btnReactivate.Visible = true;
                        return;
                    }

                    if (!status.Equals(
                        "Active",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        ShowError(
                            "ⓘ This account is currently unavailable."
                        );
                        return;
                    }


                    if (role.Equals(
                            "tutor",
                            StringComparison.OrdinalIgnoreCase) &&
                        IsTutorTemporaryPassword(
                            passwordHash,
                            storedUsername))
                    {
                        SetUserSession(
                            userId,
                            storedUsername,
                            role,
                            status,
                            profile
                        );

                        Session["ForcePasswordChange"] = true;

                        Response.Redirect(
                            ResolveUrl(
                                "~/Asm_WebPage/ResetPassword.aspx"
                            ),
                            false
                        );

                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }

                    SetUserSession(
                        userId,
                        storedUsername,
                        role,
                        status,
                        profile
                    );

                    RedirectToDashboard(role);
                }
            }
            catch (SqlException exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Local login database error: " +
                    exception.Message
                );

                ShowError(
                    "ⓘ Sign-in is temporarily unavailable. " +
                    "Please try again later."
                );
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Local login error: " +
                    exception.Message
                );

                ShowError(
                    "ⓘ Sign-in could not be completed. " +
                    "Please try again."
                );
            }
        }

        private bool VerifyPassword(
            string storedHash,
            string enteredPassword)
        {
            if (string.IsNullOrWhiteSpace(storedHash) ||
                string.IsNullOrEmpty(enteredPassword))
            {
                return false;
            }

            /*
             * Only Argon2 password hashes are accepted.
             * Plain-text password comparison is intentionally excluded.
             */
            if (!storedHash.StartsWith(
                "$argon2",
                StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine(
                    "Rejected a non-Argon2 password value."
                );

                return false;
            }

            try
            {
                return Argon2.Verify(
                    storedHash,
                    enteredPassword
                );
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Password verification error: " +
                    exception.Message
                );

                return false;
            }
        }

        private bool IsTutorTemporaryPassword(
            string storedHash,
            string username)
        {
            if (string.IsNullOrWhiteSpace(storedHash) ||
                string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            try
            {
                return Argon2.Verify(
                    storedHash,
                    username.ToLowerInvariant()
                );
            }
            catch
            {
                return false;
            }
        }

        private bool VerifyRecaptcha()
        {
            string captchaResponse =
                Request.Form["g-recaptcha-response"];

            if (string.IsNullOrWhiteSpace(captchaResponse))
            {
                ShowError(
                    "ⓘ Please verify that you are not a robot."
                );

                return false;
            }

            string secretKey = (
                ConfigurationManager
                    .AppSettings["RecaptchaSecretKey"] ?? ""
            ).Trim();

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                ShowError(
                    "ⓘ reCAPTCHA is not configured. " +
                    "Please contact the administrator."
                );

                return false;
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    NameValueCollection values =
                        new NameValueCollection();

                    values["secret"] = secretKey;
                    values["response"] = captchaResponse;

                    if (!string.IsNullOrWhiteSpace(
                        Request.UserHostAddress))
                    {
                        values["remoteip"] =
                            Request.UserHostAddress;
                    }

                    byte[] responseBytes =
                        client.UploadValues(
                            "https://www.google.com/" +
                            "recaptcha/api/siteverify",
                            "POST",
                            values
                        );

                    string responseJson =
                        Encoding.UTF8.GetString(responseBytes);

                    JavaScriptSerializer serializer =
                        new JavaScriptSerializer();

                    RecaptchaResponse result =
                        serializer.Deserialize<RecaptchaResponse>(
                            responseJson
                        );

                    if (result == null || !result.success)
                    {
                        ShowError(
                            "ⓘ Captcha verification failed."
                        );

                        return false;
                    }

                    return true;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "reCAPTCHA verification error: " +
                    exception.Message
                );

                ShowError(
                    "ⓘ Captcha verification failed. " +
                    "Please try again."
                );

                return false;
            }
        }

        private void ShowGoogleAuthenticationError()
        {
            string googleError = Request.QueryString["googleError"];

            if (!string.IsNullOrWhiteSpace(googleError))
            {
                ShowError(googleError);
            }
            else
            {
                googleError =
                    Session["GoogleAuthError"] as string;

                if (!string.IsNullOrWhiteSpace(googleError))
                {
                    ShowError(googleError);
                    Session.Remove("GoogleAuthError");
                }
            }
        }

        private void RedirectExistingAuthenticatedUser()
        {
            if (Session["UserID"] == null ||
                Session["role"] == null)
            {
                return;
            }

            string status =
                Session["status"]?.ToString() ?? "";

            if (status.Equals(
                "Suspended",
                StringComparison.OrdinalIgnoreCase))
            {
                ShowError(
                    "ⓘ Your account has been suspended."
                );

                btnReactivate.Visible = true;
                return;
            }

            if (!status.Equals(
                "Active",
                StringComparison.OrdinalIgnoreCase))
            {
                Session.Clear();
                return;
            }

            RedirectToDashboard(
                Session["role"].ToString()
            );
        }

        private void SetUserSession(
            string userId,
            string username,
            string role,
            string status,
            string profile)
        {
            ClearPendingGoogleProfile();

            Session["UserID"] = userId;
            Session["username"] = username;
            Session["role"] = role;
            Session["status"] = status;
            Session["upload_profile"] = profile ?? "";
        }

        private void ClearPendingGoogleProfile()
        {
            Session.Remove("PendingGoogleSubject");
            Session.Remove("PendingGoogleEmail");
            Session.Remove("PendingGoogleFirstName");
            Session.Remove("PendingGoogleLastName");
            Session.Remove("PendingGoogleExpiresAt");
        }

        private void RedirectToDashboard(string role)
        {
            string destination;

            switch ((role ?? "").Trim().ToLowerInvariant())
            {
                case "tutor":
                    destination = ResolveUrl(
                        "~/Asm_WebPage/TutorDashboard.aspx"
                    );
                    break;

                case "admin":
                    destination = ResolveUrl(
                        "~/Asm_WebPage/AdminDashboard.aspx"
                    );
                    break;

                case "student":
                    destination = ResolveUrl(
                        "~/Asm_WebPage/StudentDashboard.aspx"
                    );
                    break;

                default:
                    Session.Clear();
                    ShowError(
                        "ⓘ Your account role is not recognized."
                    );
                    return;
            }

            Response.Redirect(destination, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void ShowInvalidCredentials()
        {
            ShowError(
                "ⓘ Invalid username or password!"
            );
        }

        private void ShowError(string message)
        {
            /*
             * lblMessage is an ASP.NET Literal, so encode the text
             * before displaying it.
             */
            lblMessage.Text =
                Server.HtmlEncode(message ?? "");

            lblMessage.Visible = true;
        }

        private void HideMessages()
        {
            lblMessage.Text = "";
            lblMessage.Visible = false;
            btnReactivate.Visible = false;
        }

        protected void btnReactivate_Click(
            object sender,
            EventArgs e)
        {
            Response.Redirect(
                ResolveUrl(
                    "~/Asm_WebPage/ReactivationRequest.aspx"
                ),
                false
            );

            Context.ApplicationInstance.CompleteRequest();
        }

        protected void BtnGuest_Click(
            object sender,
            EventArgs e)
        {
            Session.Clear();

            Session["username"] = "Visitor";
            Session["role"] = "Guest";
            Session["status"] = "Active";

            Response.Redirect(
                ResolveUrl(
                    "~/Asm_WebPage/StudentDashboard.aspx"
                ),
                false
            );

            Context.ApplicationInstance.CompleteRequest();
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

        private sealed class RecaptchaResponse
        {
            public bool success { get; set; }
        }
    }
}