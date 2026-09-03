<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Asm_StyleSheet/ProfileStyle.css") %>" rel="stylesheet"/>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<div class="profile-card">

    <asp:Label ID="lblProfileMsg"
        runat="server"
        CssClass="success-msg"
        Visible="false" />

    <div class="profile-header">

        <!-- LEFT: Back -->
        <asp:Button ID="btnBack" runat="server"
            Text="Back"
            CssClass="verify-btn back-btn"
            OnClick="btnBack_Click"
            CausesValidation="false" />

        <!-- CENTER: Title -->
        <h2 class="profile-title center-title">My Profile</h2>

        <!-- RIGHT: Edit -->
        <asp:Button ID="btnEdit" runat="server"
            Text="Edit"
            CssClass="verify-btn"
            OnClick="btnEdit_Click"
            CausesValidation="false" />

    </div>

    <!-- Avatar -->
    <div class="avatar-wrapper">

        <asp:Image ID="imgAvatar" runat="server" CssClass="profile-avatar" />

    <asp:Panel ID="pnlAvatarUpload" runat="server" Visible="false" CssClass="avatar-actions">

        <asp:FileUpload ID="fuAvatar" runat="server" CssClass="file-clean" />

        <asp:Button ID="btnUploadAvatar" runat="server"
            Text="Upload"
            CssClass="verify-btn"
            OnClick="btnUploadAvatar_Click"
            CausesValidation="false"
            OnClientClick="return confirm('Upload this profile picture?');" />

        <asp:Button ID="btnRemoveAvatar" runat="server"
            Text="Remove"
            CssClass="verify-btn"
            OnClick="btnRemoveAvatar_Click"
            CausesValidation="false"
            OnClientClick="return confirm('Remove profile picture?');" />

        <asp:Label 
            ID="lblAvatarMsg" 
            runat="server" 
            CssClass="validator-text" />

    </asp:Panel>

    </div>

    <!-- Account -->
    <h4 class="section-header">Account Information</h4>

    <div class="detail-item">
        <span class="label">User ID</span>
        <span class="value">
            <asp:Label ID="lblUserID" runat="server" />
        </span>
    </div>

    <div class="detail-item">
        <span class="label">Username</span>
        <span class="value">

            <asp:Label ID="lblUsername" runat="server" />
            <asp:TextBox ID="txtUsername" runat="server" Visible="false" />

            <asp:Label 
                ID="lblUsernameMsg"
                runat="server"
                CssClass="validation-error"
                Visible="false" />

            <asp:RequiredFieldValidator
                ID="reqUsername"
                runat="server"
                ControlToValidate="txtUsername"
                ErrorMessage="ⓘ Username is required."
                CssClass="validator-text"
                Display="Dynamic" />

            <asp:RegularExpressionValidator
                ID="revUsername"
                runat="server"
                ControlToValidate="txtUsername"
                ValidationExpression="^(?=(?:.*[A-Za-z]){3,})(?!.*[_-]{2})[A-Za-z0-9][A-Za-z0-9_-]{1,18}[A-Za-z0-9]$"
                ErrorMessage="ⓘ Username must be 3–20 characters and contain at least 3 letters."
                CssClass="validation-error"
                Display="Dynamic" />

        </span>
    </div>

    <!-- Personal -->
    <h4 class="section-header">Personal Details</h4>

    <div class="detail-item">
        <span class="label">First Name</span>
        <span class="value">

            <asp:Label ID="lblFname" runat="server" />
            <asp:TextBox ID="txtFname" runat="server" Visible="false" />

            <asp:RequiredFieldValidator
                ID="reqFname"
                runat="server"
                ControlToValidate="txtFname"
                ErrorMessage="ⓘ First name is required."
                CssClass="validator-text"
                Display="Dynamic" />

            <asp:RegularExpressionValidator 
                ID="revFname"
                runat="server"
                ControlToValidate="txtFname"
                ValidationExpression="^[A-Za-z\s\-'/]+$"
                ErrorMessage="ⓘ First name cannot contain numbers."
                CssClass="validation-error"
                Display="Dynamic" />

        </span>
    </div>

    <div class="detail-item">
        <span class="label">Last Name</span>
        <span class="value">

            <asp:Label ID="lblLname" runat="server" />
            <asp:TextBox ID="txtLname" runat="server" Visible="false" />

            <asp:RequiredFieldValidator
                ID="reqLname"
                runat="server"
                ControlToValidate="txtLname"
                ErrorMessage="ⓘ Last name is required."
                CssClass="validator-text"
                Display="Dynamic" />

            <asp:RegularExpressionValidator 
                ID="revLname"
                runat="server"
                ControlToValidate="txtLname"
                ValidationExpression="^[A-Za-z\s\-'/]+$"
                ErrorMessage="ⓘ Invalid last name."
                CssClass="validation-error"
                Display="Dynamic" />

        </span>
    </div>

    <div class="detail-item">
        <span class="label">Email</span>
        <span class="value">

            <asp:Label ID="lblEmail" runat="server" />
            <asp:TextBox ID="txtEmail" runat="server" Visible="false" />

            <asp:Label 
                ID="lblEmailMsg"
                runat="server"
                CssClass="validation-error"
                Visible="false" />

            <asp:RequiredFieldValidator
                ID="reqEmail"
                runat="server"
                ControlToValidate="txtEmail"
                ErrorMessage="ⓘ Email is required."
                CssClass="validator-text"
                Display="Dynamic" />

            <asp:RegularExpressionValidator
                ID="revEmail"
                runat="server"
                ControlToValidate="txtEmail"
                ValidationExpression="^[^\s@]+@[^\s@]+\.[^\s@]+$"
                ErrorMessage="ⓘ Invalid email format."
                CssClass="validation-error"
                Display="Dynamic" />

        </span>
    </div>

    <div class="detail-item">
        <span class="label">DOB</span>
        <span class="value">

            <asp:Label ID="lblDOB" runat="server" />
            <asp:TextBox ID="txtDOB" runat="server" TextMode="Date" Visible="false" />

            <asp:Label
                ID="lblDOBMsg"
                runat="server"
                CssClass="validation-error"
                Visible="false" />

        </span>
    </div>

    <!-- Professional -->

    <asp:Panel ID="pnlProfessional" runat="server">
        <h4 class="section-header">Professional Information</h4>

        <div class="detail-item">
            <span class="label">Qualification</span>
            <span class="value">

                <asp:Label ID="lblQualification" runat="server" />

                <asp:DropDownList ID="ddlQualification" runat="server" CssClass="input-field" Visible="false">
                    <asp:ListItem Text="Select Qualification" Value="" />
                    <asp:ListItem Text="Bachelor" Value="Bachelor" />
                    <asp:ListItem Text="Master" Value="Master" />
                    <asp:ListItem Text="PhD" Value="PhD" />
                </asp:DropDownList>

                <asp:RequiredFieldValidator
                    ID="reqQualification"
                    runat="server"
                    ControlToValidate="ddlQualification"
                    InitialValues=""
                    ErrorMessage="ⓘ Qualification is required."
                    CssClass="validation-error"
                    Display="Dynamic" />

            </span>
        </div>
    </asp:Panel>

    <!-- Security -->
    <h4 class="section-header">Security</h4>

    <div class="detail-item">
        <span class="label">Password</span>
        <span class="value">

            <asp:Button ID="btnShowPassword" runat="server"
                Text="Change Password"
                CssClass="verify-btn"
                OnClick="btnShowPassword_Click"
                CausesValidation="false" />

            <asp:Panel ID="pnlPassword" runat="server" Visible="false" CssClass="verify-panel">

                <asp:TextBox ID="txtCurrentPassword" runat="server"
                    TextMode="Password" placeholder="Current password" />

                <asp:TextBox ID="txtNewPassword" runat="server"
                    TextMode="Password" placeholder="New password" />

                <asp:TextBox ID="txtConfirmPassword" runat="server"
                    TextMode="Password" placeholder="Confirm new password" />

                <asp:Button ID="btnChangePassword" runat="server"
                    Text="Update Password"
                    CssClass="verify-btn"
                    OnClick="btnChangePassword_Click"
                    CausesValidation="false" />

            </asp:Panel>

            <asp:Label ID="lblPasswordMsg" runat="server" CssClass="password-msg" />

        </span>
    </div>

    <asp:Panel ID="pnlEditActions" runat="server" Visible="false" style="margin-top:20px;">
        
    <asp:Button ID="btnSave" runat="server"
        Text="Save"
        CssClass="verify-btn"
        OnClick="btnSave_Click"
        CausesValidation="true" />

    <asp:Button ID="btnCancel" runat="server"
        Text="Cancel"
        CssClass="verify-btn"
        OnClick="btnCancel_Click"
        CausesValidation="false" />
    </asp:Panel>

</div>

    <script>
        document.addEventListener("DOMContentLoaded", function () {

            const input = document.querySelector("#<%= fuAvatar.ClientID %>");
            const avatar = document.querySelector("#<%= imgAvatar.ClientID %>");

            input.addEventListener("change", function () {
                const file = this.files[0];
                if (file) {
                    avatar.src = URL.createObjectURL(file);
                }
            });

        });
    </script>

</asp:Content>
