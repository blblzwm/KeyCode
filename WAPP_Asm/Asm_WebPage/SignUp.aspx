<%@ Page Title="Sign Up" Language="C#" AutoEventWireup="true"
    CodeBehind="SignUp.aspx.cs"
    Inherits="WAPP_Asm.Asm_WebPage.SignUp" %>

<!DOCTYPE html>
<html>
<head runat="server">

<script src="https://www.google.com/recaptcha/api.js" async defer></script>
<title>KEYCODE - Sign Up</title>
<link href="<%= ResolveUrl("~/Asm_StyleSheet/Login.css") %>" rel="stylesheet" />
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
        <asp:TextBox ID="txtUsername" runat="server" CssClass="input-field" />
    </div>

    <div class="form-group">
        <label>Password</label>
        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"
            CssClass="input-field" onkeyup="validatePassword()" />
        <ul class="password-rules">
            <li id="rule-length" data-text="Minimum 8 characters">Minimum 8 characters</li>
            <li id="rule-upper"  data-text="At least 1 uppercase letter">At least 1 uppercase letter</li>
            <li id="rule-number" data-text="At least 1 number">At least 1 number</li>
            <li id="rule-symbol" data-text="At least 1 symbol">At least 1 symbol</li>
        </ul>
    </div>

    <div class="form-group">
        <label>Confirm Password</label>
        <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password"
            CssClass="input-field" onkeyup="checkMatch()" />
        <small id="matchMsg"></small>
    </div>

    <div class="form-group">
        <label>First Name</label>
        <asp:TextBox ID="txtFname" runat="server" CssClass="input-field" />
    </div>

    <div class="form-group">
        <label>Last Name</label>
        <asp:TextBox ID="txtLname" runat="server" CssClass="input-field" />
    </div>

    <div class="form-group">
        <label>Email Address</label>
        <asp:TextBox ID="txtEmail" runat="server" TextMode="Email"
            CssClass="input-field" onkeyup="validateEmail()" />
        <small id="emailMsg"></small>
    </div>

    <div class="form-group">
        <label>Date of Birth</label>
        <asp:TextBox ID="txtDOB" runat="server" TextMode="Date" CssClass="input-field" />
    </div>

    <!-- ── ACCOUNT RECOVERY SECTION ──────────────────────────────── -->
    <p class="section-label">Account Recovery</p>

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
    <asp:RequiredFieldValidator
        ID="rfvSecurityQuestion"
        runat="server"
        ControlToValidate="ddlSecurityQuestion"
        InitialValue=""
        ErrorMessage="ⓘ Please select a security question."
        CssClass="validator-text"
        Display="Dynamic"
        ValidationGroup="RegisterGroup" />

    <div class="form-group">
        <label>Your Answer</label>
        <asp:TextBox ID="txtSecurityAnswer" runat="server" CssClass="input-field"
            placeholder="Type your answer here" />
        <span class="security-hint">⚠ Remember this. You will need it to reset your password.</span>
    </div>
    <asp:RequiredFieldValidator
        ID="rfvSecurityAnswer"
        runat="server"
        ControlToValidate="txtSecurityAnswer"
        ErrorMessage="ⓘ Please provide an answer to your security question."
        CssClass="validator-text"
        Display="Dynamic"
        ValidationGroup="RegisterGroup" />
    <!-- ──────────────────────────────────────────────────────────── -->

    <div class="form-group">
        <div class="recaptcha-wrapper">
            <div class="g-recaptcha"
                 data-sitekey="6LegAWwsAAAAACMX8xzbmSptxbl1piKrqICGBRwr">
            </div>
        </div>
    </div>

    <asp:Label ID="lblMessage" runat="server"
        CssClass="login-error-message" Visible="false" />

    <div class="button-group">
        <asp:Button ID="btnRegister" runat="server"
            Text="Register"
            CssClass="btn btn-primary"
            OnClick="btnRegister_Click"
            ValidationGroup="RegisterGroup"/>
    </div>

    <div class="login-footer">
        Already have an account?
        <asp:HyperLink ID="lnkLogin" runat="server" NavigateUrl="Login.aspx">Login</asp:HyperLink>
    </div>

</div>
</div>
</form>

<script>
    function validatePassword() {
        const pass = document.getElementById('<%= txtPassword.ClientID %>').value;
        const rulesBox = document.querySelector(".password-rules");

        if (pass.length > 0) rulesBox.classList.add("show");
        else { rulesBox.classList.remove("show"); return; }

        updateRule("rule-length", pass.length >= 8);
        updateRule("rule-upper", /[A-Z]/.test(pass));
        updateRule("rule-number", /[0-9]/.test(pass));
        updateRule("rule-symbol", /[\W]/.test(pass));
        checkMatch();
    }

    function updateRule(id, ok) {
        const el = document.getElementById(id);
        const text = el.getAttribute("data-text");
        el.classList.toggle("rule-ok", ok);
        el.classList.toggle("rule-bad", !ok);
        el.innerHTML = (ok ? "✔ " : "ⓘ ") + text;
    }

    function checkMatch() {
        const pass = document.getElementById('<%= txtPassword.ClientID %>').value;
        const confirm = document.getElementById('<%= txtConfirm.ClientID %>').value;
        const msg     = document.getElementById("matchMsg");
        if (!confirm) { msg.innerText = ""; return; }
        if (pass === confirm) {
            msg.innerText   = "✔ Passwords match";
            msg.style.color = "#16a34a";
        } else {
            msg.innerText   = "ⓘ Passwords do not match";
            msg.style.color = "#dc2626";
        }
    }

    function validateEmail() {
        const email = document.getElementById('<%= txtEmail.ClientID %>').value;
        const msg = document.getElementById("emailMsg");
        if (!email) { msg.innerText = ""; return; }
        const ok = /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email);
        msg.innerText = ok ? "✔ Valid email format" : "ⓘ Invalid email format";
        msg.style.color = ok ? "#16a34a" : "#dc2626";
    }
</script>
</body>
</html>