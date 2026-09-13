<%@ Page Language="C#"
    AutoEventWireup="true"
    CodeBehind="ResetPassword.aspx.cs"
    Inherits="WAPP_Asm.Asm_WebPage.ResetPassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport"
          content="width=device-width, initial-scale=1" />

    <title>KEYCODE - Reset Password</title>

    <link href="<%= ResolveUrl("~/Asm_StyleSheet/Login.css") %>?v=5"
          rel="stylesheet"
          type="text/css" />
<style>.reactivate-link { color:#08619c; font-weight:600; text-decoration:underline; }.reactivate-link:focus-visible { outline:2px solid #08619c; outline-offset:2px; }</style>
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

            <h2 class="reset-title">
                <asp:Label
                    ID="lblTitle"
                    runat="server"
                    Text="Reset Password" />
            </h2>

            <p class="instruction-text">
                <asp:Label
                    ID="lblInstruction"
                    runat="server"
                    Text="Enter your registered email to receive an OTP." />
            </p>

            <!-- STEP INDICATOR -->
            <div class="step-indicator">

                <asp:Panel
                    ID="dotStep1"
                    runat="server"
                    CssClass="step-dot active" />

                <asp:Panel
                    ID="dotStep2"
                    runat="server"
                    CssClass="step-dot" />

                <asp:Panel
                    ID="dotStep3"
                    runat="server"
                    CssClass="step-dot" />

            </div>

            <!-- STEP 1: EMAIL -->
            <asp:Panel
                ID="pnlEmailStep"
                runat="server">

                <div class="form-group">

                    <label>Registered Email Address</label>

                    <asp:TextBox
                        ID="txtEmail"
                        runat="server"
                        TextMode="Email"
                        CssClass="input-field"
                        MaxLength="256"
                        autocomplete="email"
                        placeholder="Enter your registered email" />

                </div>

                <div class="button-group">

                    <asp:Button
                        ID="btnSendOtp"
                        runat="server"
                        Text="Get OTP"
                        CssClass="btn btn-primary"
                        OnClick="btnSendOtp_Click" />

                </div>

            </asp:Panel>

            <!-- STEP 2: OTP -->
            <asp:Panel
                ID="pnlOtpStep"
                runat="server"
                Visible="false">

                <div class="form-group">

                    <label>Verification Code</label>

                    <asp:TextBox
                        ID="txtOtp"
                        runat="server"
                        CssClass="input-field"
                        MaxLength="6"
                        inputmode="numeric"
                        autocomplete="one-time-code"
                        placeholder="Enter the 6-digit OTP" />

                </div>

                <p class="info-note">
                    The OTP expires in 10 minutes.
                </p>

                <p class="login-hint">
                    Didn't receive the OTP? Please check your Junk/Spam
                    folder.
                </p>

                <div class="button-group otp-button-group">

                    <asp:Button
                        ID="btnVerifyOtp"
                        runat="server"
                        Text="Verify OTP"
                        CssClass="btn btn-primary"
                        OnClick="btnVerifyOtp_Click" />

                    <asp:Button
                        ID="btnResendOtp"
                        runat="server"
                        Text="Resend OTP"
                        CssClass="btn btn-outline"
                        CausesValidation="false"
                        OnClick="btnResendOtp_Click" />

                </div>

            </asp:Panel>

            <!-- STEP 3: NEW PASSWORD -->
            <asp:Panel
                ID="pnlPasswordStep"
                runat="server"
                Visible="false">

                <div class="form-group">

                    <label>New Password</label>

                    <asp:TextBox
                        ID="txtPassword"
                        runat="server"
                        TextMode="Password"
                        CssClass="input-field"
                        autocomplete="new-password" />

                </div>

                <div class="form-group">

                    <label>Confirm Password</label>

                    <asp:TextBox
                        ID="txtConfirm"
                        runat="server"
                        TextMode="Password"
                        CssClass="input-field"
                        autocomplete="new-password" />

                </div>

                <div class="button-group">

                    <asp:Button
                        ID="btnReset"
                        runat="server"
                        Text="Reset Password"
                        CssClass="btn btn-primary"
                        OnClick="btnReset_Click" />

                </div>

            </asp:Panel>

            <!-- MESSAGE -->
            <asp:Label
                ID="lblStatus"
                runat="server"
                CssClass="login-error-message"
                HtmlEncode="false"
                Visible="false" />

            <!--
                Hidden on purpose: this control is never rendered.
                It only exists so it has a UniqueID that the
                "reactivation appeal" link, embedded inline inside
                lblStatus's error text (see IssueOtp in the
                code-behind), can post back to.
            -->
            <asp:LinkButton ID="btnReactivate" runat="server"
                Text="reactivation appeal"
                Visible="false"
                CausesValidation="false" OnClick="btnReactivate_Click" />

            <!-- BACK TO LOGIN -->
            <asp:Panel
                ID="pnlBackLink"
                runat="server"
                CssClass="login-footer">

                <asp:HyperLink
                    ID="lnkLogin"
                    runat="server"
                    NavigateUrl="Login.aspx">
                    Back to Login
                </asp:HyperLink>

            </asp:Panel>

        </div>
    </div>

</form>
</body>
</html>
