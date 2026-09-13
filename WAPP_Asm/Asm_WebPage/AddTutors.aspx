<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AddTutors.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.AddTutors" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Add Tutors</title>
    <link href="../Asm_StyleSheet/UserManagementStyle.css" rel="stylesheet" />
    <link href="../Asm_StyleSheet/AddTutorsStyle.css?v=wide-form-2" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="container add-tutor-page">

        <h1 class="main-heading">Add New Tutor</h1>

        <div class="card-form">
            
            <!-- Account Information -->
            <div class="form-section-title">Account Information</div>
            
            <div class="form-group-tutorId">
                <label for="txtUserID" class="form-label-tutorId">Tutor ID</label>
                <asp:TextBox ID="txtUserID" runat="server" 
                    CssClass="form-control" 
                    placeholder="Enter User ID" 
                    MaxLength="50">
                </asp:TextBox>
            </div>

            <div class="form-group">
                <label for="txtUsername" class="form-label">Username</label>
                <asp:TextBox ID="txtUsername" runat="server" 
                    CssClass="form-control" 
                    placeholder="Choose a username" 
                    MaxLength="50">
                </asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvUsername" runat="server"
                    ControlToValidate="txtUsername"
                    ErrorMessage="Username is required"
                    CssClass="validation-error"
                    Display="Dynamic"
                    SetFocusOnError="true" 
                    ValidationGroup="TutorGroup">

                </asp:RequiredFieldValidator>
                <asp:CustomValidator 
                    ID="cvUsn" 
                    runat="server"
                    ControlToValidate="txtUsername"
                    ErrorMessage="Username has been taken!"
                    CssClass="validation-error"
                    Display="Dynamic"
                    OnServerValidate="IsUsernameDuplicate"
                    ValidateEmptyText="false" 
                    SetFocusOnError="True" 
                    ValidationGroup="TutorGroup">

                </asp:CustomValidator>
            </div>

            <div class="form-group tutor-field-wide">
                <label for="txtPassword" class="form-label">Password</label>
                <asp:TextBox ID="txtPassword" runat="server"
                    TextMode="Password"
                    CssClass="form-control"
                    placeholder="Auto-generated from Username"
                    ReadOnly="true"
                    MaxLength="255">
                </asp:TextBox>
            </div>

            <!-- Personal Information -->
            <div class="form-section-title mt-4">Personal Details</div>
            
            <div class="form-group">
                <label for="txtFirstName" class="form-label">First Name</label>
                <asp:TextBox ID="txtFirstName" runat="server" 
                    CssClass="form-control" 
                    placeholder="Enter first name" 
                    MaxLength="50">
                </asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvFirstName" runat="server"
                    ControlToValidate="txtFirstName"
                    ErrorMessage="First name is required"
                    CssClass="validation-error"
                    Display="Dynamic"
                    SetFocusOnError="true" ValidationGroup="TutorGroup"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="revFname" runat="server" ControlToValidate="txtFirstName" CssClass="validation-error" Display="Dynamic" ErrorMessage="First name should only contain letters, spaces, hyphens, apostrophes or slashes" ValidationExpression="^[a-zA-Z\s\-'/]+$" ValidationGroup="TutorGroup"></asp:RegularExpressionValidator>
            </div>

            <div class="form-group">
                <label for="txtLastName" class="form-label">Last Name</label>
                <asp:TextBox ID="txtLastName" runat="server" 
                    CssClass="form-control" 
                    placeholder="Enter last name" 
                    MaxLength="50">
                </asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvLastName" runat="server"
                    ControlToValidate="txtLastName"
                    ErrorMessage="Last name is required"
                    CssClass="validation-error"
                    Display="Dynamic"
                    SetFocusOnError="true" ValidationGroup="TutorGroup"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="revLname" runat="server" ControlToValidate="txtLastName" CssClass="validation-error" Display="Dynamic" ErrorMessage="Last name should only contain letters, spaces, hyphens, apostrophes or slashes" ValidationExpression="^[a-zA-Z\s\-'/]+$" ValidationGroup="TutorGroup"></asp:RegularExpressionValidator>
            </div>

            <div class="form-group">
                <label for="txtEmail" class="form-label">Email Address</label>
                <asp:TextBox ID="txtEmail" runat="server" 
                    TextMode="Email"
                    CssClass="form-control" 
                    placeholder="example@gmail.com" 
                    MaxLength="50">
                </asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                    ControlToValidate="txtEmail"
                    ErrorMessage="Email is required"
                    CssClass="validation-error"
                    Display="Dynamic"
                    SetFocusOnError="true" ValidationGroup="TutorGroup"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="revEmail" runat="server"
                    ControlToValidate="txtEmail"
                    ErrorMessage="Invalid email format"
                    ValidationExpression="^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"
                    CssClass="validation-error"
                    Display="Dynamic"
                    SetFocusOnError="true" 
                    ValidationGroup="TutorGroup">
                </asp:RegularExpressionValidator>
                <asp:CustomValidator ID="cvEmail" 
                    runat="server" 
                    ControlToValidate="txtEmail" 
                    OnServerValidate="IsEmailDuplicate" 
                    CssClass="validation-error" 
                    Display="Dynamic" 
                    ErrorMessage="Email has been taken!" 
                    SetFocusOnError="True" 
                    ValidationGroup="TutorGroup"
                    ValidateEmptyText="false">

                </asp:CustomValidator>

            </div>

            <div class="form-group">
                <label for="txtDOB" class="form-label">Date of Birth</label>
                <asp:TextBox ID="txtDOB" runat="server" 
                    TextMode="Date"
                    CssClass="form-control">
                </asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvDOB" runat="server"
                    ControlToValidate="txtDOB"
                    ErrorMessage="Date of birth is required"
                    CssClass="validation-error"
                    Display="Dynamic"
                    SetFocusOnError="true" ValidationGroup="TutorGroup"></asp:RequiredFieldValidator>
            </div>

            <!-- Professional Information -->
            <div class="form-section-title mt-4">Professional Information</div>
            
            <div class="form-group">
                <label for="ddlQualification" class="form-label">Qualification</label>
                <asp:DropDownList ID="ddlQualification" runat="server" 
                    CssClass="form-select">
                    <asp:ListItem Value="" Text="Select highest qualification" Selected="True"></asp:ListItem>
                    <asp:ListItem Value="Bachelor">Bachelor</asp:ListItem>
                    <asp:ListItem Value="Master">Master</asp:ListItem>
                    <asp:ListItem Value="PhD">PhD</asp:ListItem>
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvQualification" runat="server"
                    ControlToValidate="ddlQualification"
                    InitialValue=""
                    ErrorMessage="Qualification is required"
                    CssClass="validation-error"
                    Display="Dynamic"
                    SetFocusOnError="true" 
                    ValidationGroup="TutorGroup"></asp:RequiredFieldValidator>
            </div>

            <!--Validation Summary-->
            <div class="summary-divider">
                <asp:ValidationSummary ID="vsSummary" runat="server" CssClass="validation-summary" HeaderText="Please fix the following errors:" ValidationGroup="TutorGroup" />
            </div>

            <!-- Actions -->
            <div class="btn-group-actions">
                <asp:Button ID="btnSave" 
                    runat="server" 
                    Text="Save Tutor" 
                    CssClass="btn-save" 
                    OnClick="btnSave_Click"
                    CausesValidation="true"
                    ValidationGroup ="TutorGroup"/>
                
                <asp:Button ID="btnCancel" runat="server" 
                    Text="Cancel" 
                    CssClass="btn-cancel" 
                    OnClick="btnCancel_Click"
                    CausesValidation="false" />
            </div>

        </div>
    </div>

    <!--javascript: auto-fill password from username-->
    <script type = "text/javascript" >
        document.addEventListener("DOMContentLoaded", function () {
            var usernameBox = document.getElementById('<%= txtUsername.ClientID %>');
            var passwordBox = document.getElementById('<%= txtPassword.ClientID %>');

            usernameBox.addEventListener("input", function () {
                passwordBox.value = usernameBox.value.trim().toLowerCase();
            });
        });
    </script>

</asp:Content>
