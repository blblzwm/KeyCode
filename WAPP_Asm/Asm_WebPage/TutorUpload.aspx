<%@ Page Title="Upload" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="TutorUpload.aspx.cs"
    Inherits="WAPP_Asm.Asm_WebPage.TutorUpload"
    ValidateRequest="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <link href="<%= ResolveUrl("~/Asm_StyleSheet/TutorDashboardStyle.css?v=" + DateTime.Now.Ticks) %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Asm_StyleSheet/LearningMaterialStyle.css?v=" + DateTime.Now.Ticks) %>" rel="stylesheet" />

    <div class="tu-upload-topbar">
        <a class="tu-dashboard-btn" href="<%= ResolveUrl("~/Asm_WebPage/TutorDashboard.aspx") %>">
            << Back to Dashboard
        </a>

        <div class="tu-tabbar">
            <asp:LinkButton ID="btnTabMaterial" runat="server"
                CssClass="tu-tab"
                CausesValidation="false" OnClick="btnTabMaterial_Click">
                📘 Learning Material
            </asp:LinkButton>

            <asp:LinkButton ID="btnTabPQ" runat="server"
                CssClass="tu-tab"
                CausesValidation="false" OnClick="btnTabPQ_Click">
                ✎ Practice Question
            </asp:LinkButton>

            <asp:LinkButton ID="btnTabAss" runat="server"
                CssClass="tu-tab"
                CausesValidation="false" OnClick="btnTabAss_Click">
                🧠 Self-Assessment
            </asp:LinkButton>
        </div>
    </div>

    <div id="toast" class="toast" style="display:none;">
        <span id="toastText"></span>
    </div>

    <script>
        let toastTimer = null;

        function showToast(msg, ok) {
            var t = document.getElementById("toast");
            var txt = document.getElementById("toastText");
            if (!t || !txt) return;

            txt.textContent = msg || "";
            t.className = ok ? "toast ok" : "toast bad";
            t.style.display = "flex";

            if (toastTimer) clearTimeout(toastTimer);
            toastTimer = setTimeout(function () {
                t.style.display = "none";
            }, 2600);
        }
    </script>

    <asp:Panel ID="pnlMaterial" runat="server" Visible="true">

        <asp:Panel ID="pnlMatCreate" runat="server" Visible="true">
            <div class="overall-card" style="margin-top:14px;">
                <div class="overall-head">
                    <div class="overall-title tu-section-title">🧾 Upload Learning Material (Subtopic)</div>
                </div>

                <div class="tu-grid">
                    <div class="tu-label">Chapter</div>
                    <div>
                        <asp:DropDownList ID="ddlChapter" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvChapter" runat="server"
                            ControlToValidate="ddlChapter"
                            InitialValue=""
                            ValidationGroup="VG_MAT"
                            ErrorMessage="Please select a chapter."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Subtopic Title</div>
                    <div>
                        <asp:TextBox ID="txtTitle" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvTitle" runat="server"
                            ControlToValidate="txtTitle"
                            ValidationGroup="VG_MAT"
                            ErrorMessage="Title is required."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Content</div>

                    <asp:TextBox ID="txtContent" runat="server" ClientIDMode="Static"
                        TextMode="MultiLine" CssClass="tu-editor" Style="min-height:320px;" />
                </div>

                <div class="tu-actions">
                    <asp:Button ID="btnCreateSubtopic" runat="server"
                        Text="Create Subtopic"
                        CssClass="dash-btn primary"
                        ValidationGroup="VG_MAT"
                        OnClientClick="if(!Page_ClientValidate('VG_MAT')) return false; return confirm('Are you sure you want to upload this learning material?');"
                        OnClick="btnCreateSubtopic_Click" />

                    <asp:Button ID="btnCancelMat" runat="server"
                        Text="Cancel"
                        CssClass="dash-btn ghost"
                        CausesValidation="false"
                        OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>

    </asp:Panel>

    <asp:Panel ID="pnlPQ" runat="server" Visible="false">

        <asp:Panel ID="pnlPQCreate" runat="server" Visible="true">
            <div class="overall-card" style="margin-top:14px;">
                <div class="overall-head">
                    <div class="overall-title tu-section-title">✎ Upload Practice Question</div>
                </div>

                <div class="tu-grid">
                    <div class="tu-label">Chapter</div>
                    <div>
                        <asp:DropDownList ID="ddlPQChapter" runat="server"
                            CssClass="tu-input" AutoPostBack="true" CausesValidation="false"
                            OnSelectedIndexChanged="ddlPQChapter_SelectedIndexChanged" />
                        <asp:RequiredFieldValidator ID="rfvPQChapter" runat="server"
                            ControlToValidate="ddlPQChapter"
                            InitialValue=""
                            ValidationGroup="VG_PQ"
                            ErrorMessage="Please select a chapter."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Subtopic</div>
                    <div>
                        <asp:DropDownList ID="ddlPQSubtopic" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvPQSubtopic" runat="server"
                            ControlToValidate="ddlPQSubtopic"
                            InitialValue=""
                            ValidationGroup="VG_PQ"
                            ErrorMessage="Please select a subtopic."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Question</div>
                    <div>
                        <asp:TextBox ID="txtPQ_Q" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvPQ_Q" runat="server"
                            ControlToValidate="txtPQ_Q" ValidationGroup="VG_PQ"
                            ErrorMessage="Question is required."
                            Display="Dynamic" CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Option A</div>
                    <div>
                        <asp:TextBox ID="txtPQ_A" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvPQ_A" runat="server"
                            ControlToValidate="txtPQ_A"
                            ValidationGroup="VG_PQ"
                            ErrorMessage="Option A is required."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Option B</div>
                    <div>
                        <asp:TextBox ID="txtPQ_B" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvPQ_B" runat="server"
                            ControlToValidate="txtPQ_B"
                            ValidationGroup="VG_PQ"
                            ErrorMessage="Option B is required."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Option C</div>
                    <div>
                        <asp:TextBox ID="txtPQ_C" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvPQ_C" runat="server"
                            ControlToValidate="txtPQ_C"
                            ValidationGroup="VG_PQ"
                            ErrorMessage="Option C is required."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Option D</div>
                    <div>
                        <asp:TextBox ID="txtPQ_D" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvPQ_D" runat="server"
                            ControlToValidate="txtPQ_D"
                            ValidationGroup="VG_PQ"
                            ErrorMessage="Option D is required."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Correct Answer</div>
                    <div>
                        <asp:DropDownList ID="ddlPQ_Ans" runat="server" CssClass="tu-input">
                            <asp:ListItem Text="Select" Value="" />
                            <asp:ListItem Text="A" Value="A" />
                            <asp:ListItem Text="B" Value="B" />
                            <asp:ListItem Text="C" Value="C" />
                            <asp:ListItem Text="D" Value="D" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvPQ_Ans" runat="server"
                            ControlToValidate="ddlPQ_Ans"
                            InitialValue=""
                            ValidationGroup="VG_PQ"
                            ErrorMessage="Please select the correct answer."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>
                </div>

                <div class="tu-actions">
                    <asp:Button ID="btnCreatePQ" runat="server"
                        Text="Add Question"
                        CssClass="dash-btn primary"
                        ValidationGroup="VG_PQ"
                        OnClientClick="if(!Page_ClientValidate('VG_PQ')) return false; return confirm('Are you sure you want to upload this practice question?');"
                        OnClick="btnCreatePQ_Click" />

                    <asp:Button ID="btnCancelPQ" runat="server"
                        Text="Cancel"
                        CssClass="dash-btn ghost"
                        CausesValidation="false"
                        OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>

    </asp:Panel>

    <asp:Panel ID="pnlAss" runat="server" Visible="false">

        <asp:Panel ID="pnlAssCreate" runat="server" Visible="true">
            <div class="overall-card" style="margin-top:14px;">
                <div class="overall-head">
                    <div class="overall-title tu-section-title">🧠 Upload Self-Assessment Question</div>
                </div>

                <div class="tu-grid">
                    <div class="tu-label">Chapter</div>
                    <div>
                        <asp:DropDownList ID="ddlAssChapter" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvAssChapter" runat="server"
                            ControlToValidate="ddlAssChapter"
                            InitialValue=""
                            ValidationGroup="VG_ASS"
                            ErrorMessage="Please select a chapter."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Question</div>
                    <div>
                        <asp:TextBox ID="txtAssQ" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvAssQ" runat="server"
                            ControlToValidate="txtAssQ" ValidationGroup="VG_ASS"
                            ErrorMessage="Question is required."
                            Display="Dynamic" CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Option A</div>
                    <div>
                        <asp:TextBox ID="txtAssA" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvAssA" runat="server"
                            ControlToValidate="txtAssA"
                            ValidationGroup="VG_ASS"
                            ErrorMessage="Option A is required."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Option B</div>
                    <div>
                        <asp:TextBox ID="txtAssB" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvAssB" runat="server"
                            ControlToValidate="txtAssB"
                            ValidationGroup="VG_ASS"
                            ErrorMessage="Option B is required."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Option C</div>
                    <div>
                        <asp:TextBox ID="txtAssC" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvAssC" runat="server"
                            ControlToValidate="txtAssC"
                            ValidationGroup="VG_ASS"
                            ErrorMessage="Option C is required."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Option D</div>
                    <div>
                        <asp:TextBox ID="txtAssD" runat="server" CssClass="tu-input" />
                        <asp:RequiredFieldValidator ID="rfvAssD" runat="server"
                            ControlToValidate="txtAssD"
                            ValidationGroup="VG_ASS"
                            ErrorMessage="Option D is required."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>

                    <div class="tu-label">Correct Answer</div>
                    <div>
                        <asp:DropDownList ID="ddlAssAns" runat="server" CssClass="tu-input">
                            <asp:ListItem Text="Select" Value="" />
                            <asp:ListItem Text="A" Value="A" />
                            <asp:ListItem Text="B" Value="B" />
                            <asp:ListItem Text="C" Value="C" />
                            <asp:ListItem Text="D" Value="D" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvAssAns" runat="server"
                            ControlToValidate="ddlAssAns"
                            InitialValue=""
                            ValidationGroup="VG_ASS"
                            ErrorMessage="Please select the correct answer."
                            Display="Dynamic"
                            CssClass="tu-error" />
                    </div>
                </div>

                <div class="tu-actions">
                    <asp:Button ID="btnCreateAss" runat="server"
                        Text="Add Question"
                        CssClass="dash-btn primary"
                        ValidationGroup="VG_ASS"
                        OnClientClick="if(!Page_ClientValidate('VG_ASS')) return false; return confirm('Are you sure you want to upload this self-assessment?');"
                        OnClick="btnCreateAss_Click" />

                    <asp:Button ID="btnCancelAss" runat="server"
                        Text="Cancel"
                        CssClass="dash-btn ghost"
                        CausesValidation="false"
                        OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>

    </asp:Panel>

    <script src="https://cdn.jsdelivr.net/npm/tinymce@6/tinymce.min.js"></script>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            if (!window.tinymce) return;

            tinymce.init({
                selector: "#txtContent",
                height: 380,
                menubar: false,
                branding: false,
                plugins: "lists link code table autoresize",
                toolbar: "undo redo | blocks | bold italic underline | bullist numlist | link table | code",
                content_style: "body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;font-size:14px;line-height:1.6}"
            });

            function triggerSave() {
                if (window.tinymce) tinymce.triggerSave();
            }

            document.addEventListener("submit", triggerSave, true);
            document.addEventListener("click", function (e) {
                var btn = e.target.closest("input[type=submit],button");
                if (btn) triggerSave();
            }, true);
        });
    </script>

    <style>
        body { overflow-y: auto }

        .tu-upload-topbar{
            display:flex;
            align-items:center;
            justify-content:space-between;
            gap:14px;
            margin-top:10px;
            margin-bottom:18px;
        }

        .tu-dashboard-btn{
            background-color: transparent !important;
            color: #08619c !important;
            border: 2px solid #08619c !important;
            padding: 8px 18px !important;
            border-radius: 12px !important;
            font-weight: 700 !important;
            font-size: 0.875rem !important;
            text-decoration: none !important;
            display: inline-flex !important;
            align-items: center !important;
            gap: 6px !important;
            letter-spacing: 0.3px !important;
            transition: all 0.2s ease !important;
        }
        .tu-dashboard-btn:hover{        
            background-color: #08619c !important;
            color: #ffffff !important;
            transform: translateY(-1px);
            box-shadow: 0 4px 10px rgba(8, 97, 156, 0.25);
        }

        .tu-tabbar{
            display:flex;
            align-items:center;
            gap:6px;
            padding:6px;
            border-radius:999px;
            background:#f1f5f9;
            border:1px solid rgba(15,23,42,.10);
        }

        .tu-tab{
            display:inline-flex;
            align-items:center;
            gap:8px;
            padding:10px 16px;
            border-radius:999px;
            font-weight:900;
            color:#0f172a;
            text-decoration:none;
            cursor:pointer;
            background:transparent;
            border:0;
        }
        .tu-tab.active{
            background:#fff;
            color:#1e40af;
            box-shadow:0 8px 18px rgba(2,6,23,.08);
            border:1px solid rgba(15,23,42,.10);
        }

        @media (max-width: 900px){
            .tu-upload-topbar{ flex-wrap:wrap; }
            .tu-tabbar{ width:100%; justify-content:flex-start; overflow:auto; }
        }

        .tu-grid{
            display:grid;
            grid-template-columns:200px 1fr;
            gap:14px;
            align-items:center;
        }
        .tu-section-title{ font-size:18px; font-weight:900; letter-spacing:-0.2px; }
        .tu-label{ font-weight:800; color:#0f172a; }

        .tu-input{
            width:100%;
            padding:10px 12px;
            border-radius:10px;
            border:1px solid rgba(15,23,42,.15);
            font-size:14px;
        }
        .tu-editor{
            width:100%;
            padding:14px;
            border-radius:12px;
            border:1px solid rgba(15,23,42,.15);
            font-size:14px;
        }
        .tu-actions{
            margin-top:18px;
            display:flex;
            gap:12px;
            justify-content:flex-end;
        }
        .tu-error{
            display:block;
            margin-top:6px;
            font-size:12px;
            color:#dc2626;
            font-weight:600;
        }

        .overall-head{
            padding-bottom:16px;
        }
        .overall-title{
            margin-bottom:6px;
        }

        .toast{
            position:fixed; top:18px; right:18px; z-index:9999;
            display:flex; align-items:center; gap:10px;
            padding:12px 14px; border-radius:14px;
            border:1px solid rgba(15,23,42,.12); background:#fff;
            box-shadow: 0 10px 24px rgba(2,6,23,.12);
            font-weight:800; color:#0f172a; min-width:220px;
        }
        .toast.ok{ border-color: rgba(29,78,216,.30); background: rgba(29,78,216,.08); color:#1d4ed8; }
        .toast.bad{ border-color: rgba(239,68,68,.30); background: rgba(239,68,68,.10); color:#991b1b; }

        @media (max-width:980px){
            .tu-grid{ grid-template-columns:1fr; }
            .toast{ left:14px; right:14px; }
        }
    </style>

</asp:Content>