<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserManagement.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.UserManagement" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>User Management</title>
    <link href="../Asm_StyleSheet/UserManagementStyle.css?v=table-scroll-5" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <div id="userManagementPage" class="container-fluid um-page um-fixed-page">

    <div class="um-heading-row">
        <h1 class="main-heading">User Management</h1>

        <div class="um-heading-switcher"
             role="group" aria-label="User category">

            <asp:Button ID="btnStudent" runat="server"
                Text="Students"
                CssClass="nav-link-custom active"
                OnClick="btnStudent_Click" />

            <asp:Button ID="btnTutor" runat="server"
                Text="Tutors"
                CssClass="nav-link-custom"
                OnClick="btnTutor_Click" />
        </div>
    </div>

    <div class="row">
        <div class="um-full-column">
            <div class="card-main">

                <div class="content-header">
                    <asp:Label ID="lblPageTitle" runat="server"
                        CssClass="content-title"
                        Text="Registered Students"></asp:Label>

                    <div class="search-box um-search">
                        <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="10.5" cy="10.5" r="6.5"/><path d="m16 16 5 5"/></svg>
                        <asp:TextBox ID="txtSearch" runat="server"
                            CssClass="form-control"
                            placeholder="Search ID or Name..." aria-label="Search users by ID or name"
                            AutoPostBack="true"
                            OnTextChanged="txtSearch_TextChanged">
                        </asp:TextBox>
                    </div>
                </div>

                <div class="action-bar">

                    <div class="left-actions">
<asp:Button ID="btnCreate" runat="server"
                            Text="Create New"
                            CssClass="btn-create"
                            Visible="false"
                            OnClick="btnCreate_Click" />
                        <asp:Button ID="btnEdit" runat="server"
                            Text="Edit"
                            CssClass="btn-edit"
                            Enabled="false"
                            OnClick="btnEdit_Click" />

                        <asp:Button ID="btnSuspend" runat="server"
                            Text="Suspend"
                            CssClass="btn-action"
                            Enabled="false"
                            OnClick="btnSuspend_Click"
                            OnClientClick="return confirm('Are you sure you want to suspend this user?');"/>

                        <asp:Button ID="btnDelete" runat="server"
                            Text="Delete"
                            CssClass="btn-action"
                            Enabled="false"
                            OnClick="btnDelete_Click" 
                            OnClientClick="return confirm('Are you sure you want to delete this user?');"/>
                    </div>



                </div>

                <div class="um-table-scroll" tabindex="0" role="region" aria-label="Scrollable users table">

                    <asp:GridView ID="gvUsers" runat="server"
                        CssClass="custom-grid"
                        AutoGenerateColumns="False"
                        DataKeyNames="UserID"
                        OnRowCreated="gvUsers_RowCreated"
                        OnRowDataBound="gvUsers_RowDataBound"
                        GridLines="None"
                        BorderStyle="None"
                        BorderWidth="0px"
                        OnRowCommand="gvUsers_RowCommand">

                        <Columns>
                        </Columns>

                    </asp:GridView>
                    
                    <asp:Panel ID="pnlNoUser" runat="server" Visible="false" CssClass="text-center py-4">
                        <p class="text-muted" style="font-style:italic;">No users found matching your search.</p>
                    </asp:Panel>
                </div>

            </div>
        </div>

    </div>

</div>
<script>
(function() {
    var root = document.getElementById("userManagementPage");
    document.documentElement.classList.add("um-viewport");
    function fitPanel() {
        var viewport = window.visualViewport ? window.visualViewport.height : window.innerHeight;
        root.style.height = Math.max(0, viewport - root.getBoundingClientRect().top - 16) + "px";
    }
    fitPanel();
    window.addEventListener("resize", fitPanel);
    window.addEventListener("load", fitPanel);
    if (window.visualViewport) window.visualViewport.addEventListener("resize", fitPanel);
    if (document.fonts && document.fonts.ready) document.fonts.ready.then(fitPanel);
}());
</script>

</asp:Content>
