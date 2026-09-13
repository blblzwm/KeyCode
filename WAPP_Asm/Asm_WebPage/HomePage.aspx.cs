using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class HomePage : System.Web.UI.Page
    {
        protected bool IsGuestForHome = true;

        protected void Page_Load(object sender, EventArgs e)
        {
            string roleRaw = (Session["role"] ?? "nonreg_student").ToString().Trim().ToLower();
            string userId = (Session["UserID"] ?? "").ToString().Trim();

            IsGuestForHome = roleRaw == "nonreg_student"
                              || roleRaw == "guest"
                              || string.IsNullOrEmpty(userId);
        }
    }
}