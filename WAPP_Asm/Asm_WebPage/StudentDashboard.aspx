<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="StudentDashboard.aspx.cs" Inherits="WAPP_Asm.StudentDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <link href="<%= ResolveUrl("~/Asm_StyleSheet/StudentDashboardStyle.css?v=" + DateTime.Now.Ticks) %>" rel="stylesheet" />

    <div class="hero-clean">
        <h2 class="hero-clean-title">
            Start your <span>Python</span> journey.
        </h2>

        <p class="hero-clean-desc">
            Learn step by step, experiment freely, and grow your skills through practice.
        </p>
    </div>

    <div class="dash-grid">
        <div class="dash-left">
            <div class="dash-sub">
                <asp:Literal ID="litSubWelcome" runat="server" />
            </div>
        </div>
    </div>

    <div class="course-list">
        <asp:Repeater ID="rptStudentDashboard" runat="server">
            <ItemTemplate>

                <a class='course-row <%# (bool)Eval("IsLocked") ? "locked" : "" %>'
                   href='<%# (bool)Eval("IsLocked") ? ResolveUrl("~/Asm_WebPage/Login.aspx") : Eval("Link") %>'>

                    <div class="course-thumb">
                        <img class="thumb-img"
                             src="<%# ResolveUrl(Eval("ImageUrl").ToString()) %>"
                             alt='<%# Eval("Title") + " cover image" %>'
                             onerror="this.style.display='none'; this.nextElementSibling.style.display='block';" />

                        <div class="thumb-grad" style="display:none;"></div>
                    </div>

                    <div class="course-info">
                        <div class="course-name"><%# Eval("Title") %></div>
                        <div class="course-meta"><%# Eval("TeacherNames") %></div>

                        <div class="course-desc"><%# Eval("Subtitle") %></div>

                        <div class="course-pills">
                            <span class="pill"><span class="pill-ic">📘</span><%# Eval("SubtopicCount") %> Subtopics</span>
                            <span class="pill"><span class="pill-ic">🧩</span><%# Eval("PracticeQuestionCount") %> Practice Questions</span>
                            <span class="pill"><span class="pill-ic">✅</span><%# Eval("SelfAssessmentCount") %> Self Assessment</span>
                        </div>

                        <asp:Panel runat="server" CssClass="row-progress"
                            Visible='<%# !(bool)Eval("IsLocked") && Convert.ToInt32(Eval("ProgressPercent")) > 0 %>'>
                            <div class="row-progress-bar">
                                <div class="row-progress-fill" style='<%# "width:" + Eval("ProgressPercent") + "%;" %>'></div>
                            </div>
                            <div class="row-progress-text"><%# Eval("ProgressPercent") %>%</div>
                        </asp:Panel>

                    </div>

                    <asp:Panel runat="server" CssClass="row-lock" Visible='<%# (bool)Eval("IsLocked") %>'>
                        <div class="lock-icon">🔒</div>
                        <div class="lock-text">Sign Up to Unlock</div>
                    </asp:Panel>

                </a>

            </ItemTemplate>
        </asp:Repeater>
    </div>

    <asp:Panel ID="pnlOverall" runat="server" Visible="true">
        <div class="overall-card">
            <div class="overall-head">
                <div class="overall-title">📊 Overall Learning Progress</div>
                <div class="overall-percent">
                    <asp:Literal ID="litOverallPercent" runat="server" />
                </div>
            </div>

            <div class="overall-bar">
                <div class="overall-fill" style='<%= "width:" + OverallProgressPercent + "%;" %>'></div>
            </div>

            <div class="overall-legend">
                <span><i class="dot done"></i> Completed</span>
                <span><i class="dot pend"></i> Pending</span>
            </div>
        </div>
    </asp:Panel>

</asp:Content>