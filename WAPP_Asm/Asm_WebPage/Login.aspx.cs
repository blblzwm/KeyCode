using System;
using System.Configuration;
using System.Data.SqlClient;
using Isopoh.Cryptography.Argon2;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Specialized;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

            if (!IsPostBack)
            {
                if (Session["UserID"] != null && Session["role"] != null)
                {
                    string sessionStatus = Session["status"]?.ToString() ?? "";
                    if (sessionStatus.Equals("Suspended", StringComparison.OrdinalIgnoreCase))
                        return;

                    string role = Session["role"].ToString().Trim().ToLowerInvariant();

                    string target = null;
                    switch (role)
                    {
                        case "student":
                            target = ResolveUrl("~/Asm_WebPage/StudentDashboard.aspx");
                            break;
                        case "tutor":
                            target = ResolveUrl("~/Asm_WebPage/TutorDashboard.aspx");
                            break;
                        case "admin":
                            target = ResolveUrl("~/Asm_WebPage/AdminDashboard.aspx");
                            break;
                    }

                    if (!string.IsNullOrEmpty(target))
                    {
                        Response.Redirect(target, false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }
                }
            }
        }

        protected void BtnLogin_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            lblMessage.Text = "";

            string captchaResponse = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(captchaResponse))
            {
                lblMessage.Text = "ⓘ Please verify that you are not a robot.";
                lblMessage.Visible = true;
                return;
            }

            string secretKey = ConfigurationManager.AppSettings["RecaptchaSecretKey"];
            try
            {
                using (WebClient client = new WebClient())
                {
                    NameValueCollection values = new NameValueCollection();
                    values["secret"] = secretKey;
                    values["response"] = captchaResponse;

                    byte[] responseBytes = client.UploadValues(
                        "https://www.google.com/recaptcha/api/siteverify", values);

                    string resultJson = System.Text.Encoding.UTF8.GetString(responseBytes);

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    dynamic captchaResult = js.Deserialize<dynamic>(resultJson);

                    if (captchaResult == null || !captchaResult.ContainsKey("success") || !(bool)captchaResult["success"])
                    {
                        lblMessage.Text = "ⓘ Captcha verification failed.";
                        lblMessage.Visible = true;
                        return;
                    }
                }
            }
            catch
            {
                lblMessage.Text = "ⓘ Captcha verification failed. Please try again.";
                lblMessage.Visible = true;
                return;
            }

            string username = (txtUsername.Text ?? "").Trim();
            string password = (txtPassword.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblMessage.Text = "ⓘ You must fill in both username and password.";
                lblMessage.Visible = true;
                return;
            }

            string cs = ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT userID, role, pwd_hash, status, upload_profile FROM dbo.Users WHERE username=@u", con))
            {
                cmd.Parameters.Add("@u", System.Data.SqlDbType.NVarChar, 50).Value = username;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        lblMessage.Text = "ⓘ Invalid username or password!";
                        lblMessage.Visible = true;
                        return;
                    }

                    string status = (dr["status"] ?? "").ToString().Trim();
                    string role = (dr["role"] ?? "").ToString().Trim().ToLowerInvariant();
                    string userId = (dr["userID"] ?? "").ToString().Trim();
                    string storedHash = (dr["pwd_hash"] ?? "").ToString();

                    if (status.Equals("Suspended", StringComparison.OrdinalIgnoreCase))
                    {
                        Session["UserID"] = userId;
                        Session["username"] = username;
                        Session["role"] = role;
                        Session["status"] = "Suspended";
                        Session["upload_profile"] = dr["upload_profile"]?.ToString() ?? "";

                        lblMessage.Text = "ⓘ Your account has been suspended.";
                        lblMessage.Visible = true;
                        btnReactivate.Visible = true;
                        return;
                    }

                    if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
                    {
                        lblMessage.Text = "ⓘ Invalid username or password!";
                        lblMessage.Visible = true;
                        return;
                    }

                    bool ok = false;
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(storedHash) &&
                            storedHash.StartsWith("$argon2", StringComparison.OrdinalIgnoreCase))
                        {
                            ok = Argon2.Verify(storedHash, password);
                        }
                        else
                        {
                            ok = string.Equals(storedHash, password, StringComparison.Ordinal);
                        }
                    }
                    catch
                    {
                        ok = false;
                    }

                    if (!ok)
                    {
                        lblMessage.Text = "ⓘ Invalid username or password!";
                        lblMessage.Visible = true;
                        return;
                    }

                    // Detect tutor first login (default password = username lowercase)
                    if (role == "tutor")
                    {
                        try
                        {
                            if (Argon2.Verify(storedHash, username.ToLower()))
                            {
                                // First login — force password + security Q&A setup
                                Session["ForcePasswordChange"] = true;
                                Session["UserID"] = userId;
                                Session["username"] = username;
                                Session["role"] = "tutor";
                                Session["status"] = "Active";
                                Session["upload_profile"] = dr["upload_profile"]?.ToString() ?? "";

                                Response.Redirect("~/Asm_WebPage/ResetPassword.aspx", false);
                                Context.ApplicationInstance.CompleteRequest();
                                return;
                            }
                        }
                        catch { }
                    }

                    Session["UserID"] = userId;
                    Session["username"] = username;
                    Session["role"] = role;
                    Session["status"] = "Active";
                    Session["upload_profile"] = dr["upload_profile"]?.ToString() ?? "";

                    string target;
                    switch (role)
                    {
                        case "student":
                            target = ResolveUrl("~/Asm_WebPage/StudentDashboard.aspx");
                            break;
                        case "tutor":
                            target = ResolveUrl("~/Asm_WebPage/TutorDashboard.aspx");
                            break;
                        case "admin":
                            target = ResolveUrl("~/Asm_WebPage/AdminDashboard.aspx");
                            break;
                        default:
                            target = ResolveUrl("~/Asm_WebPage/StudentDashboard.aspx");
                            break;
                    }

                    Response.Redirect(target, false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
        }

        protected void btnReactivate_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Asm_WebPage/ReactivationRequest.aspx");
        }


        protected void BtnGuest_Click(object sender, EventArgs e)
        {
            Session["UserID"] = null;
            Session["username"] = "Visitor";
            Session["role"] = "Guest";

            string target = ResolveUrl("~/Asm_WebPage/StudentDashboard.aspx");
            Response.Redirect(target, false);
            Context.ApplicationInstance.CompleteRequest();
        }

    }
}