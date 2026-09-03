<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.AdminDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Admin Dashboard</title>
    <link href="../Asm_StyleSheet/AdminStyle.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="dashboard-container">

        <h1 class="welcome-text">
            Welcome back, <asp:Literal ID="litWelcomeName" runat="server"></asp:Literal>
        </h1>

        <br />
        <p class="subtitle">Available Courses:</p>

        <!-- CHAPTERS -->
        <div class="chapter-container">

            <asp:HyperLink ID="lnkChapter1" runat="server"
                CssClass="chapter-card"
                NavigateUrl="~/Asm_WebPage/LearningMaterial.aspx?chapter=C001">
                <div class="chapter-img-wrap">
                    <img src="../chp1.png" alt="Python Fundamentals" class="chapter-img" />
                    <span class="chapter-badge">Chapter 1</span>
                </div>
                <div class="chapter-body">
                    <div class="chapter-title">Python Fundamentals</div>
                    <p class="chapter-desc">Learn variables, data types, user input, and output. Write your first Python programs from scratch.</p>
                    <span class="chapter-arrow">View Learning Materials →</span>
                </div>
            </asp:HyperLink>

            <asp:HyperLink ID="lnkChapter2" runat="server"
                CssClass="chapter-card"
                NavigateUrl="~/Asm_WebPage/LearningMaterial.aspx?chapter=C002">
                <div class="chapter-img-wrap">
                    <img src="../chp2.png" alt="Decision and Loop Controls" class="chapter-img" />
                    <span class="chapter-badge">Chapter 2</span>
                </div>
                <div class="chapter-body">
                    <div class="chapter-title">Decision & Loop Controls</div>
                    <p class="chapter-desc">Control program flow using conditions and loops. Make decisions and repeat actions intelligently.</p>
                    <span class="chapter-arrow">View Learning Materials →</span>
                </div>
            </asp:HyperLink>

            <asp:HyperLink ID="lnkChapter3" runat="server"
                CssClass="chapter-card"
                NavigateUrl="~/Asm_WebPage/LearningMaterial.aspx?chapter=C003">
                <div class="chapter-img-wrap">
                    <img src="../chp3.png" alt="Basic Data Structures" class="chapter-img" />
                    <span class="chapter-badge">Chapter 3</span>
                </div>
                <div class="chapter-body">
                    <div class="chapter-title">Basic Data Structures</div>
                    <p class="chapter-desc">Store and manage data using lists, tuples, and sets. Organize multiple values with ease.</p>
                    <span class="chapter-arrow">View Learning Materials →</span>
                </div>
            </asp:HyperLink>

        </div>

        <!-- MANAGEMENT GRID -->
        <div class="management-grid">

            <!-- left col (analytics) -->
            <div class="management-section management-tall">
                <div>
                    <h3 class="analytics-title">Analytics</h3>
                    <p class="analytics-desc">Monitor forum activity and view student performance insights across all chapters.</p>
                    <div class="analytics-img-wrap">
                        <asp:Image runat="server"
                            ImageUrl="~/analytics2_gif.gif"
                            AlternateText="Analytics gif."
                            CssClass="analytics-img-large" Height="195px" Width="215px" />
                    </div>
                </div>
                <asp:Button ID="btnAnalytics" runat="server"
                    CssClass="management-btn analytics-btn"
                    Text="View Analytics →"
                    OnClick="btnAnalytics_Click" />
            </div>

            <!-- right col (appeals and user management) -->
            <div class="management-right-col">

                <!-- top: Appeals -->
                <div class="management-section">
                    <div class="management-top">
                        <div class="management-info">
                            <div class="management-icon">
                                <asp:Image CssClass="management-img" runat="server"
                                    ImageUrl="~/appeal_icon.png"
                                    AlternateText="Reactivation Appeals icon."
                                    Height="47px" Width="47px"/>
                            </div>
                            <div>
                                <h3>Reactivation Requests</h3>
                                <p>Review and process account reactivation appeals submitted by suspended students and tutors.</p>
                            </div>
                        </div>
                        <asp:Button ID="btnReactivationRequest" runat="server"
                            CssClass="management-btn"
                            Text="View Appeals →"
                            OnClick="btnReactivationRequest_Click" />
                    </div>
                    <div class="management-stats">
                        <div class="stat-box">
                            <p class="stat-label">Pending</p>
                            <p class="stat-value">
                                <asp:Literal ID="litPendingCount" runat="server" Text="0"></asp:Literal>
                            </p>
                        </div>
                        <div class="stat-box">
                            <p class="stat-label">Approved</p>
                            <p class="stat-value">
                                <asp:Literal ID="litApprovedCount" runat="server" Text="0"></asp:Literal>
                            </p>
                        </div>
                        <div class="stat-box">
                            <p class="stat-label">Total Appeals</p>
                            <p class="stat-value">
                                <asp:Literal ID="litTotalAppealsCount" runat="server" Text="0"></asp:Literal>
                            </p>
                        </div>
                    </div>
                </div>

                <!-- bottom: User Management -->
                <div class="management-section">
                    <div class="management-top">
                        <div class="management-info">
                            <div class="management-icon">
                                <asp:Image CssClass="management-img" runat="server"
                                    ImageUrl="~/user_icon.jpg"
                                    AlternateText="User icon."
                                    Height="47px" Width="47px"/>
                            </div>
                            <div>
                                <h3>User Management</h3>
                                <p>Manage tutor and student accounts, update roles, and maintain access control.</p>
                            </div>
                        </div>
                        <asp:Button ID="btnUserManagement" runat="server"
                            CssClass="management-btn"
                            Text="Manage Users →"
                            OnClick="btnUserManagement_Click" />
                    </div>
                    <div class="management-stats">
                        <div class="stat-box">
                            <p class="stat-label">Students</p>
                            <p class="stat-value">
                                <asp:Literal ID="litStudentCount" runat="server" Text="--"></asp:Literal>
                            </p>
                        </div>
                        <div class="stat-box">
                            <p class="stat-label">Tutors</p>
                            <p class="stat-value">
                                <asp:Literal ID="litTutorsCount" runat="server" Text="--"></asp:Literal>
                            </p>
                        </div>
                        <div class="stat-box">
                            <p class="stat-label">Total Users</p>
                            <p class="stat-value">
                                <asp:Literal ID="litTotalCount" runat="server" Text="--"></asp:Literal>
                            </p>
                        </div>
                    </div>
                </div>

            </div>
        </div>

    </div>
</asp:Content>