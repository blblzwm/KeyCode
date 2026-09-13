<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.AdminDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    
    <link href="<%= ResolveUrl("~/Asm_StyleSheet/AdminStyle.css?v=student-match-4") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="dashboard-container admin-overview">

        <h1 class="welcome-text">
            Welcome back, <asp:Literal ID="litWelcomeName" runat="server"></asp:Literal>
        </h1>

        <div class="admin-summary" aria-label="Active account summary">
<div class="admin-summary-card tone-0"><span class="summary-icon" aria-hidden="true"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"><path d="m2 9 10-5 10 5-10 5L2 9Z"/><path d="M6 11v6c4 3 8 3 12 0v-6M22 9v7"/></svg></span><div class="summary-copy"><span>Active Students</span><strong><asp:Literal ID="litStudentCount" runat="server" Text="—" /></strong><small>Student accounts</small></div></div>
<div class="admin-summary-card tone-1"><span class="summary-icon" aria-hidden="true"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="13" rx="2"/><path d="M8 21h8M12 16v5M7 8h10M7 11h6"/></svg></span><div class="summary-copy"><span>Active Tutors</span><strong><asp:Literal ID="litTutorsCount" runat="server" Text="—" /></strong><small>Teaching accounts</small></div></div>
<div class="admin-summary-card tone-2"><span class="summary-icon" aria-hidden="true"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"><circle cx="9" cy="8" r="3"/><path d="M3 21v-3a6 6 0 0 1 12 0v3M16 5a3 3 0 0 1 0 6M18 15a5 5 0 0 1 3 4v2"/></svg></span><div class="summary-copy"><span>Total Active Learners</span><strong><asp:Literal ID="litTotalCount" runat="server" Text="—" /></strong><small>Students and tutors combined</small></div></div>
</div>
        <h2 class="subtitle">Learning materials</h2>

        <!-- CHAPTERS -->
        <div class="chapter-container">

            <asp:HyperLink ID="lnkChapter1" runat="server"
                CssClass="chapter-card"
                NavigateUrl="~/Asm_WebPage/LearningMaterial.aspx?chapter=C001">
                <div class="chapter-img-wrap">
                    <img src="../chp1.png" alt="Python Fundamentals" class="chapter-img" />
                </div>
                <div class="chapter-body">
                    <div class="chapter-title">Python Fundamentals</div>
                    <div class="chapter-meta">Chapter 1</div>
                    <p class="chapter-desc">Learn variables, data types, user input, and output. Write your first Python programs from scratch.</p>
                    <div class="chapter-pills"><span class="chapter-arrow">View Learning Materials →</span></div>
                </div>
            </asp:HyperLink>

            <asp:HyperLink ID="lnkChapter2" runat="server"
                CssClass="chapter-card"
                NavigateUrl="~/Asm_WebPage/LearningMaterial.aspx?chapter=C002">
                <div class="chapter-img-wrap">
                    <img src="../chp2.png" alt="Decision and Loop Controls" class="chapter-img" />
                </div>
                <div class="chapter-body">
                    <div class="chapter-title">Decision & Loop Controls</div>
                    <div class="chapter-meta">Chapter 2</div>
                    <p class="chapter-desc">Control program flow using conditions and loops. Make decisions and repeat actions intelligently.</p>
                    <div class="chapter-pills"><span class="chapter-arrow">View Learning Materials →</span></div>
                </div>
            </asp:HyperLink>

            <asp:HyperLink ID="lnkChapter3" runat="server"
                CssClass="chapter-card"
                NavigateUrl="~/Asm_WebPage/LearningMaterial.aspx?chapter=C003">
                <div class="chapter-img-wrap">
                    <img src="../chp3.png" alt="Basic Data Structures" class="chapter-img" />
                </div>
                <div class="chapter-body">
                    <div class="chapter-title">Basic Data Structures</div>
                    <div class="chapter-meta">Chapter 3</div>
                    <p class="chapter-desc">Store and manage data using lists, tuples, and sets. Organize multiple values with ease.</p>
                    <div class="chapter-pills"><span class="chapter-arrow">View Learning Materials →</span></div>
                </div>
            </asp:HyperLink>

        </div>

<section class="admin-appeals" aria-labelledby="appealsHeading">
<div class="admin-appeals-heading"><div><h2 id="appealsHeading">Reactivation Requests</h2><p>Keep track of account appeals and requests awaiting review.</p></div>
<a href="<%= ResolveUrl("~/Asm_WebPage/ReactivationRequest.aspx") %>">View appeals →</a></div>
<div class="admin-appeal-stats">
<div><span>Pending</span><strong><asp:Literal ID="litPendingCount" runat="server" Text="0" /></strong></div>
<div><span>Approved</span><strong><asp:Literal ID="litApprovedCount" runat="server" Text="0" /></strong></div>
<div><span>Total Appeals</span><strong><asp:Literal ID="litTotalAppealsCount" runat="server" Text="0" /></strong></div>
</div></section>
</div>
</asp:Content>
