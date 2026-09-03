<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="ForgotPassword.aspx.cs"
    Inherits="WAPP_Asm.Asm_WebPage.ForgotPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<title>KEYCODE - Forgot Password</title>
<link href="<%= ResolveUrl("~/Asm_StyleSheet/Login.css") %>" rel="stylesheet" />
</head>

<body>
<form id="form1" runat="server">
<div class="login-wrapper">
<div class="login-card">

    <header>
        <img src="../logo.png" style="width:265px; height:82px;" alt="KeyCode" />
    </header>

    <h2 class="reset-title">Forgot Password</h2>

    <!-- Step dots -->
    <div class="step-indicator">
        <asp:Panel ID="dotStep1" runat="server" CssClass="step-dot active" />
        <asp:Panel ID="dotStep2" runat="server" CssClass="step-dot" />
        <asp:Panel ID="dotStep3" runat="server" CssClass="step-dot" />
    </div>

    <!-- ── STEP 1: Enter Username ──────────────────────────────────── -->
    <asp:Panel ID="pnlStep1" runat="server">
        <p class="instruction-text">Enter your username to begin.</p>

        <div class="form-group">
            <label>Username</label>
            <asp:TextBox ID="txtUsername" runat="server" CssClass="input-field"
                placeholder="Your registered username" />
        </div>

        <div class="button-group">
            <asp:Button ID="btnFindAccount" runat="server"
                Text="Next →"
                CssClass="btn btn-primary"
                OnClick="btnFindAccount_Click" />
        </div>
    </asp:Panel>

    <!-- ── STEP 2: DOB + Security Answer ──────────────────────────── -->
    <asp:Panel ID="pnlStep2" runat="server" Visible="false">
        <p class="instruction-text">Verify your identity to unlock password reset.</p>

        <div class="form-group">
            <label>Date of Birth</label>
            <asp:TextBox ID="txtDOB" runat="server"
                TextMode="Date" CssClass="input-field" />
        </div>

        <div class="form-group">
            <label>Security Question</label>
            <div class="question-box">
                <asp:Literal ID="litQuestion" runat="server" />
            </div>
        </div>

        <div class="form-group">
            <label>Your Answer</label>
            <asp:TextBox ID="txtAnswer" runat="server" CssClass="input-field"
                placeholder="Answer is case-insensitive" />
        </div>

        <p class="info-note">Both your date of birth and security answer must match your registration details.</p>

        <div class="button-group">
            <asp:Button ID="btnVerify" runat="server"
                Text="Verify Identity"
                CssClass="btn btn-primary"
                OnClick="btnVerifyIdentity_Click" />
        </div>
    </asp:Panel>

    <!-- ── STEP 3: Set New Password ────────────────────────────────── -->
    <asp:Panel ID="pnlStep3" runat="server" Visible="false">
        <p class="instruction-text">Enter your new password below.</p>

        <div class="form-group">
            <label>New Password</label>
            <asp:TextBox ID="txtPassword" runat="server"
                TextMode="Password" CssClass="input-field"
                onkeyup="validatePassword()" />
            <ul class="password-rules">
                <li id="rule-length" data-text="Minimum 8 characters">Minimum 8 characters</li>
                <li id="rule-upper"  data-text="At least 1 uppercase letter">At least 1 uppercase letter</li>
                <li id="rule-number" data-text="At least 1 number">At least 1 number</li>
                <li id="rule-symbol" data-text="At least 1 symbol">At least 1 symbol</li>
            </ul>
        </div>

        <div class="form-group">
            <label>Confirm Password</label>
            <asp:TextBox ID="txtConfirm" runat="server"
                TextMode="Password" CssClass="input-field"
                onkeyup="checkMatch()" />
            <small id="matchMsg"></small>
        </div>

        <div class="button-group">
            <asp:Button ID="btnReset" runat="server"
                Text="Reset Password"
                CssClass="btn btn-primary"
                OnClick="btnReset_Click" />
        </div>
    </asp:Panel>

    <!-- Error / success message -->
    <asp:Label ID="lblMessage" runat="server"
        CssClass="login-error-message" Visible="false" />

    <div class="login-footer">
        Remembered your password?
        <asp:HyperLink runat="server" NavigateUrl="Login.aspx">Back to Login</asp:HyperLink>
    </div>

</div>
</div>
</form>

<script>
    // Update step dots based on which panel is active
    window.onload = function () {
        var s2 = '<%= pnlStep2.Visible ? "true" : "false" %>';
        var s3 = '<%= pnlStep3.Visible ? "true" : "false" %>';

        if (s3 === 'true') {
            setDot('<%= dotStep1.ClientID %>', 'done');
            setDot('<%= dotStep2.ClientID %>', 'done');
            setDot('<%= dotStep3.ClientID %>', 'active');
        } else if (s2 === 'true') {
            setDot('<%= dotStep1.ClientID %>', 'done');
            setDot('<%= dotStep2.ClientID %>', 'active');
        }
    };

    function setDot(id, state) {
        document.getElementById(id).className = 'step-dot ' + state;
    }

    function validatePassword() {
        const pass    = document.getElementById('<%= txtPassword.ClientID %>').value;
        const rulesBox = document.querySelector(".password-rules");

        if (pass.length > 0) rulesBox.classList.add("show");
        else { rulesBox.classList.remove("show"); return; }

        updateRule("rule-length", pass.length >= 8);
        updateRule("rule-upper",  /[A-Z]/.test(pass));
        updateRule("rule-number", /[0-9]/.test(pass));
        updateRule("rule-symbol", /[\W]/.test(pass));
        checkMatch();
    }

    function updateRule(id, ok) {
        const el   = document.getElementById(id);
        const text = el.getAttribute("data-text");
        el.classList.toggle("rule-ok",  ok);
        el.classList.toggle("rule-bad", !ok);
        el.innerHTML = (ok ? "✔ " : "ⓘ ") + text;
    }

    function checkMatch() {
        const pass    = document.getElementById('<%= txtPassword.ClientID %>').value;
        const confirm = document.getElementById('<%= txtConfirm.ClientID %>').value;
        const msg = document.getElementById("matchMsg");
        if (!confirm) { msg.innerText = ""; return; }
        if (pass === confirm) {
            msg.innerText = "✔ Passwords match";
            msg.style.color = "#16a34a";
        } else {
            msg.innerText = "ⓘ Passwords do not match";
            msg.style.color = "#dc2626";
        }
    }
</script>
</body>
</html>