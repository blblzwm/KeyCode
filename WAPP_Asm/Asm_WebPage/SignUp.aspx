<%@ Page Title="Sign Up" Language="C#" AutoEventWireup="true"
    CodeBehind="SignUp.aspx.cs"
    Inherits="WAPP_Asm.Asm_WebPage.SignUp" %>

<!DOCTYPE html>
<html>
<head runat="server">

    <script src="https://www.google.com/recaptcha/api.js"
        async defer></script>

    <title>KEYCODE - Sign Up</title>

    <link href="<%= ResolveUrl("~/Asm_StyleSheet/Login.css") %>"
        rel="stylesheet" />

</head>

<body>
<form id="form1" runat="server">

<div class="login-wrapper">
<div class="login-card">

    <header>
        <img class="auto-style1"
            src="../logo.png"
            alt="KEYCODE" />
    </header>

    <div class="form-group">
        <label>Username</label>

        <asp:TextBox
            ID="txtUsername"
            runat="server"
            CssClass="input-field"
            MaxLength="20"
            autocomplete="username" />

        <span class="security-hint">
            Suggested from email. You can change it.
        </span>
    </div>


    <div class="form-group">
        <label>First Name</label>

        <asp:TextBox
            ID="txtFname"
            runat="server"
            CssClass="input-field"
            MaxLength="50" />
    </div>

    <div class="form-group">
        <label>Last Name</label>

        <asp:TextBox
            ID="txtLname"
            runat="server"
            CssClass="input-field"
            MaxLength="50" />
    </div>

    <div class="form-group">
        <label>Email Address</label>

        <asp:TextBox
            ID="txtEmail"
            runat="server"
            TextMode="Email"
            CssClass="input-field"
            MaxLength="256"
            ReadOnly="true" />

    </div>

    <div class="form-group">
        <label>Date of Birth</label>

        <asp:TextBox
            ID="txtDOB"
            runat="server"
            TextMode="Date"
            CssClass="input-field" />
    </div>

    <div class="form-group">
        <div class="recaptcha-wrapper">

            <div class="g-recaptcha"
                 data-sitekey="6LegAWwsAAAAACMX8xzbmSptxbl1piKrqICGBRwr">
            </div>

        </div>
    </div>

    <asp:Label
        ID="lblMessage"
        runat="server"
        CssClass="login-error-message"
        Visible="false" />

    <div class="button-group">

        <asp:Button
            ID="btnRegister"
            runat="server"
            Text="Register"
            CssClass="btn btn-primary"
            OnClick="btnRegister_Click"
            ValidationGroup="RegisterGroup" />

    </div>

    <div class="login-footer">
        Already have an account?

        <asp:HyperLink
            ID="lnkLogin"
            runat="server"
            NavigateUrl="Login.aspx">
            Login
        </asp:HyperLink>
    </div>

</div>
</div>

</form>

</body>
</html>