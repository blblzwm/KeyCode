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
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void btnGuest_Click(object sender, EventArgs e)
        {
            Session["UserID"] = null;
            Session["username"] = "Visitor";
            Session["role"] = "guest";
            Session["upload_profile"] = "";

            Response.Redirect("~/Asm_WebPage/StudentDashboard.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}