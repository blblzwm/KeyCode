<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.ResetPassword" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<title>Reset Password</title>
    <link href="<%= ResolveUrl("~/Asm_StyleSheet/Login.css") %>" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
    <div class="login-wrapper">
        <div class="login-card">

            <header>
                <img src="../logo.png" style="width:265px;height:82px;" alt="KEYCODE" />
            </header>

            <%-- Title changes based on forced vs normal reset --%>
            <h2 class="reset-title">
                <asp:Label ID="lblTitle" runat="server" Text="Reset Password" />
            </h2>

            <p class="instruction-text">
                <asp:Label ID="lblInstruction" runat="server"
                    Text="Enter your new password below." />
            </p>

            <div class="form-group">
                <label>New Password</label>
                <asp:TextBox ID="txtPassword" runat="server"
                    TextMode="Password"
                    CssClass="input-field" />
            </div>

            <div class="form-group">
                <label>Confirm Password</label>
                <asp:TextBox ID="txtConfirm" runat="server"
                    TextMode="Password"
                    CssClass="input-field" />
            </div>

            <%-- Security Q&A panel — only shown on tutor first login --%>
            <asp:Panel ID="pnlSecuritySetup" runat="server" Visible="false">

                <p class="section-label">Set Up Account Recovery</p>
                <p class="instruction-text" style="font-size:0.85rem;">
                    Since this is your first login, please set a security question.
                    You will need it if you ever forget your password.
                </p>

                <div class="form-group">
                    <label>Security Question</label>
                    <asp:DropDownList ID="ddlSecurityQuestion" runat="server" CssClass="input-field">
                        <asp:ListItem Value="">Select a question</asp:ListItem>
                        <asp:ListItem Value="What was the name of your first pet?">What was the name of your first pet?</asp:ListItem>
                        <asp:ListItem Value="What street did you grow up on?">What street did you grow up on?</asp:ListItem>
                        <asp:ListItem Value="What was your childhood nickname?">What was your childhood nickname?</asp:ListItem>
                        <asp:ListItem Value="What is your mother's maiden name?">What is your mother's maiden name?</asp:ListItem>
                        <asp:ListItem Value="What was the name of your primary school?">What was the name of your primary school?</asp:ListItem>
                        <asp:ListItem Value="What city were you born in?">What city were you born in?</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <div class="form-group">
                    <label>Your Answer</label>
                    <asp:TextBox ID="txtSecurityAnswer" runat="server"
                        CssClass="input-field"
                        placeholder="Type your answer here" />
                    <span class="security-hint">⚠ Remember this — you will need it to reset your password.</span>
                </div>

            </asp:Panel>

            <div class="button-group">
                <asp:Button ID="btnReset" runat="server"
                    Text="Reset Password"
                    CssClass="btn btn-primary"
                    OnClick="btnReset_Click" />
            </div>

            <asp:Label ID="lblStatus" runat="server"
                CssClass="login-error-message"
                Visible="false" />

            <%-- Hide "Back to Login" on forced change — tutor must complete setup --%>
            <asp:Panel ID="pnlBackLink" runat="server" CssClass="login-footer">
                <asp:HyperLink ID="lnkLogin" runat="server"
                    NavigateUrl="Login.aspx">
                    Back to Login
                </asp:HyperLink>
            </asp:Panel>

        </div>
    </div>
</form>
</body>
</html>