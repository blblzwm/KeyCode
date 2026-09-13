using System;
using System.Web.UI;

namespace WAPP_Asm.Asm_WebPage
{
    public partial class ForgotPassword : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("~/Asm_WebPage/ResetPassword.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
