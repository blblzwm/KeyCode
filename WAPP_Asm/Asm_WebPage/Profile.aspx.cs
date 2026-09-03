using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using Isopoh.Cryptography.Argon2;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class Profile : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                string profileId = !string.IsNullOrEmpty(Request.QueryString["id"])
                    ? Request.QueryString["id"]
                    : Session["UserID"].ToString();

                ViewState["ProfileUserID"] = profileId;
                FetchUserProfile(profileId);

                bool isOwnProfile = profileId == Session["UserID"].ToString();
                btnShowPassword.Visible = isOwnProfile;
            }
        }

        private void FetchUserProfile(string userId)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT * FROM Users WHERE userID=@id", conn);
                cmd.Parameters.AddWithValue("@id", userId);

                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    lblUserID.Text = reader["userID"].ToString();
                    lblUsername.Text = reader["username"].ToString();
                    lblFname.Text = reader["fname"].ToString();
                    lblLname.Text = reader["lname"].ToString();
                    lblEmail.Text = reader["email"].ToString();
                    lblDOB.Text = Convert.ToDateTime(reader["dob"])
                                  .ToString("dd MMMM yyyy");

                    string avatar = reader["upload_profile"]?.ToString();
                    string username = reader["username"].ToString();

                    if (string.IsNullOrEmpty(avatar))
                    {
                        string initial = username.Substring(0, 1).ToUpper();
                        string brandBlue = "1d395e";
                        Session["upload_profile"] = "";
                        imgAvatar.ImageUrl =
                            $"https://ui-avatars.com/api/?name={initial}&background={brandBlue}&color=ffffff&size=160&bold=true&rounded=true";
                    }
                    else
                    {
                        imgAvatar.ImageUrl = ResolveUrl(avatar);
                        Session["upload_profile"] = avatar;
                    }

                    string role = reader["role"].ToString();
                    ViewState["ProfileRole"] = role;

                    pnlProfessional.Visible =
                        role.Equals("Tutor", StringComparison.OrdinalIgnoreCase);

                    string qual = reader["qualification"]?.ToString();
                    lblQualification.Text = qual;

                    if (!string.IsNullOrEmpty(qual) && ddlQualification.Items.FindByValue(qual) != null)
                    {
                        ddlQualification.SelectedValue = qual;
                    }
                }
            }
        }

        protected void btnUploadAvatar_Click(object sender, EventArgs e)
        {
            lblAvatarMsg.Text = "";
            lblAvatarMsg.Visible = false;

            if (!fuAvatar.HasFile)
            {
                lblAvatarMsg.Text = "ⓘ Please select an image.";
                lblAvatarMsg.CssClass = "validator-text";
                lblAvatarMsg.Visible = true;
                return;
            }

            if (fuAvatar.PostedFile.ContentLength > 2_000_000)
            {
                lblAvatarMsg.Text = "ⓘ Image must be under 2MB.";
                lblAvatarMsg.CssClass = "validator-text";
                lblAvatarMsg.Visible = true;
                return;
            }

            string ext = Path.GetExtension(fuAvatar.FileName).ToLower();

            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            {
                lblAvatarMsg.Text = "ⓘ Only PNG or JPG images allowed.";
                lblAvatarMsg.CssClass = "validator-text";
                lblAvatarMsg.Visible = true;
                return;
            }

            if (!fuAvatar.PostedFile.ContentType.StartsWith("image/"))
            {
                lblAvatarMsg.Text = "ⓘ Invalid image file.";
                lblAvatarMsg.CssClass = "validator-text";
                lblAvatarMsg.Visible = true;
                return;
            }

            string fileName = Guid.NewGuid() + ext;
            string path = "~/Uploads/UserProfile/" + fileName;
            fuAvatar.SaveAs(Server.MapPath(path));

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Users SET upload_profile=@img WHERE userID=@id", conn);
                cmd.Parameters.AddWithValue("@img", path);
                cmd.Parameters.AddWithValue("@id", ViewState["ProfileUserID"]);
                cmd.ExecuteNonQuery();
            }

            imgAvatar.ImageUrl = ResolveUrl(path);
            Session["upload_profile"] = path;

            Response.Redirect(Request.RawUrl);
        }

        protected void btnRemoveAvatar_Click(object sender, EventArgs e)
        {
            string userId = ViewState["ProfileUserID"].ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                SqlCommand getCmd = new SqlCommand(
                    "SELECT upload_profile FROM Users WHERE userID=@id", conn);
                getCmd.Parameters.AddWithValue("@id", userId);

                string avatarPath = getCmd.ExecuteScalar() as string;

                if (!string.IsNullOrEmpty(avatarPath))
                {
                    string fullPath = Server.MapPath(avatarPath);
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }

                SqlCommand updateCmd = new SqlCommand(
                    "UPDATE Users SET upload_profile=NULL WHERE userID=@id", conn);
                updateCmd.Parameters.AddWithValue("@id", userId);
                updateCmd.ExecuteNonQuery();
            }

            Session["upload_profile"] = "";
            Response.Redirect(Request.RawUrl);
        }

        protected void btnShowPassword_Click(object sender, EventArgs e)
        {
            pnlPassword.Visible = !pnlPassword.Visible;
            lblPasswordMsg.Text = "";
            lblPasswordMsg.CssClass = "password-msg";
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            if (Session["PasswordChanged"] != null)
            {
                lblPasswordMsg.Text = "ⓘ You can only change your password once per session.";
                lblPasswordMsg.CssClass = "password-msg error";
                return;
            }

            string current = txtCurrentPassword.Text.Trim();
            string newPass = txtNewPassword.Text.Trim();
            string confirm = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(current) ||
                string.IsNullOrWhiteSpace(newPass) ||
                string.IsNullOrWhiteSpace(confirm))
            {
                lblPasswordMsg.Text = "ⓘ All fields are required.";
                lblPasswordMsg.CssClass = "password-msg error";
                return;
            }

            if (newPass != confirm)
            {
                lblPasswordMsg.Text = "ⓘ New passwords do not match.";
                lblPasswordMsg.CssClass = "password-msg error";
                return;
            }

            if (newPass.Length < 8)
            {
                lblPasswordMsg.Text = "ⓘ Password must be at least 8 characters.";
                lblPasswordMsg.CssClass = "password-msg error";
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(newPass, @"[A-Z]") ||
                !System.Text.RegularExpressions.Regex.IsMatch(newPass, @"[0-9]") ||
                !System.Text.RegularExpressions.Regex.IsMatch(newPass, @"[\W]"))
            {
                lblPasswordMsg.Text = "ⓘ Must contain uppercase, number and symbol.";
                lblPasswordMsg.CssClass = "password-msg error";
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();

                    SqlCommand getCmd = new SqlCommand(
                        "SELECT pwd_hash FROM Users WHERE userID=@id", conn);
                    getCmd.Parameters.AddWithValue("@id", Session["UserID"]);

                    string storedHash = getCmd.ExecuteScalar() as string;

                    if (string.IsNullOrEmpty(storedHash))
                    {
                        lblPasswordMsg.Text = "Password record not found.";
                        lblPasswordMsg.CssClass = "password-msg error";
                        return;
                    }

                    if (!Argon2.Verify(storedHash, current))
                    {
                        lblPasswordMsg.Text = "ⓘ Current password incorrect.";
                        lblPasswordMsg.CssClass = "password-msg error";
                        return;
                    }

                    if (Argon2.Verify(storedHash, newPass))
                    {
                        lblPasswordMsg.Text = "ⓘ New password cannot be same as old.";
                        lblPasswordMsg.CssClass = "password-msg error";
                        return;
                    }

                    string newHash = Argon2.Hash(newPass);

                    SqlCommand update = new SqlCommand(
                        "UPDATE Users SET pwd_hash=@p WHERE userID=@id", conn);
                    update.Parameters.AddWithValue("@p", newHash);
                    update.Parameters.AddWithValue("@id", Session["UserID"]);
                    update.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                lblPasswordMsg.Text = "Something went wrong. Try again.";
                lblPasswordMsg.CssClass = "password-msg error";
                return;
            }

            lblPasswordMsg.Text = "Password updated successfully!";
            lblPasswordMsg.CssClass = "password-msg success";
            Session["PasswordChanged"] = true;
            pnlPassword.Visible = false;
            txtCurrentPassword.Text = "";
            txtNewPassword.Text = "";
            txtConfirmPassword.Text = "";
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            lblProfileMsg.Visible = false;
            ToggleEdit(true);

            string sessionRole = Session["role"]?.ToString().ToLower();
            string profileRole = ViewState["ProfileRole"]?.ToString().ToLower();

            if (sessionRole == "admin" && profileRole == "tutor")
            {
                btnSave.OnClientClick = "return confirm('Are you sure you want to save changes to this tutor?');";
            }
            else
            {
                btnSave.OnClientClick = "return confirm('Are you sure you want to save these changes?');";
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            if (pnlEditActions.Visible)
            {
                ToggleEdit(false);
                FetchUserProfile(ViewState["ProfileUserID"].ToString());
                return;
            }

            string returnUrl = Request.QueryString["returnUrl"];

            if (!string.IsNullOrEmpty(returnUrl))
            {
                Response.Redirect(returnUrl);
                return;
            }

            if (Session["role"] != null)
            {
                string role = Session["role"].ToString();

                if (role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                    Response.Redirect("StudentDashboard.aspx");
                else if (role.Equals("Tutor", StringComparison.OrdinalIgnoreCase))
                    Response.Redirect("TutorDashboard.aspx");
                else
                    Response.Redirect("AdminDashboard.aspx");
            }
            else
            {
                Response.Redirect("Home.aspx");
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            lblDOBMsg.Visible = false;
            lblUsernameMsg.Visible = false;
            lblEmailMsg.Visible = false;
            lblProfileMsg.Visible = false;

            ToggleEdit(false);

            if (ViewState["ProfileUserID"] != null)
                FetchUserProfile(ViewState["ProfileUserID"].ToString());
        }

        private void ToggleEdit(bool edit)
        {
            if (!edit)
            {
                lblDOBMsg.Visible = false;
                lblUsernameMsg.Visible = false;
                lblEmailMsg.Visible = false;
                lblProfileMsg.Visible = false;
            }

            txtUsername.Visible = edit;
            lblUsername.Visible = !edit;

            if (edit)
                txtUsername.Text = lblUsername.Text;

            txtFname.Visible = edit;
            txtLname.Visible = edit;
            lblFname.Visible = !edit;
            lblLname.Visible = !edit;

            btnSave.Visible = edit;
            btnCancel.Visible = edit;

            txtEmail.Visible = edit;
            lblEmail.Visible = !edit;

            ddlQualification.Visible = edit && pnlProfessional.Visible;
            lblQualification.Visible = !edit;

            pnlAvatarUpload.Visible = edit;

            pnlEditActions.Visible = edit;
            btnEdit.Style["visibility"] = edit ? "hidden" : "visible";

            txtDOB.Visible = edit;
            lblDOB.Visible = !edit;

            reqUsername.Enabled = edit;
            revUsername.Enabled = edit;
            reqFname.Enabled = edit;
            revFname.Enabled = edit;
            reqLname.Enabled = edit;
            revLname.Enabled = edit;
            reqEmail.Enabled = edit;
            revEmail.Enabled = edit;

            reqQualification.Enabled = edit && pnlProfessional.Visible;

            if (edit)
            {
                txtFname.Text = lblFname.Text;
                txtLname.Text = lblLname.Text;
                txtEmail.Text = lblEmail.Text;

                if (ddlQualification.Items.FindByValue(lblQualification.Text) != null)
                    ddlQualification.SelectedValue = lblQualification.Text;

                DateTime dob;
                if (DateTime.TryParse(lblDOB.Text, out dob))
                    txtDOB.Text = dob.ToString("yyyy-MM-dd");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            lblUsernameMsg.Visible = false;
            lblUsernameMsg.Text = "";
            lblEmailMsg.Visible = false;
            lblProfileMsg.Visible = false;
            lblDOBMsg.Visible = false;

            if (fuAvatar.HasFile)
            {
                lblAvatarMsg.Text = "ⓘ Please click the Upload button to save the avatar.";
                lblAvatarMsg.CssClass = "validator-text";
                lblAvatarMsg.Visible = true;
                return;
            }

            DateTime dob;

            if (!DateTime.TryParse(txtDOB.Text, out dob))
            {
                lblDOBMsg.Text = "ⓘ Invalid date of birth.";
                lblDOBMsg.Visible = true;
                return;
            }

            int age = DateTime.Today.Year - dob.Year;
            if (dob > DateTime.Today.AddYears(-age)) age--;

            string role = ViewState["ProfileRole"]?.ToString() ?? "";
            bool isTutor = role.Equals("Tutor", StringComparison.OrdinalIgnoreCase);

            if (isTutor && (age < 18 || age > 100))
            {
                lblDOBMsg.Text = "ⓘ Tutor age must be between 18 and 100.";
                lblDOBMsg.Visible = true;
                return;
            }

            if (!Page.IsValid)
                return;

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                SqlCommand checkEmail = new SqlCommand(
                    "SELECT COUNT(*) FROM Users WHERE email=@e AND userID<>@id", conn);
                checkEmail.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
                checkEmail.Parameters.AddWithValue("@id", ViewState["ProfileUserID"]);

                int emailExists = (int)checkEmail.ExecuteScalar();

                if (emailExists > 0)
                {
                    lblEmailMsg.Text = "ⓘ Email already exists.";
                    lblEmailMsg.Visible = true;
                    return;
                }

                string qualification = isTutor ? ddlQualification.SelectedValue : null;

                if (isTutor && string.IsNullOrWhiteSpace(ddlQualification.SelectedValue))
                {
                    lblProfileMsg.Text = "ⓘ Qualification is required for tutors.";
                    lblProfileMsg.CssClass = "validation-error";
                    return;
                }

                string username = txtUsername.Text.Trim();

                if (!System.Text.RegularExpressions.Regex.IsMatch(
                    username,
                    @"^(?=(?:.*[A-Za-z]){3,})(?!.*[_-]{2})[A-Za-z0-9][A-Za-z0-9_-]{1,18}[A-Za-z0-9]$"))
                {
                    lblUsernameMsg.Text = "ⓘ Invalid username format.";
                    lblUsernameMsg.CssClass = "validation-error";
                    return;
                }

                SqlCommand checkUsername = new SqlCommand(
                    "SELECT COUNT(*) FROM Users WHERE LOWER(username)=LOWER(@u) AND userID<>@id", conn);
                checkUsername.Parameters.AddWithValue("@u", txtUsername.Text.Trim());
                checkUsername.Parameters.AddWithValue("@id", ViewState["ProfileUserID"]);

                int usernameExists = (int)checkUsername.ExecuteScalar();

                if (usernameExists > 0)
                {
                    lblUsernameMsg.Text = "ⓘ Username already taken.";
                    lblUsernameMsg.Visible = true;
                    return;
                }

                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Users SET
                        username = @u,
                        fname = @f,
                        lname = @l,
                        email = @e,
                        dob = @dob,
                        qualification = @q
                    WHERE userID = @id", conn);

                cmd.Parameters.AddWithValue("@u", txtUsername.Text.Trim());
                cmd.Parameters.AddWithValue("@f", txtFname.Text.Trim());
                cmd.Parameters.AddWithValue("@l", txtLname.Text.Trim());
                cmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@dob", dob);
                cmd.Parameters.AddWithValue("@q", (object)qualification ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", ViewState["ProfileUserID"]);

                cmd.ExecuteNonQuery();
            }

            ToggleEdit(false);
            FetchUserProfile(ViewState["ProfileUserID"].ToString());
        }
    }
}