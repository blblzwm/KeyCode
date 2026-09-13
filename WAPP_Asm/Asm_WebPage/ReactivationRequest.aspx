<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReactivationRequest.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.ReactivationRequest" EnableEventValidation="false" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="../Asm_StyleSheet/ReactivationRequestsStyle.css?v=admin-scroll-2" rel="stylesheet">
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <!-- PANEL 1: SUSPENDED USER -->
    <asp:Panel ID="pnlUser" runat="server" Visible="false">
        <div class="container-fluid" style="max-width:1400px;">

            <div class="appeal-card">
                <h2 class="appeal-title">Account Reactivation Appeal</h2>
                <p class="appeal-subtitle">
                    If your account has been suspended, you may submit a request
                    to the administration for review.
                </p>

                <asp:Panel ID="pnlAppealForm" runat="server">
                    <div class="mb-3">
                        <label class="form-label">User ID</label>
                        <asp:TextBox ID="txtUserID" runat="server"
                            CssClass="form-control"
                            ReadOnly="true" />
                    </div>

                    <div class="mb-4">
                        <label class="form-label">Reason for Reactivation</label>
                        <asp:TextBox ID="txtReason" runat="server"
                            CssClass="form-control"
                            TextMode="MultiLine"
                            Rows="5"
                            placeholder="Please explain why your account should be reactivated..."
                            MaxLength="2000" />
                        <asp:RequiredFieldValidator
                            ID="rfvReason" runat="server"
                            ControlToValidate="txtReason"
                            ErrorMessage="Please provide a reason."
                            CssClass="text-danger small"
                            Display="Dynamic" />
                    </div>

                    <div class="mb-4">
                        <label class="form-label">Supporting Document <span class="text-muted small">(Optional — PDF, PNG, JPEG or JPG only, max 5MB)</span></label>
                        <asp:FileUpload ID="fuProof" runat="server" CssClass="form-control" Accept=".pdf,.jpg,.jpeg,.png" />
                        <asp:Label ID="lblFileError" runat="server" CssClass="text-danger small" Visible="false" />
                    </div>

                    <asp:Button ID="btnSubmitAppeal" runat="server"
                        Text="Submit Appeal"
                        CssClass="btn-submit-appeal"
                        OnClick="btnSubmitAppeal_Click" />
                </asp:Panel>

            </div>
        </div>
    </asp:Panel>


    <!-- PANEL 2: ADMIN DASHBOARD -->
    <asp:Panel ID="pnlAdmin" runat="server" Visible="false">
        <div id="requestAdminPage" class="rr-admin"><div class="rr-heading"><h1 class="main-heading">Reactivation Requests</h1><div class="rr-tabs" role="group" aria-label="User category"><asp:LinkButton ID="btnStudents" runat="server"
                            CssClass="nav-link-custom active"
                            OnClick="btnStudents_Click">
                            Students
                        </asp:LinkButton>

                        <asp:LinkButton ID="btnTutors" runat="server"
                            CssClass="nav-link-custom"
                            OnClick="btnTutors_Click">
                            Tutors
                        </asp:LinkButton></div></div><div class="rr-fill"><div class="rr-fill">
                    <div class="card-main">
                        <div class="content-header">
                            <h3 class="content-title">
                                <asp:Label ID="lblCategoryTitle" runat="server" Text="Student" /> Reactivation Requests
                            </h3><div class="rr-search"><svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="10.5" cy="10.5" r="6.5"/><path d="m16 16 5 5"/></svg><asp:TextBox ID="txtRequestSearch" runat="server" CssClass="form-control" placeholder="Search request or user ID..." aria-label="Search request or user ID" AutoPostBack="true" OnTextChanged="RequestSearch_Changed" /></div>
                        </div>

                        <!-- Action buttons -->
                        <div class="action-bar">
                            <asp:Button ID="btnApprove" runat="server"
                                Text="Approve"
                                CssClass="btn-action btn-approve"
                                OnClick="btnApprove_Click"
                                CausesValidation="false"
                                Enabled="false"
                                OnClientClick="return confirm('Are you sure you want to approve this request?');" />

                            <asp:Button ID="btnReject" runat="server"
                                Text="Reject"
                                CssClass="btn-action btn-reject"
                                OnClick="btnReject_Click"
                                CausesValidation="false"
                                Enabled="false"
                                OnClientClick="return confirm('Are you sure you want to reject this request?');" />
                        </div>

                        <div class="rr-scroll" tabindex="0" role="region" aria-label="Scrollable requests table">
                            <asp:GridView ID="gvRequests" runat="server"
                                CssClass="custom-grid"
                                AutoGenerateColumns="false"
                                GridLines="None" BorderStyle="None" BorderWidth="0px" EmptyDataText="No requests found for this category or search."
                                DataKeyNames="requestID"
                                OnRowCreated="gvRequests_RowCreated"
                                OnRowCommand="gvRequests_RowCommand"
                                OnRowDataBound="gvRequests_RowDataBound">
                                <Columns>
                                    <asp:TemplateField HeaderText="Request ID">
                                        <ItemTemplate>#<%# Eval("requestID") %></ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="userID"
                                        HeaderText="User ID"
                                        ItemStyle-CssClass="fw-bold" />

                                    <asp:BoundField DataField="requestDate"
                                        HeaderText="Date Submitted"
                                        DataFormatString="{0:yyyy-MM-dd}" />

                                    <asp:TemplateField HeaderText="Status">
                                        <ItemTemplate>
                                            <asp:Label ID="lblStatus" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="reason"
                                        HeaderText="Reason" />

                                    <asp:TemplateField HeaderText="Proof">
                                        <ItemTemplate>
                                            <asp:HyperLink ID="hlAttachment" runat="server"
                                                NavigateUrl='<%# Eval("attachmentPath") != DBNull.Value ? ResolveUrl(Eval("attachmentPath").ToString()) : "" %>'
                                                Target="_blank"
                                                Visible='<%# Eval("attachmentPath") != DBNull.Value && Eval("attachmentPath").ToString() != "" %>'
                                                CssClass="link-proof">
                                                View
                                            </asp:HyperLink>
                                            <asp:Label ID="lblNoProof" runat="server"
                                                Text="—"
                                                Visible='<%# Eval("attachmentPath") == DBNull.Value || Eval("attachmentPath").ToString() == "" %>'
                                                CssClass="text-muted" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>

                        <!--Hidden fields to carry selected row data -->
                        <asp:HiddenField ID="hdnRequestId" runat="server" />
                        <asp:HiddenField ID="hdnUserId"    runat="server" />
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>
<script>
(function() {
    var root = document.getElementById("requestAdminPage");
    if (!root) return;
    document.documentElement.classList.add("rr-viewport");
    function fit() {
        var h = window.visualViewport ? window.visualViewport.height : window.innerHeight;
        root.style.height = Math.max(0, h - root.getBoundingClientRect().top - 16) + "px";
    }
    fit(); window.addEventListener("resize", fit); window.addEventListener("load", fit);
    if (window.visualViewport) window.visualViewport.addEventListener("resize", fit);
    if (document.fonts) document.fonts.ready.then(fit);
}());
</script>
</asp:Content>
