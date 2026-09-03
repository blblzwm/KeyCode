<%@ Page Title="Tutor Dashboard" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="TutorDashboard.aspx.cs"
    Inherits="WAPP_Asm.Asm_WebPage.TutorDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <link href="<%= ResolveUrl("~/Asm_StyleSheet/TutorDashboardStyle.css?v=" + DateTime.Now.Ticks) %>" rel="stylesheet" />

    <div class="dash-grid">
        <div class="dash-left">
            <h1 class="dash-title">
                <asp:Literal ID="litWelcome" runat="server" />
            </h1>

            <div class="dash-sub">
                <asp:Literal ID="litSubWelcome" runat="server" />
            </div>
        </div>

        <div class="dash-right">

            <a class="dash-btn primary"
               href="<%= ResolveUrl("~/Asm_WebPage/TutorUpload.aspx") %>">
                + Upload
            </a>

            <asp:HyperLink ID="lnkRecentlyDeleted" runat="server"
                CssClass="dash-btn outline danger"
                NavigateUrl="~/Asm_WebPage/RecentlyDeleted.aspx"
                Text="🗑 Recently Deleted" />

            <asp:Panel ID="pnlRecentlyDeleted" runat="server" Visible="false" CssClass="rd-pop">
                <div class="rd-head">
                    <div class="rd-title">Recently Deleted</div>
                    <asp:LinkButton ID="btnCloseDeleted"
                        runat="server"
                        CssClass="rd-close"
                        OnClick="btnCloseDeleted_Click">
                        ✕
                    </asp:LinkButton>
                </div>

                <div class="rd-sub">Only subtopics deleted by you can be restored.</div>

                <asp:GridView ID="gvDeleted"
                    runat="server"
                    AutoGenerateColumns="false"
                    GridLines="None"
                    CssClass="rd-table"
                    OnRowCommand="gvDeleted_RowCommand"
                    EmptyDataText="No recently deleted subtopics.">
                    <Columns>
                        <asp:BoundField HeaderText="Subtopic ID" DataField="subtopicID" />
                        <asp:BoundField HeaderText="Title" DataField="title" />
                        <asp:BoundField HeaderText="Deleted At" DataField="deleted_at" DataFormatString="{0:yyyy-MM-dd HH:mm}" />

                        <asp:TemplateField HeaderText="">
                            <ItemTemplate>
                                <asp:LinkButton runat="server"
                                    Text="Restore"
                                    CssClass="rd-restore"
                                    CommandName="RESTORE"
                                    CommandArgument='<%# Eval("subtopicID") %>'
                                    OnClientClick="return confirm('Restore this subtopic?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </asp:Panel>

        </div>
    </div>

    <div class="course-list">
        <asp:Repeater ID="rptTutorChapters" runat="server">
            <ItemTemplate>

                <a class="course-row" href='<%# Eval("Link") %>'>

                    <div class="course-thumb">
                        <img class="thumb-img"
                             src="<%# ResolveUrl(Eval("ImageUrl").ToString()) %>"
                             alt='<%# Eval("Title") + " cover image" %>'
                             onerror="this.style.display='none'; this.nextElementSibling.style.display='block';" />

                        <div class="thumb-grad" style="display:none;"></div>
                    </div>
 
                    <div class="course-info">
                        <div class="course-name"><%# Eval("Title") %></div>
                        <div class="course-desc"><%# Eval("Subtitle") %></div>

                        <div class="course-pills">
                            <span class="pill"><span class="pill-ic">📘</span><%# Eval("SubtopicCount") %> Subtopics</span>
                            <span class="pill"><span class="pill-ic">👥</span><%# Eval("UniqueLearners") %> Learners</span>
                            <span class="pill"><span class="pill-ic">✅</span><%# Eval("TotalCompletions") %> Completions</span>
                        </div>

                        <div class="stats-note">Stats are based on subtopics created by you.</div>
                    </div>

                </a>

            </ItemTemplate>
        </asp:Repeater>
    </div>

    <asp:Panel ID="pnlEmpty" runat="server"
        Visible="false"
        CssClass="empty-card">
        No chapters found. Create a subtopic first.
    </asp:Panel>

</asp:Content>