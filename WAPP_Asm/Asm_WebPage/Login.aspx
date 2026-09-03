<%@ Page Title="Login" Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.Login" %>

<!DOCTYPE html>
<html>

<head runat="server">
    <script src="https://www.google.com/recaptcha/api.js" async defer></script>
    <title>KEYCODE - Login</title>
    <link href="<%= ResolveUrl("~/Asm_StyleSheet/Login.css") %>" rel="stylesheet" type="text/css" />
</head>

<body>
    <form id="form1" runat="server">
        <div class="login-wrapper">
            <div class="login-card">

                <header>
                    <img class="auto-style1" src="../logo.png" />
                </header>

                <div class="form-group">
                    <label>Username</label>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="input-field"></asp:TextBox>
                    <div class="validator-container">
                        <asp:RequiredFieldValidator ID="rfvUsername" runat="server"
                            ControlToValidate="txtUsername"
                            ErrorMessage="ⓘ Username is required."
                            CssClass="validator-text"
                            Display="Dynamic" />
                    </div>
                </div>

                <div class="form-group">
                    <label>Password</label>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="input-field"></asp:TextBox>
                    
                    <div class="login-hint">
                    ⓘ New tutors: Default password is your <strong><em>username in lowercase</em></strong>. 
                    You will be required to change it after logging in.
                    </div>

                    <div class="validator-container">
                       
                        <asp:RequiredFieldValidator ID="rfvPassword" runat="server"
                            ControlToValidate="txtPassword"
                            ErrorMessage="ⓘ Password is required."
                            CssClass="validator-text"
                            Display="Dynamic" />
                      
                        <span class="login-error-message">
                            <asp:Literal ID="lblMessage" runat="server" Visible="false"></asp:Literal>
                            <asp:LinkButton ID="btnReactivate" runat="server" 
                                Text=" Click here to request reactivation."
                                Visible="false"
                                OnClick="btnReactivate_Click"
                                CausesValidation="false" />
                        </span>

                    </div>
                </div>

                <div class="form-group">
                    <div class="recaptcha-wrapper">
                        <div class="g-recaptcha"
                             data-sitekey="6LegAWwsAAAAACMX8xzbmSptxbl1piKrqICGBRwr">
                        </div>
                    </div>
                </div>


                <div class="button-group">
                    <asp:Button ID="btnLogin" runat="server"
                        Text="Login"
                        CssClass="btn btn-primary"
                        OnClick="BtnLogin_Click" />

                    <asp:Button ID="btnGuest" runat="server"
                        Text="Continue as Guest"
                        CssClass="btn btn-primary"
                        OnClick="BtnGuest_Click"
                        CausesValidation="false" />
                </div>

                <div class="login-footer">
                    <div class="footer-line">
                        Not yet Registered?
                        <asp:HyperLink ID="lnkSignUp" runat="server"
                            NavigateUrl="SignUp.aspx">Sign Up</asp:HyperLink>
                    </div>

                    <div class="footer-line">
                        Forgot Password?
                        <asp:HyperLink ID="lnkForget" runat="server"
                            NavigateUrl="ForgotPassword.aspx">Click Here</asp:HyperLink>
                    </div>
                </div>

            </div>
        </div>
    </form>
</body>
</html>