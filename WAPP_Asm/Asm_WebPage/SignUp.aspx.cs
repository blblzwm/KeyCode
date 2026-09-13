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

        protected void Page_Init(
            object sender,
            EventArgs e)
        {
            if (Session != null)
                ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            const int minimumAge = 5;
            const int maximumAge = 100;

            // The browser date picker will only allow ages 5–100.
            txtDOB.Attributes["max"] =
                DateTime.Today
                    .AddYears(-minimumAge)
                    .ToString("yyyy-MM-dd");

            txtDOB.Attributes["min"] =
                DateTime.Today
                    .AddYears(-maximumAge)
                    .ToString("yyyy-MM-dd");

            if (!HasValidPendingGoogleProfile())
            {
                ClearPendingGoogleProfile();

                Response.Redirect(
                    ResolveUrl("~/Asm_WebPage/Login.aspx") +
                    "?googleError=" +
                    Server.UrlEncode(
                        "Please verify your Google Account " +
                        "before registering."
                    ),
                    false
                );

                Context.ApplicationInstance
                    .CompleteRequest();

                return;
            }

            string verifiedEmail =
                Session["PendingGoogleEmail"]
                    .ToString()
                    .Trim()
                    .ToLowerInvariant();

            /*
             * Always use the Google-verified email.
             * Never trust an email sent from the browser.
             */
            txtEmail.Text = verifiedEmail;
            txtEmail.ReadOnly = true;

            if (!IsPostBack)
            {
                txtUsername.Text = GetUsernameFromEmail(verifiedEmail);
            }
        }

        private string GetUsernameFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "";

            int atPosition = email.IndexOf('@');

            string suggestedUsername =
                atPosition > 0
                    ? email.Substring(0, atPosition)
                    : email;

            // Keep only characters accepted by the username field.
            suggestedUsername = System.Text.RegularExpressions.Regex.Replace(
                suggestedUsername,
                @"[^a-zA-Z0-9_-]",
                ""
            );

            if (suggestedUsername.Length > 20)
                suggestedUsername = suggestedUsername.Substring(0, 20);

            return suggestedUsername;
        }

        private string BuildSuggestedUsername(
    string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return "student";
            }

            /*
             * Extract everything before @.
             *
             * john.doe@gmail.com becomes john.doe
             */
            string localPart =
                email.Split('@')[0]
                    .Trim()
                    .ToLowerInvariant();

            /*
             * Your username validation only allows letters,
             * numbers, underscores and hyphens.
             *
             * john.doe becomes johndoe
             */
            string username =
                Regex.Replace(
                    localPart,
                    @"[^a-z0-9_-]",
                    ""
                );

            /*
             * Prevent consecutive underscores or hyphens.
             */
            username =
                Regex.Replace(
                    username,
                    @"[_-]{2,}",
                    "_"
                );

            username =
                username.Trim('_', '-');

            /*
             * Your username rule requires at least three letters.
             */
            if (!Regex.IsMatch(
                username,
                @"^(?=(?:.*[a-z]){3,})"))
            {
                username =
                    "student" + username;
            }

            /*
             * Database/UI maximum is 20 characters.
             */
            if (username.Length > 20)
            {
                username =
                    username.Substring(0, 20);
            }

            username =
                username.Trim('_', '-');

            if (string.IsNullOrWhiteSpace(username))
            {
                username = "student";
            }

            return username;
        }

        protected void btnRegister_Click(
            object sender,
            EventArgs e)
        {
            lblMessage.Visible = false;

            if (!HasValidPendingGoogleProfile())
            {
                ClearPendingGoogleProfile();

                Response.Redirect(
                    ResolveUrl("~/Asm_WebPage/Login.aspx") +
                    "?googleError=" +
                    Server.UrlEncode(
                        "Google verification succeeded, but the pending registration session was lost."
                    ),
                    false
                );

                Context.ApplicationInstance
                    .CompleteRequest();

                return;
            }

            string googleSubject =
                Session["PendingGoogleSubject"]
                    .ToString()
                    .Trim();

            string email =
                Session["PendingGoogleEmail"]
                    .ToString()
                    .Trim()
                    .ToLowerInvariant();

            string username =
                (txtUsername.Text ?? "")
                    .Trim()
                    .ToLowerInvariant();

            string fname =
                (txtFname.Text ?? "").Trim();

            string lname =
                (txtLname.Text ?? "").Trim();

            string captchaResponse =
                Request.Form["g-recaptcha-response"];

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(fname) ||
                string.IsNullOrWhiteSpace(lname) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(txtDOB.Text))
            {
                ShowError("ⓘ All fields are required.");
                return;
            }

            if (!Regex.IsMatch(
                    username,
                    @"^(?=(?:.*[A-Za-z]){3,})" +
                    @"(?!.*[_-]{2})" +
                    @"[A-Za-z0-9]" +
                    @"[A-Za-z0-9_-]{1,18}" +
                    @"[A-Za-z0-9]$"))
            {
                ShowError(
                    "ⓘ Username must be 3–20 characters " +
                    "and contain at least 3 letters."
                );

                return;
            }

            if (fname.Length > 50 ||
                lname.Length > 50)
            {
                ShowError(
                    "ⓘ First and last names must not " +
                    "exceed 50 characters."
                );

                return;
            }

            if (string.IsNullOrWhiteSpace(email) || email.Length > 256)
            {
                ShowError(
                    "ⓘ Google returned an unsupported email address."
                );

                return;
            }


            DateTime dob;

            if (!DateTime.TryParse(txtDOB.Text, out dob))
            {
                ShowError("ⓘ Please enter a valid date of birth.");
                return;
            }

            dob = dob.Date;

            const int minimumAge = 5;
            const int maximumAge = 100;

            DateTime latestAllowedDOB =
                DateTime.Today.AddYears(-minimumAge);

            DateTime earliestAllowedDOB =
                DateTime.Today.AddYears(-maximumAge);

            if (dob > latestAllowedDOB)
            {
                ShowError(
                    "ⓘ Student must be at least " +
                    minimumAge +
                    " years old."
                );

                return;
            }

            if (dob < earliestAllowedDOB)
            {
                ShowError("ⓘ Please enter a valid date of birth.");
                return;
            }

            if (!VerifyRecaptcha(captchaResponse))
                return;

            string cs =
                ConfigurationManager
                    .ConnectionStrings["KeyCodeDB"]
                    .ConnectionString;

            string createdUserID = null;

            try
            {
                using (
                    SqlConnection con =
                        new SqlConnection(cs))
                {
                    con.Open();

                    using (
                        SqlTransaction transaction =
                            con.BeginTransaction(
                                IsolationLevel.Serializable))
                    {
                        using (
                            SqlCommand checkCommand =
                                new SqlCommand(@"
                                    SELECT COUNT(*)
                                    FROM dbo.Users
                                         WITH (UPDLOCK, HOLDLOCK)
                                    WHERE
                                        LOWER(LTRIM(RTRIM(username)))
                                            = @username
                                        OR
                                        LOWER(LTRIM(RTRIM(email)))
                                            = @email
                                        OR
                                        google_subject
                                            = @subject",
                                    con,
                                    transaction))
                        {
                            checkCommand.Parameters.Add(
                                "@username",
                                SqlDbType.NVarChar,
                                50
                            ).Value = username;

                            checkCommand.Parameters.Add(
                                "@email",
                                SqlDbType.NVarChar,
                                256
                            ).Value = email;

                            checkCommand.Parameters.Add(
                                "@subject",
                                SqlDbType.NVarChar,
                                255
                            ).Value = googleSubject;

                            int existing =
                                Convert.ToInt32(
                                    checkCommand.ExecuteScalar()
                                );

                            if (existing > 0)
                            {
                                ShowError(
                                    "ⓘ Username, email, or " +
                                    "Google Account already exists."
                                );

                                return;
                            }
                        }

                        int nextNumber;

                        using (
                            SqlCommand idCommand =
                                new SqlCommand(@"
                                    SELECT
                                        ISNULL(
                                            MAX(
                                                TRY_CAST(
                                                    SUBSTRING(
                                                        userID,
                                                        2,
                                                        3
                                                    ) AS INT
                                                )
                                            ),
                                            0
                                        ) + 1
                                    FROM dbo.Users
                                         WITH (UPDLOCK, HOLDLOCK)
                                    WHERE role = 'Student'
                                      AND userID LIKE
                                          'S[0-9][0-9][0-9]'",
                                    con,
                                    transaction))
                        {
                            nextNumber =
                                Convert.ToInt32(
                                    idCommand.ExecuteScalar()
                                );
                        }

                        if (nextNumber > 999)
                        {
                            throw new InvalidOperationException(
                                "Student user ID range is exhausted."
                            );
                        }

                        createdUserID =
                            "S" + nextNumber.ToString("D3");

                        using (SqlCommand insertCommand = new SqlCommand(@"
                            INSERT INTO dbo.Users
                            (
                                userID,
                                username,
                                fname,
                                lname,
                                email,
                                dob,
                                role,
                                status,
                                google_subject
                            )
                            VALUES
                            (
                                @id,
                                @username,
                                @fname,
                                @lname,
                                @email,
                                @dob,
                                'Student',
                                'Active',
                                @subject
                            )",
                            con,
                            transaction))
                        {
                            insertCommand.Parameters.Add(
                                "@id",
                                SqlDbType.NVarChar,
                                50
                            ).Value = createdUserID;

                            insertCommand.Parameters.Add(
                                "@username",
                                SqlDbType.NVarChar,
                                50
                            ).Value = username;

                            insertCommand.Parameters.Add(
                                "@fname",
                                SqlDbType.NVarChar,
                                50
                            ).Value = fname;

                            insertCommand.Parameters.Add(
                                "@lname",
                                SqlDbType.NVarChar,
                                50
                            ).Value = lname;

                            insertCommand.Parameters.Add(
                                "@email",
                                SqlDbType.NVarChar,
                                256
                            ).Value = email;

                            insertCommand.Parameters.Add(
                                "@dob",
                                SqlDbType.Date
                            ).Value = dob.Date;

                            insertCommand.Parameters.Add(
                                "@subject",
                                SqlDbType.NVarChar,
                                255
                            ).Value = googleSubject;

                            insertCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Registration SQL error: " + ex.ToString()
                );

                ShowError(
                    "ⓘ Database error " +
                    ex.Number +
                    ": " +
                    ex.Message
                );

                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Registration error: " + ex.ToString()
                );

                ShowError(
                    "ⓘ Registration error: " + ex.Message
                );

                return;
            }

            /*
             * Registration succeeded. Sign the Student into
             * KEYCODE and go directly to their dashboard.
             */
            ClearPendingGoogleProfile();

            Session["UserID"] = createdUserID;
            Session["username"] = username;
            Session["role"] = "student";
            Session["status"] = "Active";
            Session["upload_profile"] = "";

            Response.Redirect(
                ResolveUrl(
                    "~/Asm_WebPage/StudentDashboard.aspx"
                ),
                false
            );

            Context.ApplicationInstance.CompleteRequest();
        }

        private bool VerifyRecaptcha(
            string captchaResponse)
        {
            if (string.IsNullOrWhiteSpace(
                    captchaResponse))
            {
                ShowError(
                    "ⓘ Please verify that you are not a robot."
                );

                return false;
            }

            string secretKey =
                ConfigurationManager
                    .AppSettings["RecaptchaSecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                ShowError(
                    "ⓘ reCAPTCHA is not configured."
                );

                return false;
            }

            try
            {
                using (
                    WebClient client =
                        new WebClient())
                {
                    NameValueCollection values =
                        new NameValueCollection
                        {
                            ["secret"] = secretKey,
                            ["response"] = captchaResponse,
                            ["remoteip"] =
                                Request.UserHostAddress
                        };

                    byte[] response =
                        client.UploadValues(
                            "https://www.google.com/" +
                            "recaptcha/api/siteverify",
                            values
                        );

                    string json =
                        System.Text.Encoding.UTF8
                            .GetString(response);

                    JavaScriptSerializer serializer =
                        new JavaScriptSerializer();

                    dynamic result =
                        serializer.Deserialize<dynamic>(
                            json
                        );

                    bool success =
                        result != null &&
                        result.ContainsKey("success") &&
                        (bool)result["success"];

                    if (!success)
                    {
                        ShowError(
                            "ⓘ Captcha verification failed."
                        );
                    }

                    return success;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "reCAPTCHA error: " + ex.Message
                );

                ShowError(
                    "ⓘ Captcha verification failed. " +
                    "Please try again."
                );

                return false;
            }
        }

        private bool HasValidPendingGoogleProfile()
        {
            if (Session["PendingGoogleSubject"] == null ||
                Session["PendingGoogleEmail"] == null ||
                Session["PendingGoogleExpiresAt"] == null)
            {
                return false;
            }

            DateTime expiresAt;

            if (Session["PendingGoogleExpiresAt"] is DateTime)
            {
                expiresAt =
                    (DateTime)Session["PendingGoogleExpiresAt"];
            }
            else if (!DateTime.TryParse(
                Session["PendingGoogleExpiresAt"].ToString(),
                out expiresAt))
            {
                return false;
            }

            return DateTime.UtcNow <=
                   expiresAt.ToUniversalTime();
        }

        private void ClearPendingGoogleProfile()
        {
            Session.Remove("PendingGoogleSubject");
            Session.Remove("PendingGoogleEmail");
            Session.Remove("PendingGoogleFirstName");
            Session.Remove("PendingGoogleLastName");
            Session.Remove("PendingGoogleExpiresAt");
        }

        private void ShowError(string message)
        {
            lblMessage.Text =
                Server.HtmlEncode(message);

            lblMessage.Visible = true;
        }

        private bool IsStrongPassword(
            string password)
        {
            return password.Length >= 8 &&
                   Regex.IsMatch(password, @"[A-Z]") &&
                   Regex.IsMatch(password, @"[0-9]") &&
                   Regex.IsMatch(password, @"[\W]");
        }

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod]
        public static bool UsernameExists(
            string username)
        {
            string cs =
                ConfigurationManager
                    .ConnectionStrings["KeyCodeDB"]
                    .ConnectionString;

            using (
                SqlConnection con =
                    new SqlConnection(cs))
            using (
                SqlCommand command =
                    new SqlCommand(@"
                        SELECT COUNT(*)
                        FROM dbo.Users
                        WHERE
                            LOWER(LTRIM(RTRIM(username)))
                                = @username",
                        con))
            {
                command.Parameters.Add(
                    "@username",
                    SqlDbType.NVarChar,
                    50
                ).Value =
                    (username ?? "")
                        .Trim()
                        .ToLowerInvariant();

                con.Open();

                return Convert.ToInt32(
                    command.ExecuteScalar()
                ) > 0;
            }
        }
    }
}