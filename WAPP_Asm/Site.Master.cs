using System;

namespace KEYCODE
{
    public partial class Site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string roleRaw = (Session["role"] ?? "nonreg_student").ToString().Trim().ToLower();
                string username = (Session["username"] ?? "Visitor").ToString().Trim();
                string userId = (Session["UserID"] ?? "").ToString().Trim();
                string uploadProfile = (Session["upload_profile"] ?? "").ToString().Trim();

                bool isGuest = roleRaw == "nonreg_student"
                               || roleRaw == "guest"
                               || string.IsNullOrEmpty(userId);

                // role display text
                string roleDisplay;
                if (isGuest) roleDisplay = "Guest";
                else if (roleRaw == "admin") roleDisplay = "Admin";
                else if (roleRaw == "tutor") roleDisplay = "Tutor";
                else roleDisplay = "Student";

                // Role pill always show
                lblRole.Text = roleDisplay;

                // Hide icons for guest (NEW master: pnlForumIcon + pnlAvatar)
                if (pnlForumIcon != null) pnlForumIcon.Visible = !isGuest;
                if (pnlAvatar != null) pnlAvatar.Visible = !isGuest;

                // Initial (even guest can have "V")
                string initial = "U";
                if (!string.IsNullOrWhiteSpace(username))
                    initial = username.Substring(0, 1).ToUpper();

                // Topbar avatar letter (only visible when pnlAvatar.Visible=true)
                lblInitial.Text = initial;

                // Dropdown info (safe to set even if hidden)
                lblInitialMenu.Text = initial;
                lblRoleMenu.Text = roleDisplay;
                lblUsernameMenu.Text = isGuest ? "Visitor" : username;

                // show profile pic if uploaded, otherwise show initial
                if (!string.IsNullOrEmpty(uploadProfile))
                {
                    imgAvatar.ImageUrl = ResolveUrl(uploadProfile);
                    imgAvatarMenu.ImageUrl = ResolveUrl(uploadProfile);

                    imgAvatar.Visible = true;
                    imgAvatarMenu.Visible = true;

                    lblInitial.Visible = false;
                    lblInitialMenu.Visible = false;
                }
                else
                {
                    imgAvatar.ImageUrl = "";
                    imgAvatarMenu.ImageUrl = "";

                    imgAvatar.Visible = false;
                    imgAvatarMenu.Visible = false;

                    lblInitial.Visible = true;
                    lblInitialMenu.Visible = true;
                }
            }
        }

        public void ShowAlert(string message)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = message;
        }
    }
}
