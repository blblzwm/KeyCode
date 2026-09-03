<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserManagement.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.UserManagement" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>User Management</title>
    <link href="../Asm_StyleSheet/UserManagementStyle.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="container-fluid">

    <asp:HyperLink ID="lnkBack" runat="server" 
        NavigateUrl="../Asm_WebPage/AdminDashboard.aspx" 
        CssClass="btn-back">
        « Back to Dashboard
    </asp:HyperLink>

    <h1 class="main-heading">User Management</h1>

    <div class="row">

        <div class="col-md-3 mb-4">
            <div class="card-sidebar">
                <div class="sidebar-title">Categories</div>

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

        <div class="col-md-9 mb-4">
            <div class="card-main">

                <div class="content-header">
                    <asp:Label ID="lblPageTitle" runat="server"
                        CssClass="content-title"
                        Text="Registered Students"></asp:Label>

                    <div class="search-box">
                        <asp:TextBox ID="txtSearch" runat="server"
                            CssClass="form-control"
                            placeholder="Search ID or Name..."
                            AutoPostBack="true"
                            OnTextChanged="txtSearch_TextChanged">
                        </asp:TextBox>
                    </div>
                </div>

                <div class="action-bar">

                    <div class="left-actions">
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

                    <div class="right-actions">
                        <asp:Button ID="btnCreate" runat="server"
                            Text="Create New"
                            CssClass="btn-create"
                            Visible="false"
                            OnClick="btnCreate_Click" />
                    </div>

                </div>

                <div style="overflow-x:auto;">

                    <asp:GridView ID="gvUsers" runat="server"
                        CssClass="custom-grid"
                        AutoGenerateColumns="False"
                        DataKeyNames="UserID"
                        OnRowCreated="gvUsers_RowCreated"
                        OnRowDataBound="gvUsers_RowDataBound"
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

</asp:Content>