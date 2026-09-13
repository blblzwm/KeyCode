using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class BattleGame : System.Web.UI.Page
    {
        protected bool IsGuestForPage = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            IsGuestForPage = IsGuest();
        }

        // Ported as-is from StudentDashboard.aspx.cs so guest detection
        // behaves identically across the dashboard and the battle game.
        private bool IsGuest()
        {
            string role = (Session["role"] ?? "nonreg_student").ToString();
            return role.Equals("nonreg_student", StringComparison.OrdinalIgnoreCase)
                || role.Equals("guest", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrEmpty((Session["UserID"] ?? "").ToString());
        }
    }
}