<%@ Page Title="Password Login Retired" Language="C#" AutoEventWireup="true"
    CodeBehind="ForgotPassword.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.ForgotPassword" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>KEYCODE - Google Sign-In</title>
    <link href="<%= ResolveUrl("~/Asm_StyleSheet/Login.css") %>" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
    <div class="login-wrapper">
        <div class="login-card">
            <header><img class="auto-style1" src="../logo.png" alt="KEYCODE" /></header>
            <h2 class="reset-title">Password Login Retired</h2>
            <p class="instruction-text">KEYCODE now uses Google Sign-In instead of site passwords.</p>
            <div class="login-footer">
                <asp:HyperLink ID="lnkLogin" runat="server" NavigateUrl="ResetPassword.aspx">
                    Continue to Google Sign-In
                </asp:HyperLink>
            </div>
        </div>
    </div>
</form>
</body>
</html>
