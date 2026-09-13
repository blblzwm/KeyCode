using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace KEYCODE
{
    public partial class Site : System.Web.UI.MasterPage
    {
        protected bool IsAdminNavigation
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Convert.ToString(Session["UserID"]))
                && string.Equals(Convert.ToString(Session["role"]).Trim(), "admin", StringComparison.OrdinalIgnoreCase);
            }
        }
        protected int? PendingAppealCount { get; private set; }
        protected string AdminNavClass(string fileName)
        {
            return string.Equals(System.IO.Path.GetFileName(Request.Path), fileName,
                StringComparison.OrdinalIgnoreCase) ? "active" : "";
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            // After page actions, so an approval postback can update the badge.
            if (!IsAdminNavigation) return;
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["KeyCodeDB"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM ReactivationRequests WHERE status = 'Pending'", con))
                {
                    con.Open();
                    PendingAppealCount = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (SqlException ex)
            {
                PendingAppealCount = null;
                System.Diagnostics.Trace.TraceError("Admin pending count: {0}", ex);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string roleRaw = (Session["role"] ?? "nonreg_student").ToString().Trim().ToLower();
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

                // This panel now holds only the logout icon beside the profile.
                if (pnlRoleRow != null) pnlRoleRow.Visible = !isGuest;
                string displayName = isGuest ? "Guest" : LoadFullName(userId);

                // Forum is still member-only; the profile/avatar button is now shown to
                // everyone, but which avatar variant renders differs (see below).
                if (pnlForumIcon != null) pnlForumIcon.Visible = !isGuest;

                // Guests get a generic grey silhouette; logged-in users get their
                // initial letter or uploaded photo. Same split applies to both the
                // sidebar avatar AND the popup profile menu's avatar.
                if (pnlGuestAvatar != null) pnlGuestAvatar.Visible = isGuest;
                if (pnlUserAvatar != null) pnlUserAvatar.Visible = !isGuest;
                if (pnlGuestAvatarMenu != null) pnlGuestAvatarMenu.Visible = isGuest;
                if (pnlUserAvatarMenu != null) pnlUserAvatarMenu.Visible = !isGuest;

                // Initial (even guest can have "V")
                string initial = "U";
                if (!string.IsNullOrWhiteSpace(displayName))
                    initial = displayName.Substring(0, 1).ToUpperInvariant();

                // Topbar/sidebar avatar letter (only visible when the user-avatar panel is shown)
                lblInitial.Text = Server.HtmlEncode(initial);
                lblInitialMenu.Text = Server.HtmlEncode(initial);

                // Sidebar profile button label + dropdown info
                lblUsernameSide.Text = Server.HtmlEncode(displayName);
                lblUsernameSide.ToolTip = displayName;
                lblRoleUnderName.Text = "Tap to sign in";
                lblRoleUnderName.Visible = isGuest;
                lblRoleMenu.Text = roleDisplay;
                if (pnlRoleMenuRow != null) pnlRoleMenuRow.Visible = !isGuest;
                lblUsernameMenu.Text = Server.HtmlEncode(displayName);

                // Guests are encouraged to log in rather than edit a profile they don't have.
                if (pnlEditProfileAction != null) pnlEditProfileAction.Visible = !isGuest;
                if (pnlGuestLoginAction != null) pnlGuestLoginAction.Visible = isGuest;

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

        protected string GetDashboardUrl()
        {
            // Convert to lowercase to ensure case-insensitive matching
            string role = (Session["role"] ?? "").ToString().ToLower();

            switch (role)
            {
                case "admin":
                    return ResolveUrl("~/Asm_WebPage/AdminDashboard.aspx");

                case "tutor":
                    return ResolveUrl("~/Asm_WebPage/TutorDashboard.aspx");

                case "student":
                case "guest":
                case "nonreg_student":
                case "": // Handles cases where the session role is completely null/empty
                    return ResolveUrl("~/Asm_WebPage/StudentDashboard.aspx");

                default:
                    // Fallback for any other unrecognized role
                    return ResolveUrl("~/Asm_WebPage/Login.aspx");
            }
        }



        // Read by the authenticated user ID so existing sessions and all login
        // methods show the same name without changing authentication code.
        private string LoadFullName(string userId)
        {
            string connectionString = ConfigurationManager
                .ConnectionStrings["KeyCodeDB"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(
                "SELECT fname, lname FROM dbo.Users WHERE userID = @UserID", connection))
            {
                command.Parameters.Add("@UserID", SqlDbType.NVarChar, 50).Value = userId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        string firstName = Convert.ToString(reader["fname"]).Trim();
                        string lastName = Convert.ToString(reader["lname"]).Trim();
                        string fullName = (firstName + " " + lastName).Trim();
                        if (!string.IsNullOrWhiteSpace(fullName)) return fullName;
                    }
                }
            }
            return "User";
        }

        public void ShowAlert(string message)
        {
            pnlAlert.Visible = true;
            lblAlert.Text = message;
        }
    }
}
