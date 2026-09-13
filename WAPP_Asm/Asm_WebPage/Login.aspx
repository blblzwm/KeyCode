<%@ Page Title="Login"
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="Login.aspx.cs"
    Inherits="WAPP_Asm.Asm_WebPage.Login" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport"
          content="width=device-width, initial-scale=1" />

    <title>KEYCODE - Login</title>

    <link href="<%= ResolveUrl("~/Asm_StyleSheet/Login.css") %>?v=3"
          rel="stylesheet"
          type="text/css" />

    <!-- Google Identity Services -->
    <script src="https://accounts.google.com/gsi/client"
            async
            defer></script>

    <!-- Google reCAPTCHA -->
    <script src="https://www.google.com/recaptcha/api.js"
        async
        defer></script>
</head>

<body>
<form id="form1" runat="server">

    <div class="login-wrapper">
        <div class="login-card">

            <header>
                <img class="auto-style1"
                     src="<%= ResolveUrl("~/logo.png") %>"
                     alt="KEYCODE" />
            </header>

            <!-- USERNAME -->
            <div class="form-group">

                <asp:Label
                    ID="lblUsername"
                    runat="server"
                    AssociatedControlID="txtUsername"
                    Text="Username" />

                <asp:TextBox
                    ID="txtUsername"
                    runat="server"
                    CssClass="input-field"
                    MaxLength="50"
                    autocomplete="username"
                    placeholder="Enter your username" />

                <asp:RequiredFieldValidator
                    ID="rfvUsername"
                    runat="server"
                    ControlToValidate="txtUsername"
                    ValidationGroup="LocalLogin"
                    ErrorMessage="Username is required."
                    CssClass="validator-text"
                    Display="Dynamic" />

            </div>

            <!-- PASSWORD -->
            <div class="form-group">

                <asp:Label
                    ID="lblPassword"
                    runat="server"
                    AssociatedControlID="txtPassword"
                    Text="Password" />

                <asp:TextBox
                    ID="txtPassword"
                    runat="server"
                    CssClass="input-field"
                    TextMode="Password"
                    autocomplete="current-password"
                    placeholder="Enter your password" />

                <asp:RequiredFieldValidator
                    ID="rfvPassword"
                    runat="server"
                    ControlToValidate="txtPassword"
                    ValidationGroup="LocalLogin"
                    ErrorMessage="Password is required."
                    CssClass="validator-text"
                    Display="Dynamic" />

            </div>

            <!-- FORGOT PASSWORD -->
            <div class="login-footer forgot-password-link">
                <a href="<%= ResolveUrl("~/Asm_WebPage/ResetPassword.aspx") %>">
                    Forgot your password?
                </a>
            </div>

            <!-- RECAPTCHA -->
            <div class="recaptcha-wrapper">
                <div
                    class="g-recaptcha"
                    data-sitekey="<%= Server.HtmlEncode(RecaptchaSiteKey) %>">
                </div>
            </div>

            <!-- LOCAL LOGIN -->
            <div class="button-group local-login-group">

                <asp:Button
                    ID="btnLogin"
                    runat="server"
                    Text="Login"
                    CssClass="btn btn-primary"
                    ValidationGroup="LocalLogin"
                    OnClick="BtnLogin_Click" />

            </div>

            <!-- ERROR MESSAGE -->
            <div class="message-wrapper">

                <asp:Literal
                    ID="lblMessage"
                    runat="server"
                    Visible="false" />

                <asp:LinkButton
                    ID="btnReactivate"
                    runat="server"
                    Text=" Click here to request reactivation."
                    CssClass="reactivate-link"
                    Visible="false"
                    CausesValidation="false"
                    OnClick="btnReactivate_Click" />

            </div>

            <!-- DIVIDER -->
            <hr class="auth-divider" />

            <!-- ALTERNATIVE LOGIN METHODS -->
            <div class="alternative-login-group">

                <!-- GUEST -->
                <asp:Button
                    ID="btnGuest"
                    runat="server"
                    Text="Continue as Guest"
                    CssClass="btn btn-primary"
                    CausesValidation="false"
                    OnClick="BtnGuest_Click" />

                <!-- GOOGLE CONFIGURATION -->
                <div id="g_id_onload"
                     data-client_id="<%= Server.HtmlEncode(GoogleClientId) %>"
                     data-login_uri="<%= Server.HtmlEncode(GoogleAuthUri) %>"
                     data-ux_mode="redirect"
                     data-auto_prompt="false"
                     data-auto_select="false">
                </div>

                <!-- GOOGLE BUTTON -->
                <div class="google-button-wrapper">
                    <div class="g_id_signin"
                         data-type="standard"
                         data-size="large"
                         data-theme="outline"
                         data-text="signin_with"
                         data-shape="pill"
                         data-logo_alignment="left"
                         data-width="352">
                    </div>
                </div>

            </div>

        </div>
    </div>

</form>
</body>
</html>