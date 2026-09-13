<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Forum.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.Forum" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Discussion Forum</title>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link href="../Asm_StyleSheet/ForumStyle.css?v=forum-icons-3" rel="stylesheet" />
    <script>
        // This page owns the Forum selection, including after full postbacks.
        document.addEventListener("DOMContentLoaded", function () {
            const links = document.querySelectorAll("#dashSidebar a.side-nav-item");
            const forumPath = new URL('<%= ResolveUrl("~/Asm_WebPage/Forum.aspx") %>', location.href).pathname.toLowerCase();
            const forumLink = Array.from(links).find(function (link) {
                return new URL(link.href, location.href).pathname.toLowerCase() === forumPath;
            });
            if (!forumLink) return;
            links.forEach(function (link) {
                const selected = link === forumLink;
                link.classList.toggle("active", selected);
                if (selected) link.setAttribute("aria-current", "page");
                else link.removeAttribute("aria-current");
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .forum-reply-child { margin-left:42px; border-left:3px solid #dbeafe; padding-left:12px; }
        .forum-ai-reply { background:#f5f3ff; border-radius:10px; padding:10px; }
        .badge-ai { background:#7c3aed; color:white; }
        .report-content { max-width:360px; white-space:normal; word-break:break-word; }
        .reply-context { font-size:.85rem; color:#6b7280; margin-bottom:6px; }
    </style>

    <div class="container-fluid" style="max-width:1200px;">

        <div class="forum-heading"><h1 class="main-heading">Discussion Forum</h1>                    <asp:Panel ID="pnlAdminReports" runat="server" Visible="false">
                        <button type="button" class="btn btn-outline-danger" onclick="showReportsModal()">
                            Review reports <asp:Label ID="lblPendingReportCount" runat="server" Text="0" CssClass="forum-report-count" />
                        </button>
                    </asp:Panel></div>

        <div class="forum-main">
<section id="announcementCarousel" class="forum-announcements" hidden aria-label="Announcements">
<div class="announcement-controls"><h2>Announcements</h2><div><button type="button" id="announcementPrev" aria-label="Previous announcement">←</button><span id="announcementPosition"></span><button type="button" id="announcementNext" aria-label="Next announcement">→</button><button type="button" id="announcementPause">Pause</button></div></div>
<div id="announcementSlides"></div>
</section>

            <div class="feed-header">
                <h3 class="feed-title">All Discussions</h3>
                <div style="display:flex; gap:10px; align-items:center;">
                    <div class="forum-search-field"><svg class="forum-ui-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><circle cx="10.5" cy="10.5" r="6.5"/><path d="m16 16 5 5"/></svg><input type="text" id="searchPost" aria-label="Search discussions by title" 
                        placeholder="Search by title..."
                        oninput="filterPosts(this.value)"
                        class="search-input" /></div>

                    <asp:Button ID="btnNewPost" runat="server"
                        CssClass="btn-new-post"
                        Text="+ New Post"
                        OnClientClick="$('#newPostModal').modal('show'); return false;" />
                </div>
            </div>

            <!-- No search results -->
            <div id="noSearchResults" style="display:none;" class="text-center py-4">
                <p class="text-muted" style="font-style:italic;">No posts found matching your search.</p>
            </div>

            <div id="postFeed">
                <asp:Repeater ID="rptPosts" runat="server" OnItemDataBound="rptPosts_ItemDataBound">
                    <ItemTemplate>
                        <div class='post-card <%# Convert.ToBoolean(Eval("is_announcement")) ? "announcement-post" : "" %>'>

                            <asp:Panel ID="pnlAnnouncementTag" runat="server"
                                Visible='<%# Convert.ToBoolean(Eval("is_announcement")) %>'
                                CssClass="announcement-tag">
                                Announcement
                            </asp:Panel>

                            <div class="post-header">

                                <%# GetAvatarHtml(Eval("fname"), Eval("lname"), Eval("role"), Eval("upload_profile")) %>

                                <div class="post-info">
                                    <div class="post-author">
                                        <%# Eval("username") %>
                                        <span class='<%# GetRoleBadgeClass(Eval("role").ToString()) %>'>
                                            <%# Eval("role") %>
                                        </span>
                                    </div>
                                    <div class="post-time"><%# GetRelativeTime(Eval("created_at")) %></div>
                                </div>

                                <button type="button"
                                    class="btn-tts"
                                    data-postid='<%# Eval("postID") %>'
                                    data-title='<%# EscapeForJs(Eval("title").ToString()) %>'
                                    data-content='<%# EscapeForJs(Eval("content").ToString()) %>'
                                    onclick="speakPostWithComments(this)">
                                    <svg class="forum-ui-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M11 5 6 9H3v6h3l5 4V5Z"/><path d="M15 8a6 6 0 0 1 0 8M18 5a10 10 0 0 1 0 14"/></svg> Read Aloud
                                </button>

                                <!-- Dots Menu -->
                                <div class="dots-menu-wrapper">
                                    <asp:Panel ID="pnlPostDots" runat="server" Visible="false">
                                        <button type="button" class="btn-dots" onclick="toggleMenu(this)">⋮</button>
                                        <div class="dots-dropdown">
                                            <asp:LinkButton ID="btnEditPost" runat="server"
                                                CssClass="dots-item"
                                                CommandName="EditPost"
                                                CommandArgument='<%# Eval("postID") %>'
                                                OnCommand="PostAction_Command"
                                                Visible="false">
                                                Edit
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnReportPost" runat="server"
                                                CssClass="dots-item"
                                                CommandName="ReportPost"
                                                CommandArgument='<%# Eval("postID") %>'
                                                OnCommand="PostAction_Command"
                                                Visible="false">
                                                Report
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDeletePost" runat="server"
                                                CssClass="dots-item dots-item-danger"
                                                CommandName="DeletePost"
                                                CommandArgument='<%# Eval("postID") %>'
                                                OnCommand="PostAction_Command"
                                                OnClientClick="return confirm('Are you sure you want to delete this post?');"
                                                Visible="false">
                                                Delete
                                            </asp:LinkButton>
                                        </div>
                                    </asp:Panel>
                                </div>
                            </div>

                            <div class="post-content">
                                <div class="post-title"><%# FormatForumText(Eval("title")) %></div>
                                <div class="post-body"><%# FormatForumText(Eval("content")) %></div>
                            </div>

                            <div class="forum-comment-total">
                                <asp:Label ID="lblCommentCount" runat="server" />
                            </div>

                            <!-- Like and comment actions -->
                            <div class="post-footer forum-post-actions">
                                <asp:LinkButton ID="btnLikePost" runat="server"
                                    CommandName="LikePost"
                                    CommandArgument='<%# Eval("postID") %>'
                                    OnCommand="Like_Command"
                                    CausesValidation="false"
                                    CssClass="forum-like" />

                                <asp:LinkButton ID="btnViewComments" runat="server"
                                    CssClass="forum-comment-action"
                                    CommandArgument='<%# Eval("postID") %>'
                                    OnClick="btnViewComments_Click"
                                    CausesValidation="false"
                                    ToolTip="View comments"
                                    aria-label="View comments">

                                    <svg viewBox="0 0 24 24"
                                         fill="none"
                                         stroke="currentColor"
                                         stroke-width="1.8"
                                         stroke-linecap="round"
                                         stroke-linejoin="round"
                                         aria-hidden="true"
                                         focusable="false">
                                        <path d="M21 11.5a8.4 8.4 0 0 1-.9 3.8
                                                 8.5 8.5 0 0 1-7.6 4.7
                                                 8.4 8.4 0 0 1-3.8-.9L3 21l1.9-5.7
                                                 a8.4 8.4 0 0 1-.9-3.8
                                                 8.5 8.5 0 0 1 4.7-7.6
                                                 8.4 8.4 0 0 1 3.8-.9h.5
                                                 a8.5 8.5 0 0 1 8 8v.5z" />
                                    </svg>
                                </asp:LinkButton>
                            </div>

                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Panel ID="pnlNoPosts" runat="server" Visible="false" CssClass="text-center py-5">
                    <p class="text-muted" style="font-style:italic;">No posts yet. Be the first to start a discussion!</p>
                </asp:Panel>
            </div>
        </div>

    </div>


    <!-- MODAL: VIEW THREAD & COMMENTS -->
    <div class="modal fade" id="threadModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">

                <div class="modal-header">
                    <h5 class="modal-title thread-modal-title">
                        <asp:Label ID="lblThreadTitle" runat="server"></asp:Label>
                    </h5>
                    <div class="thread-modal-header-actions">
                        <span class="forum-summary-control"><asp:Button ID="btnSummarise" runat="server"
                            Text="✨Summarise"
                            CssClass="btn-summarise"
                            OnClick="btnSummarise_Click"
                            CausesValidation="false" /></span>
                        <button type="button" id="btnModalTts" class="btn-tts" onclick="speakModalThread()">
                            <svg class="forum-ui-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M11 5 6 9H3v6h3l5 4V5Z"/><path d="M15 8a6 6 0 0 1 0 8M18 5a10 10 0 0 1 0 14"/></svg> Read Aloud
                        </button>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                </div>

                <div class="modal-body modal-body-white">

                    <asp:HiddenField ID="hfCurrentPostID" runat="server" />
                    <asp:Literal ID="litCommentData" runat="server"></asp:Literal>

                    <asp:Panel ID="pnlSummary" runat="server" Visible="false" CssClass="summary-box">
                        <div class="summary-header">✨ AI Summary</div>
                        <asp:Label ID="lblSummary" runat="server" CssClass="summary-text" />
                    </asp:Panel>

                    <!-- Original Post -->
                    <asp:Panel ID="pnlOriginalPost" runat="server" CssClass="mb-4 original-post-panel">

                        <div class="orig-post-author-row">
                            <asp:Literal ID="litOrigPostAvatar" runat="server" />
                            <div>
                                <div class="orig-post-author-name">
                                    <asp:Label ID="lblOrigPostAuthor" runat="server"></asp:Label>
                                    <asp:Label ID="lblOrigPostRole"   runat="server"></asp:Label>
                                </div>
                                <div class="orig-post-time">
                                    <asp:Label ID="lblOrigPostTime" runat="server"></asp:Label>
                                </div>
                            </div>
                        </div>

                        <p class="orig-post-content">
                            <asp:Label ID="lblOrigPostContent" runat="server"></asp:Label>
                        </p>
                    </asp:Panel>

                    <h6 class="comments-heading">Comments</h6>

                    <asp:Repeater ID="rptComments" runat="server" OnItemDataBound="rptComments_ItemDataBound">
                        <ItemTemplate>
                            <div class='reply-item <%# Convert.ToBoolean(Eval("is_ai")) ? "forum-ai-reply" : "" %> <%# string.IsNullOrEmpty(Eval("parentCommentID").ToString()) ? "" : "forum-reply-child" %>'>

                                <%# GetAvatarHtml(Eval("fname"), Eval("lname"), Eval("role"), Eval("upload_profile"), "32px", "0.8rem") %>

                                <div class="reply-item-body">
                                    <div class="reply-item-header">
                                        <div class="reply-item-meta">
                                            <%# Eval("username") %>
                                            <span class='<%# GetRoleBadgeClass(Eval("role").ToString()) %>'
                                                  style="font-size:0.7rem;">
                                                <%# Eval("role") %>
                                            </span>
                                            <span class="reply-item-time">
                                                <%# GetRelativeTime(Eval("created_at")) %>
                                            </span>
                                        </div>

                                        <!-- ⋮ Dots Menu for comments -->
                                        <div class="dots-menu-wrapper">
                                            <asp:Panel ID="pnlCommentDots" runat="server" Visible="false">
                                                <button type="button" class="btn-dots" onclick="toggleMenu(this)">⋮</button>
                                                <div class="dots-dropdown">
                                                    <asp:LinkButton ID="btnEditComment" runat="server"
                                                        CssClass="dots-item"
                                                        CommandName="EditComment"
                                                        CommandArgument='<%# Eval("commentID") %>'
                                                        OnCommand="CommentAction_Command"
                                                        Visible="false">
                                                        Edit
                                                    </asp:LinkButton>
                                                    
                                                    <asp:LinkButton ID="btnReportComment" runat="server"
                                                        CssClass="dots-item"
                                                        CommandName="ReportComment"
                                                        CommandArgument='<%# Eval("commentID") %>'
                                                        OnCommand="CommentAction_Command"
                                                        Visible="false">Report</asp:LinkButton>
                                                    <asp:LinkButton ID="btnDeleteComment" runat="server"
                                                        CssClass="dots-item dots-item-danger"
                                                        CommandName="DeleteComment"
                                                        CommandArgument='<%# Eval("commentID") %>'
                                                        OnCommand="CommentAction_Command"
                                                        OnClientClick="return confirm('Are you sure you want to delete this comment?');"
                                                        Visible="false">
                                                        Delete
                                                    </asp:LinkButton>
                                                </div>
                                            </asp:Panel>
                                        </div>
                                    </div>
                                    <div class="reply-context" style='<%# string.IsNullOrEmpty(Eval("parent_username").ToString()) ? "display:none" : "" %>'>
                                        Replying to <%# Server.HtmlEncode(Eval("parent_username").ToString()) %>
                                    </div>
                                    <div class="reply-bubble"><%# FormatForumText(Eval("content")) %></div><asp:LinkButton ID="btnLikeComment" runat="server" CommandName="LikeComment" CommandArgument='<%# Eval("commentID") %>' OnCommand="Like_Command" CausesValidation="false" CssClass="forum-like" /><asp:LinkButton ID="btnReplyComment" runat="server"
                                                        CssClass="forum-reply-action" CausesValidation="false"
                                                        CommandName="ReplyComment"
                                                        CommandArgument='<%# Eval("commentID") %>'
                                                        OnCommand="CommentAction_Command"
                                                        Visible="false">Reply</asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Panel ID="pnlNoComments" runat="server" Visible="false" CssClass="text-center text-muted py-3">
                        <em>No comments yet. Be the first to comment!</em>
                    </asp:Panel>

                </div>

                <div class="modal-footer thread-modal-footer">

                    <asp:RequiredFieldValidator ID="rfvComment" runat="server"
                        ControlToValidate="txtComment"
                        ErrorMessage="Comment cannot be empty"
                        CssClass="text-danger small"
                        Display="Dynamic"
                        ValidationGroup="PostComment">
                    </asp:RequiredFieldValidator>

                    <div id="commentLengthError" style="display:none; color:red;">
                        Comment must not exceed 1000 characters.
                    </div>

                    <div id="commentProfanityError" role="alert" aria-live="assertive" class="alert-box alert-error w-100" style="display:none; margin-bottom:8px;">
                        Please remove inappropriate language before posting.
                    </div>

                    <asp:HiddenField ID="hfReplyToCommentID" runat="server" />
                    <asp:Panel ID="pnlReplyContext" runat="server" Visible="false" CssClass="reply-context">
                        <asp:Label ID="lblReplyingTo" runat="server" />
                        <asp:LinkButton ID="btnCancelReply" runat="server" Text="Cancel reply"
                            CausesValidation="false" OnClick="btnCancelReply_Click" />
                    </asp:Panel>
                    <asp:Panel ID="pnlCommentBox" runat="server" CssClass="input-group w-100" Visible="false">
                        <asp:TextBox ID="txtComment" runat="server"
                            CssClass="form-control comment-input"
                            placeholder="Write a comment, or tag @AI to ask KeyCode AI...">
                        </asp:TextBox>

                        <asp:Button ID="btnPostComment" runat="server"
                            CssClass="btn btn-outline-publish comment-post-btn"
                            Text="Post"
                            OnClick="btnPostComment_Click"
                            OnClientClick="if (!validateForumComment()) return false;"
                            ValidationGroup="PostComment" />
                    </asp:Panel>

                </div>
            </div>
        </div>
    </div>


    <!-- REPORT CONTENT -->
    <div class="modal fade" id="reportModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title">Report content</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button></div>
            <div class="modal-body">
                <asp:HiddenField ID="hfReportTargetType" runat="server" />
                <asp:HiddenField ID="hfReportTargetID" runat="server" />
                <label class="form-label">Reason</label>
                <asp:DropDownList ID="ddlReportReason" runat="server" CssClass="form-select mb-3">
                    <asp:ListItem Value="Harassment">Harassment or bullying</asp:ListItem>
                    <asp:ListItem Value="Hate">Hate or discrimination</asp:ListItem>
                    <asp:ListItem Value="Threat">Threat or violence</asp:ListItem>
                    <asp:ListItem Value="Spam">Spam or misleading content</asp:ListItem>
                    <asp:ListItem Value="Other">Other</asp:ListItem>
                </asp:DropDownList>
                <label class="form-label">Details (optional)</label>
                <asp:TextBox ID="txtReportDetails" runat="server" TextMode="MultiLine"
                    Rows="3" MaxLength="500" CssClass="form-control" />
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button>
                <asp:Button ID="btnSubmitReport" runat="server" Text="Submit report"
                    CssClass="btn btn-danger" CausesValidation="false" OnClick="btnSubmitReport_Click" />
            </div>
        </div></div>
    </div>

    <!-- ADMIN REPORT REVIEW -->
    <div class="modal fade" id="reportsModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-scrollable"><div class="modal-content">
            <div class="modal-header"><h5 class="modal-title">Pending reports</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button></div>
            <div class="modal-body">
                <div class="table-responsive"><table class="table table-striped align-middle">
                    <thead><tr><th>Type</th><th>Reason</th><th>Content</th><th>Reporter</th><th>Time</th><th>Action</th></tr></thead>
                    <tbody><asp:Repeater ID="rptReports" runat="server"><ItemTemplate><tr>
                        <td><%# Server.HtmlEncode(Eval("target_type").ToString()) %></td>
                        <td><strong><%# Server.HtmlEncode(Eval("reason").ToString()) %></strong><br />
                            <%# FormatForumText(Eval("details")) %></td>
                        <td class="report-content"><%# FormatForumText(Eval("reported_content")) %></td>
                        <td><%# Server.HtmlEncode(Eval("reporter").ToString()) %></td>
                        <td><%# GetRelativeTime(Eval("created_at")) %></td>
                        <td class="text-nowrap">
                            <asp:LinkButton ID="btnDismissReport" runat="server" Text="Dismiss"
                                CssClass="btn btn-sm btn-outline-secondary" CommandName="DismissReport"
                                CommandArgument='<%# Eval("reportID") %>' OnCommand="ReportAction_Command" CausesValidation="false" />
                            <asp:LinkButton ID="btnDeleteReported" runat="server" Text="Delete content"
                                CssClass="btn btn-sm btn-danger" CommandName="DeleteReportedContent"
                                CommandArgument='<%# Eval("reportID") %>' OnCommand="ReportAction_Command"
                                CausesValidation="false" OnClientClick="return confirm('Delete this reported content?');" />
                        </td>
                    </tr></ItemTemplate></asp:Repeater></tbody>
                </table></div>
            </div>
        </div></div>
    </div>

    <!-- CREATE NEW POST -->
    <div class="modal fade" id="newPostModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title modal-title-bold">Create New Post</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label modal-form-label">Subject / Title</label>
                        <asp:TextBox ID="txtPostTitle" runat="server"
                            CssClass="form-control"
                            placeholder="What is this discussion about?"
                            MaxLength="50">
                        </asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvPostTitle" runat="server"
                            ControlToValidate="txtPostTitle"
                            ErrorMessage="Title is required"
                            CssClass="text-danger small"
                            Display="Dynamic"
                            ValidationGroup="NewPost">
                        </asp:RequiredFieldValidator>
                    </div>
                    <div class="mb-3">
                        <label class="form-label modal-form-label">Message</label>
                        <asp:TextBox ID="txtPostContent" runat="server"
                            TextMode="MultiLine"
                            Rows="5"
                            CssClass="form-control"
                            placeholder="Type your message here...">
                        </asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvPostContent" runat="server"
                            ControlToValidate="txtPostContent"
                            ErrorMessage="Message is required"
                            CssClass="text-danger small"
                            Display="Dynamic"
                            ValidationGroup="NewPost">
                        </asp:RequiredFieldValidator>
                    </div>

                    <div id="profanityError" class="alert-box alert-error" style="display:none;">
                        ⚠️ Your post contains inappropriate language. Please revise it before submitting.
                    </div>

                    <asp:Panel ID="pnlAnnouncementOption" runat="server" Visible="false" CssClass="mb-3">
                        <div class="form-check announce-check-row">
                            <asp:CheckBox ID="chkIsAnnouncement" runat="server" />
                            <asp:Label ID="lblMarkAnnouncement" runat="server" CssClass="check-announce" Text="📢 Mark as Announcement" />
                        </div>
                    </asp:Panel>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button>
                    <asp:Button ID="btnPublishPost" runat="server"
                        CssClass="btn btn-outline-publish"
                        Text="Publish Post"
                        OnClick="btnPublishPost_Click"
                        ValidationGroup="NewPost" />
                </div>
            </div>
        </div>
    </div>


    <!-- EDIT POST -->
    <div class="modal fade" id="editPostModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title modal-title-bold">Edit Post</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfEditPostID" runat="server" />
                    <div class="mb-3">
                        <label class="form-label modal-form-label">Subject / Title</label>
                        <asp:TextBox ID="txtEditPostTitle" runat="server"
                            CssClass="form-control"
                            MaxLength="100">
                        </asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEditPostTitle" runat="server"
                            ControlToValidate="txtEditPostTitle"
                            ErrorMessage="Title is required"
                            CssClass="text-danger small"
                            Display="Dynamic"
                            ValidationGroup="EditPost">
                        </asp:RequiredFieldValidator>
                    </div>
                    <div class="mb-3">
                        <label class="form-label modal-form-label">Message</label>
                        <asp:TextBox ID="txtEditPostContent" runat="server"
                            TextMode="MultiLine"
                            Rows="5"
                            CssClass="form-control">
                        </asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEditPostContent" runat="server"
                            ControlToValidate="txtEditPostContent"
                            ErrorMessage="Message is required"
                            CssClass="text-danger small"
                            Display="Dynamic"
                            ValidationGroup="EditPost">
                        </asp:RequiredFieldValidator>
                    </div>

                    <div id="editPostProfanityError" class="alert-box alert-error" style="display:none;">
                        ⚠️ Your post contains inappropriate language. Please revise it before submitting.
                    </div>

                    <asp:Panel ID="pnlEditAnnouncementOption" runat="server" Visible="false" CssClass="mb-3">
                        <div class="form-check announce-check-row">
                            <asp:CheckBox ID="chkisEditAnnouncement" runat="server" />
                            <asp:Label ID="lblEditMarkAnnouncement" runat="server" CssClass="check-announce" Text="📢 Mark as Announcement" />
                        </div>
                    </asp:Panel>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button>
                    <asp:Button ID="btnSaveEditPost" runat="server"
                        CssClass="btn btn-outline-publish"
                        Text="Save Changes"
                        OnClick="btnSaveEditPost_Click"
                        ValidationGroup="EditPost" />
                </div>
            </div>
        </div>
    </div>


    <!-- EDIT COMMENT -->
    <div class="modal fade" id="editCommentModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title modal-title-bold">Edit Comment</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfEditCommentID"     runat="server" />
                    <asp:HiddenField ID="hfEditCommentPostID" runat="server" />
                    <div class="mb-3">
                        <label class="form-label modal-form-label">Comment</label>
                        <asp:TextBox ID="txtEditComment" runat="server"
                            TextMode="MultiLine"
                            Rows="4"
                            CssClass="form-control">
                        </asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEditComment" runat="server"
                            ControlToValidate="txtEditComment"
                            ErrorMessage="Comment cannot be empty"
                            CssClass="text-danger small"
                            Display="Dynamic"
                            ValidationGroup="EditComment">
                        </asp:RequiredFieldValidator>

                        <div id="editCommentLengthError" style="display:none; color:red;">
                            Comment must not exceed 1000 characters.
                        </div>

                        <div id="editCommentProfanityError" class="alert-box alert-error" style="display:none;">
                            Please remove inappropriate language before posting. Please revise it before submitting.
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button>
                    <asp:Button ID="btnSaveEditComment" runat="server"
                        CssClass="btn btn-outline-publish"
                        Text="Save Changes"
                        OnClick="btnSaveEditComment_Click"
                        ValidationGroup="EditComment" />
                </div>
            </div>
        </div>
    </div>

    <!--javascript-->
    <script type="text/javascript">

        function validateForumComment() {
            var input = document.getElementById('<%= txtComment.ClientID %>');
            var error = document.getElementById('commentProfanityError');
            var blocked = /\b(shit|fuck|fucking|bullshit)\b/i.test(input.value);
            error.style.display = blocked ? 'block' : 'none';
            input.setAttribute('aria-invalid', blocked ? 'true' : 'false');
            if (blocked) {
                input.setAttribute('aria-describedby', 'commentProfanityError');
                input.focus();
            } else input.removeAttribute('aria-describedby');
            return !blocked;
        }

        function filterPosts(query) {
            var cards = document.querySelectorAll('#postFeed .post-card');
            var noResult = document.getElementById('noSearchResults');
            var q = query.toLowerCase().trim();
            var visibleCount = 0;

            cards.forEach(function (card) {
                var titleEl = card.querySelector('.post-title');
                if (!titleEl) return;
                var title = titleEl.innerText.toLowerCase();
                if (q === '' || title.includes(q)) {
                    card.style.display = 'block';
                    visibleCount++;
                } else {
                    card.style.display = 'none';
                }
            });

            noResult.style.display = (visibleCount === 0 && q !== '') ? 'block' : 'none';
        }

        function showThreadModal() {
            var el = document.getElementById('threadModal');
            if (!el) return;
            var existing = bootstrap.Modal.getInstance(el);
            if (existing) { existing.show(); return; }
            new bootstrap.Modal(el).show();
        }

        function hideNewPostModal() {
            var el = document.getElementById('newPostModal');
            var m = bootstrap.Modal.getInstance(el);
            if (m) m.hide();
        }

        function showEditPostModal() {
            new bootstrap.Modal(document.getElementById('editPostModal')).show();
        }

        function hideEditPostModal() {
            var el = document.getElementById('editPostModal');
            var m = bootstrap.Modal.getInstance(el);
            if (m) m.hide();
        }

        function showEditCommentModal() {
            new bootstrap.Modal(document.getElementById('editCommentModal')).show();
        }

        function hideEditCommentModal() {
            var el = document.getElementById('editCommentModal');
            var m = bootstrap.Modal.getInstance(el);
            if (m) m.hide();
        }

        // DOTS MENU TOGGLE
        function toggleMenu(btn) {
            var dropdown = btn.nextElementSibling;
            var isOpen = dropdown.classList.contains('open');
            closeAllMenus();
            if (!isOpen) {
                dropdown.classList.add('open');
                btn.classList.add('active');
            }
        }

        function closeAllMenus() {
            document.querySelectorAll('.dots-dropdown.open').forEach(function (d) {
                d.classList.remove('open');
            });
            document.querySelectorAll('.btn-dots.active').forEach(function (b) {
                b.classList.remove('active');
            });
        }

        document.addEventListener('click', function (e) {
            if (!e.target.closest('.dots-menu-wrapper'))
                closeAllMenus();
        });

        // TEXT TO SPEECH
        var currentBtn = null;
        var currentUtterance = null;

        function speakQueue(texts, btn) {
            if (!texts || texts.length === 0) return;

            if (window.speechSynthesis.speaking) {
                window.speechSynthesis.cancel();
                if (currentBtn) resetBtn(currentBtn);
            }

            currentBtn = btn;
            btn.innerHTML = '<svg class="forum-ui-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><rect x="6" y="6" width="12" height="12" rx="2"/></svg> Stop';
            btn.classList.add('btn-tts-speaking');

            function speakNext(index) {
                if (index >= texts.length) {
                    resetBtn(btn);
                    currentBtn = null;
                    return;
                }
                var u = new SpeechSynthesisUtterance(texts[index]);
                u.lang = 'en-US';
                u.rate = 0.95;
                u.pitch = 1;
                u.volume = 1;
                u.onend = function () { speakNext(index + 1); };
                u.onerror = function () { resetBtn(btn); currentBtn = null; };
                currentUtterance = u;
                window.speechSynthesis.speak(u);
            }

            speakNext(0);
        }

        function speakPostWithComments(btn) {
            if (currentBtn === btn && window.speechSynthesis.speaking) {
                window.speechSynthesis.cancel();
                resetBtn(btn);
                currentBtn = null;
                return;
            }

            var postId = btn.getAttribute('data-postid');
            var title = btn.getAttribute('data-title');
            var content = btn.getAttribute('data-content');
            var texts = ['Post. ' + title + '. ' + content];

            var hfPostId = document.getElementById('<%= hfCurrentPostID.ClientID %>');
            var store = document.getElementById('commentDataStore');

            if (store && hfPostId && hfPostId.value === postId) {
                var count = parseInt(store.getAttribute('data-count') || '0');
                if (count === 0) {
                    texts.push('No comments on this post yet.');
                } else {
                    for (var i = 1; i <= count; i++) {
                        var author = store.getAttribute('data-comment-' + i + '-author') || '';
                        var comment = store.getAttribute('data-comment-' + i + '-content') || '';
                        texts.push('Comment ' + i + ' by ' + author + '. ' + comment);
                    }
                }
            } else {
                texts.push('Open the thread to also hear the comments.');
            }

            speakQueue(texts, btn);
        }

        function speakModalThread() {
            var btn = document.getElementById('btnModalTts');

            if (currentBtn === btn && window.speechSynthesis.speaking) {
                window.speechSynthesis.cancel();
                resetBtn(btn);
                currentBtn = null;
                return;
            }

            var texts = [];
            var titleEl = document.getElementById('<%= lblThreadTitle.ClientID %>');
            var contentEl = document.getElementById('<%= lblOrigPostContent.ClientID %>');

            if (titleEl && contentEl)
                texts.push('Post. ' + titleEl.innerText + '. ' + contentEl.innerText);

            var store = document.getElementById('commentDataStore');
            if (store) {
                var count = parseInt(store.getAttribute('data-count') || '0');
                if (count === 0) {
                    texts.push('No comments on this post yet.');
                } else {
                    for (var i = 1; i <= count; i++) {
                        var author = store.getAttribute('data-comment-' + i + '-author') || '';
                        var comment = store.getAttribute('data-comment-' + i + '-content') || '';
                        texts.push('Comment ' + i + ' by ' + author + '. ' + comment);
                    }
                }
            }

            speakQueue(texts, btn);
        }

        function resetBtn(btn) {
            if (!btn) return;
            btn.innerHTML = '<svg class="forum-ui-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M11 5 6 9H3v6h3l5 4V5Z"/><path d="M15 8a6 6 0 0 1 0 8M18 5a10 10 0 0 1 0 14"/></svg> Read Aloud';
            btn.classList.remove('btn-tts-speaking');
        }

        document.addEventListener('hidden.bs.modal', function () {
            if (window.speechSynthesis.speaking) {
                window.speechSynthesis.cancel();
                if (currentBtn) resetBtn(currentBtn);
                currentBtn = null;
            }
        });

        window.addEventListener('beforeunload', function () {
            window.speechSynthesis.cancel();
        });


        function showReportModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('reportModal')).show();
        }
        function hideReportModal() {
            var modal = bootstrap.Modal.getInstance(document.getElementById('reportModal'));
            if (modal) modal.hide();
        }
        function showReportsModal() {
            bootstrap.Modal.getOrCreateInstance(document.getElementById('reportsModal')).show();
        }

    </script>
<script>
    (function () {
        function init() {
            var box = document.getElementById("announcementCarousel"), slides = document.getElementById("announcementSlides");
            if (!box || box.dataset.ready) return; box.dataset.ready = "1";
            var cards = Array.from(document.querySelectorAll("#postFeed .announcement-post"));
            if (!cards.length) return;
            cards.forEach(function (card) { slides.appendChild(card); });
            box.hidden = false;
            var index = 0, paused = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
            var prev = document.getElementById("announcementPrev"), next = document.getElementById("announcementNext"), pause = document.getElementById("announcementPause");
            function show(n) { index = (n + cards.length) % cards.length; cards.forEach(function (card, i) { card.hidden = i !== index; }); document.getElementById("announcementPosition").textContent = (index + 1) + " / " + cards.length; }
            function label() { pause.textContent = paused ? "Play" : "Pause"; pause.setAttribute("aria-pressed", String(paused)); }
            prev.onclick = function () { paused = true; label(); show(index - 1); };
            next.onclick = function () { paused = true; label(); show(index + 1); };
            pause.onclick = function () { paused = !paused; label(); };
            box.addEventListener("focusin", function () { paused = true; label(); });
            var hover = false; box.addEventListener("mouseenter", function () { hover = true; }); box.addEventListener("mouseleave", function () { hover = false; });
            [prev, next, pause].forEach(function (b) { b.hidden = cards.length < 2; });
            label(); show(0);
            var timer = setInterval(function () { if (!paused && !hover && !document.hidden && !document.querySelector(".modal.show")) show(index + 1); }, 7000);
            window.addEventListener("pagehide", function () { clearInterval(timer); }, { once: true });
        }
        if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init); else init();
    }());
</script>
</asp:Content>
