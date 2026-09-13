using Google.Apis.Auth;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class GoogleAuth : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Request.HttpMethod.Equals(
                "POST",
                StringComparison.OrdinalIgnoreCase))
            {
                Fail(
                    "ⓘ GoogleAuth.aspx received " +
                    Request.HttpMethod +
                    " instead of POST."
                );

                return;
            }

            string cookieCsrf =
                Request.Cookies["g_csrf_token"]?.Value;

            string bodyCsrf =
                Request.Form["g_csrf_token"];

            if (string.IsNullOrWhiteSpace(cookieCsrf) ||
                string.IsNullOrWhiteSpace(bodyCsrf) ||
                !string.Equals(
                    cookieCsrf,
                    bodyCsrf,
                    StringComparison.Ordinal))
            {
                Fail("ⓘ Invalid Google sign-in request.");
                return;
            }

            string credential = Request.Form["credential"];

            string clientId =
                (ConfigurationManager
                    .AppSettings["GoogleClientId"] ?? "")
                .Trim();

            if (string.IsNullOrWhiteSpace(credential) ||
                string.IsNullOrWhiteSpace(clientId))
            {
                Fail("ⓘ Google Sign-In is not configured correctly.");
                return;
            }

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload =
                    GoogleJsonWebSignature.ValidateAsync(
                        credential,
                        new GoogleJsonWebSignature.ValidationSettings
                        {
                            Audience = new[] { clientId }
                        })
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Google validation failed: " + ex.Message);

                Fail("ⓘ Google authentication failed.");
                return;
            }

            if (payload == null ||
                string.IsNullOrWhiteSpace(payload.Subject) ||
                string.IsNullOrWhiteSpace(payload.Email) ||
                !payload.EmailVerified)
            {
                Fail("ⓘ Google did not provide a verified email.");
                return;
            }

            string subject = payload.Subject.Trim();

            string email = payload.Email
                .Trim()
                .ToLowerInvariant();

            string cs = ConfigurationManager
                .ConnectionStrings["KeyCodeDB"]
                .ConnectionString;

            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();

                    string userId = null;
                    string username = null;
                    string role = null;
                    string status = null;
                    string profile = "";
                    string storedSubject = null;

                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT TOP (1)
                            userID,
                            username,
                            role,
                            status,
                            upload_profile,
                            google_subject
                        FROM dbo.Users
                        WHERE google_subject = @subject
                           OR LOWER(LTRIM(RTRIM(email))) = @email
                        ORDER BY
                            CASE
                                WHEN google_subject = @subject
                                THEN 0
                                ELSE 1
                            END", con))
                    {
                        cmd.Parameters.Add(
                            "@subject",
                            SqlDbType.NVarChar,
                            255).Value = subject;

                        cmd.Parameters.Add(
                            "@email",
                            SqlDbType.NVarChar,
                            256).Value = email;

                        using (SqlDataReader dr =
                               cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                userId = dr["userID"]
                                    .ToString()
                                    .Trim();

                                username = dr["username"]
                                    .ToString()
                                    .Trim();

                                role = dr["role"]
                                    .ToString()
                                    .Trim()
                                    .ToLowerInvariant();

                                status = dr["status"]
                                    .ToString()
                                    .Trim();

                                profile =
                                    dr["upload_profile"] == DBNull.Value
                                    ? ""
                                    : dr["upload_profile"].ToString();

                                storedSubject =
                                    dr["google_subject"] == DBNull.Value
                                    ? null
                                    : dr["google_subject"]
                                        .ToString()
                                        .Trim();
                            }
                        }
                    }

                    // No account exists. Only a Student may self-register.
                    if (userId == null)
                    {
                        Session.Clear();

                        if (userId == null)
                        {
                            ClearPendingGoogleProfile();

                            Session["PendingGoogleSubject"] =
                                subject;

                            Session["PendingGoogleEmail"] =
                                email;

                            Session["PendingGoogleFirstName"] =
                                payload.GivenName ?? "";

                            Session["PendingGoogleLastName"] =
                                payload.FamilyName ?? "";

                            Session["PendingGoogleExpiresAt"] =
                                DateTime.UtcNow.AddMinutes(10);

                            Response.Redirect(
                                ResolveUrl("~/Asm_WebPage/SignUp.aspx"),
                                false
                            );

                            Context.ApplicationInstance
                                .CompleteRequest();

                            return;
                        }

                        Response.Redirect(
                            "SignUp.aspx",
                            false);

                        Context.ApplicationInstance
                            .CompleteRequest();

                        return;
                    }

                    if (status.Equals(
                            "Deleted",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        Fail("ⓘ This account is unavailable.");
                        return;
                    }

                    // Link a legacy account on its first Google login.
                    if (string.IsNullOrWhiteSpace(storedSubject))
                    {
                        using (SqlCommand link =
                               new SqlCommand(@"
                            UPDATE dbo.Users
                            SET google_subject = @subject
                            WHERE userID = @id
                              AND google_subject IS NULL", con))
                        {
                            link.Parameters.Add(
                                "@subject",
                                SqlDbType.NVarChar,
                                255).Value = subject;

                            link.Parameters.Add(
                                "@id",
                                SqlDbType.NVarChar,
                                50).Value = userId;

                            if (link.ExecuteNonQuery() != 1)
                            {
                                Fail(
                                    "ⓘ Account linking failed. " +
                                    "Please contact the administrator.");

                                return;
                            }
                        }
                    }
                    else if (!string.Equals(
                                 storedSubject,
                                 subject,
                                 StringComparison.Ordinal))
                    {
                        Fail(
                            "ⓘ This email is already linked " +
                            "to another Google Account.");

                        return;
                    }

                    Session.Clear();

                    Session["UserID"] = userId;
                    Session["username"] = username;
                    Session["role"] = role;
                    Session["status"] = status;
                    Session["upload_profile"] = profile;

                    if (status.Equals(
                            "Suspended",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        Fail("ⓘ Your account has been suspended.");
                        return;
                    }

                    RedirectToDashboard(role);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Google login database error: " + ex.Message);

                Fail(
                    "ⓘ Google sign-in is temporarily unavailable.");
            }
        }

        private void RedirectToDashboard(string role)
        {
            string target;

            switch ((role ?? "")
                    .Trim()
                    .ToLowerInvariant())
            {
                case "tutor":
                    target = ResolveUrl(
                        "~/Asm_WebPage/TutorDashboard.aspx");
                    break;

                case "admin":
                    target = ResolveUrl(
                        "~/Asm_WebPage/AdminDashboard.aspx");
                    break;

                default:
                    target = ResolveUrl(
                        "~/Asm_WebPage/StudentDashboard.aspx");
                    break;
            }

            Response.Redirect(target, false);

            Context.ApplicationInstance
                .CompleteRequest();
        }

        private void Fail(string message)
        {
            Session["GoogleAuthError"] = message;

            string target =
                ResolveUrl("~/Asm_WebPage/Login.aspx") +
                "?googleError=" +
                Server.UrlEncode(message);

            Response.Redirect(target, false);

            Context.ApplicationInstance
                .CompleteRequest();
        }

        private void RedirectToLogin()
        {
            Response.Redirect("Login.aspx", false);

            Context.ApplicationInstance
                .CompleteRequest();
        }

        private void ClearPendingGoogleProfile()
        {
            Session.Remove("PendingGoogleSubject");
            Session.Remove("PendingGoogleEmail");
            Session.Remove("PendingGoogleFirstName");
            Session.Remove("PendingGoogleLastName");
            Session.Remove("PendingGoogleExpiresAt");
        }
    }
}