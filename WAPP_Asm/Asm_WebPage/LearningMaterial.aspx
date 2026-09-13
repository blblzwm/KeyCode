<%@ Page Title="Learning Material" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="LearningMaterial.aspx.cs"
    Inherits="WAPP_Asm.Asm_WebPage.LearningMaterial"
    MaintainScrollPositionOnPostBack="true"
    ValidateRequest="false" %>

<asp:Content ID="Head1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Asm_StyleSheet/LearningMaterialStyle.css?v=" + DateTime.Now.Ticks) %>" rel="stylesheet" />

    <script src="https://cdn.jsdelivr.net/pyodide/v0.26.2/full/pyodide.js"></script>

    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.65.16/codemirror.min.css" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.65.16/codemirror.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/codemirror/5.65.16/mode/python/python.min.js"></script>

    <script src="https://cdn.jsdelivr.net/npm/tinymce@6/tinymce.min.js"></script>

    <style>
        .title-row { display:flex; align-items:center; justify-content:space-between; gap:12px; }
        .title-row .content-title { margin:0; }
        .title-row .lm-action-wrapper { display:flex; align-items:center; }

        .action-menu { position:relative; display:inline-block; }
        .action-dots{
            width:42px;height:38px;border-radius:12px;
            border:1px solid rgba(15,23,42,.10);background:#fff;cursor:pointer;
            font-weight:900;font-size:18px;line-height:1;
        }
        .action-dots:hover{ background:#f1f5f9; }

        .action-dropdown{
            position:absolute; top:44px; min-width:280px;
            background:#fff; border:1px solid rgba(15,23,42,.10);
            box-shadow:0 10px 26px rgba(2,6,23,.10);
            border-radius:14px; padding:8px; display:none; z-index:99999;
            left:auto; right:0;
        }
        .action-menu.open .action-dropdown{ display:block; }

        .action-item{
            width:100%; text-align:left; background:transparent; border:0;
            padding:10px 10px; border-radius:10px; cursor:pointer;
            font-weight:800; color:#0f172a; display:block;
        }
        .action-item:hover{ background:#f1f5f9; }
        .action-item.danger{ color:#991b1b; }
        .action-item.danger:hover{ background:#fee2e2; }
        .action-sep{ height:1px; background:rgba(15,23,42,.10); margin:6px 4px; }

        .action-dropdown input.action-item[disabled],
        .action-dropdown button.action-item[disabled]{
            opacity:.45 !important; cursor:not-allowed !important;
            pointer-events:none !important; filter:grayscale(.25) !important;
        }
        .action-dropdown input.action-item[disabled].danger,
        .action-dropdown button.action-item[disabled].danger{
            color:rgba(153,27,27,.6) !important;
        }

        .field-error{ border:2px solid #b91c1c !important; outline:none !important; }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="learning-layout">

        <div class="top-nav">
            <div class="nav-left">


                <div class="chapter-wrap" title="<%= lblChapterTitle.Text %>">
                    <asp:Label ID="lblChapterTitle" runat="server" CssClass="nav-chapter-title" />
                </div>
            </div>

            <div class="nav-center">
                <asp:Repeater ID="rptSubtopics" runat="server">
                    <ItemTemplate>
                        <a class='top-nav-item <%# (bool)Eval("IsActive") ? "active" : "" %>'
                           href='<%# ResolveUrl("~/Asm_WebPage/LearningMaterial.aspx?chapter="
                                + Server.UrlEncode(Eval("ChapterId").ToString())
                                + "&sub=" + Server.UrlEncode(Eval("SubtopicId").ToString())) %>'>
                            <%# Eval("DisplayNo") %> <%# Eval("CleanTitle") %>
                        </a>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:HyperLink ID="lnkSelf" runat="server" CssClass="top-nav-item self" Text="★ Self Assessment" />
            </div>

            <div class="nav-right">
                <asp:HyperLink ID="lnkPrev" runat="server" CssClass="top-nav-item home nav-icon" Visible="false">&lt;</asp:HyperLink>
                <asp:HyperLink ID="lnkNext" runat="server" CssClass="top-nav-item home nav-icon" Visible="false">&gt;</asp:HyperLink>
            </div>
        </div>

        <div class="lesson-split">

            <div class="lesson-left">

                <div class="title-row">
                    <h1 class="content-title"><asp:Label ID="lblSubtopicTitle" runat="server" /></h1>

                    <div class="lm-action-wrapper">

                        <asp:Panel ID="pnlTutorActions" runat="server" Visible="false" CssClass="lm-actions">
                            <div class="action-menu">
                                <button type="button" class="action-dots" aria-haspopup="true" aria-expanded="false">⋯</button>
                                <div class="action-dropdown">
                                    <asp:Button ID="btnEdit" runat="server" Text="Edit" CssClass="action-item" OnClick="btnEdit_Click" />
                                    <div class="action-sep"></div>
                                    <asp:Button ID="btnDeleteSubtopic" runat="server" Text="Delete Subtopic" CssClass="action-item danger"
                                        OnClientClick="return confirm('Delete Subtopic?\nThis will delete the subtopic AND its practice question.\nYou can restore it in Recently Deleted within 30 days.');"
                                        OnClick="btnDeleteSubtopic_Click" />
                                    <div class="action-sep"></div>
                                    <asp:Button ID="btnDeletePQ" runat="server" Text="Delete Practice Question" CssClass="action-item danger"
                                        OnClientClick="return confirm('Delete Practice Question?\nYou can restore it in Recently Deleted within 30 days.');"
                                        OnClick="btnDeletePQ_Click" />
                                    <div class="action-sep"></div>
                                    <a class="action-item" style="text-decoration:none;" href="<%= ResolveUrl("~/Asm_WebPage/RecentlyDeleted.aspx") %>">View Recently Deleted</a>
                                </div>
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlAdminDelete" runat="server" Visible="false" CssClass="lm-actions">
                            <div class="action-menu">
                                <button type="button" class="action-dots" aria-haspopup="true" aria-expanded="false">⋯</button>
                                <div class="action-dropdown">
                                    <asp:Button ID="btnAdminDeleteSubtopic" runat="server" Text="Delete Subtopic" CssClass="action-item danger"
                                        OnClientClick="return confirm('Admin: delete subtopic + PQ?\nRestore within 30 days.');"
                                        OnClick="btnAdminDeleteSubtopic_Click" />
                                    <div class="action-sep"></div>
                                    <asp:Button ID="btnAdminDeletePQ" runat="server" Text="Delete Practice Question" CssClass="action-item danger"
                                        OnClientClick="return confirm('Admin: delete Practice Question?\nRestore within 30 days.');"
                                        OnClick="btnAdminDeletePQ_Click" />
                                    <div class="action-sep"></div>
                                    <a class="action-item" style="text-decoration:none;" href="<%= ResolveUrl("~/Asm_WebPage/RecentlyDeleted.aspx") %>">View Recently Deleted</a>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                </div>

                <div class="divider"></div>

                <asp:Panel ID="pnlDisplay" runat="server" Visible="true">
                    <div class="content-desc">
                        <asp:Literal ID="litContent" runat="server" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlEdit" runat="server" Visible="false">

                    <asp:TextBox ID="txtEditContent" runat="server" ClientIDMode="Static"
                        TextMode="MultiLine" Rows="15" CssClass="edit-box" Width="100%" />

                    <asp:Panel ID="pnlPQEdit" runat="server" Visible="false" CssClass="practice-card" style="margin-top:16px;">
                        <div class="pq-title">Edit Practice Question</div>

                        <div style="margin-top:10px;">
                            <div style="font-weight:800;margin-bottom:6px;">Question</div>
                            <asp:TextBox ID="txtPQQuestion" runat="server" TextMode="MultiLine" Rows="3" CssClass="edit-box" Width="100%" />
                        </div>

                        <div style="margin-top:10px;">
                            <div style="font-weight:800;margin-bottom:6px;">Option A</div>
                            <asp:TextBox ID="txtPQA" runat="server" CssClass="edit-box" Width="100%" />
                        </div>

                        <div style="margin-top:10px;">
                            <div style="font-weight:800;margin-bottom:6px;">Option B</div>
                            <asp:TextBox ID="txtPQB" runat="server" CssClass="edit-box" Width="100%" />
                        </div>

                        <div style="margin-top:10px;">
                            <div style="font-weight:800;margin-bottom:6px;">Option C</div>
                            <asp:TextBox ID="txtPQC" runat="server" CssClass="edit-box" Width="100%" />
                        </div>

                        <div style="margin-top:10px;">
                            <div style="font-weight:800;margin-bottom:6px;">Option D</div>
                            <asp:TextBox ID="txtPQD" runat="server" CssClass="edit-box" Width="100%" />
                        </div>

                        <div style="margin-top:12px;">
                            <div style="font-weight:800;margin-bottom:6px;">Correct Answer</div>
                            <asp:DropDownList ID="ddlPQAns" runat="server" CssClass="edit-box" Width="160px">
                                <asp:ListItem Text="A" Value="A" />
                                <asp:ListItem Text="B" Value="B" />
                                <asp:ListItem Text="C" Value="C" />
                                <asp:ListItem Text="D" Value="D" />
                            </asp:DropDownList>
                        </div>
                    </asp:Panel>

                    <asp:Label ID="lblEditError" runat="server" Visible="false" ClientIDMode="Static"
                        style="display:block;margin-top:10px;margin-bottom:10px;color:#b91c1c;font-weight:800;" />

                    <div style="margin-top:12px;">
                        <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="lm-btn primary"
                            OnClientClick="return validateEditBeforePostback() && confirm('Save changes now?');"
                            OnClick="btnSave_Click" />

                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="lm-btn" OnClick="btnCancel_Click"
                            OnClientClick="return confirm('Discard all changes and exit edit mode?');" />
                    </div>
                </asp:Panel>

                <asp:UpdatePanel ID="upPractice" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnlPractice" runat="server" CssClass="practice-card" Visible="false">
                            <div class="pq-title">Practice Question</div>
                                <p class="q-text"><asp:Label ID="lblQ" runat="server" ClientIDMode="Static" /></p>
                                <asp:RadioButtonList ID="rblOptions" runat="server" ClientIDMode="Static" CssClass="rbl" RepeatDirection="Vertical" />
                            <div style="margin-top:16px;">
                                <asp:Button ID="btnSubmit" runat="server" Text="Submit Answer" CssClass="btn-submit" OnClick="btnSubmit_Click" />
                                <asp:Label ID="lblResult" runat="server" CssClass="result-label" />
                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>

            </div>

            <div class="resizer-vertical" title="Drag"></div>

            <div class="lesson-right">
                <div class="try-panel">
                    <div class="ide-topbar">
                        <div class="ide-lang">Python</div>
                        <div class="ide-actions">
                            <span id="pyStatus" class="ide-status">Loading Python...</span>
                            <button id="btnReset" type="button" class="ide-icon" title="Reset">⟳</button>
                        </div>
                    </div>

                    <div class="ide-editorWrap">
                        <div class="editor-container ide-editor">
                            <textarea id="pyCode"></textarea>
                        </div>
                    </div>

                    <div class="resizer-horizontal" title="Drag"></div>

                    <div class="ide-bottombar">
                        <div class="ide-tabs">
                            <div class="ide-tab active" data-tab="console">CONSOLE</div>
                            <div class="ide-tab" data-tab="ai">AI RESPONSES (<span id="aiCount">0</span>)</div>
                        </div>

                        <div class="ide-btnRow">
                            <button id="btnAskAI" type="button" class="ide-askbtn">🤖 Ask AI</button>
                            <button id="btnRun" type="button" class="ide-runbtn">▶ Run Code</button>
                        </div>
                    </div>

                    <div class="console-container ide-console">
                        <div id="pyOut" class="pyout"></div>

                        <div id="aiPanel" class="ai-panel" style="display:none;">
                            <div id="aiContent" class="ai-content"></div>
                        </div>

                        <div id="pyLine" class="pyline" style="display:none;">
                            <span id="pyPrompt" class="pyprompt"></span>
                            <input id="pyIn" class="pyin" type="text" autocomplete="off" spellcheck="false" />
                        </div>
                    </div>
                </div>
            </div>

        </div>
    </div>

    <div id="videoModal" class="video-modal" style="display:none;">
        <div class="video-backdrop"></div>
        <div class="video-card">
            <button type="button" class="video-close" aria-label="Close">✕</button>
            <div class="video-wrap">
                <iframe id="videoFrame" src=""
                        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                        allowfullscreen></iframe>
            </div>
        </div>
    </div>
    <style>
        .video-modal{ position:fixed; inset:0; z-index:100000; }
        .video-backdrop{ position:absolute; inset:0; background:rgba(2,6,23,.65); }
        .video-card{
            position:absolute; left:50%; top:50%; transform:translate(-50%,-50%);
            width:min(920px, calc(100vw - 32px));
            background:#fff; border-radius:18px;
            box-shadow:0 18px 50px rgba(2,6,23,.25);
            padding:14px;
        }
        .video-close{
            position:absolute; right:10px; top:10px;
            width:38px; height:38px; border-radius:12px;
            border:1px solid rgba(15,23,42,.12);
            background:#fff; cursor:pointer; font-weight:900;
        }
        .video-wrap{ margin-top:34px; aspect-ratio:16/9; width:100%; }
        #videoFrame{ width:100%; height:100%; border:0; border-radius:14px; }
    </style>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            document.addEventListener("click", function (e) {
                const menu = e.target.closest(".action-menu");
                const dots = e.target.closest(".action-dots");

                if (dots && menu) {
                    const isOpen = menu.classList.toggle("open");
                    dots.setAttribute("aria-expanded", isOpen ? "true" : "false");
                    e.stopPropagation();
                    return;
                }
                if (menu && e.target.closest(".action-dropdown")) return;
                document.querySelectorAll(".action-menu.open").forEach(m => m.classList.remove("open"));
            }, true);
        });
    </script>

    <script>
        (function () {
            function initTinyEdit() {
                if (!window.tinymce) return;
                var el = document.getElementById("txtEditContent");
                if (!el) return;
                if (tinymce.get("txtEditContent")) return;

                tinymce.init({
                    selector: "#txtEditContent",
                    height: 380,
                    menubar: false,
                    branding: false,
                    plugins: "lists link code table autoresize",
                    toolbar: "undo redo | blocks | bold italic underline | bullist numlist | link table | code",
                    content_style: "body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;font-size:14px;line-height:1.7}"
                });
            }

            function triggerSave() { if (window.tinymce) tinymce.triggerSave(); }

            document.addEventListener("DOMContentLoaded", function () {
                initTinyEdit();
                document.addEventListener("submit", triggerSave, true);
                document.addEventListener("click", function (e) {
                    var btn = e.target.closest("input[type=submit],button");
                    if (btn) triggerSave();
                }, true);
            });

            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                    initTinyEdit();
                });
            }
        })();
    </script>

    <script>
        function clearFieldErrors() {
            document.querySelectorAll('.field-error').forEach(function (el) {
                el.classList.remove('field-error');
            });
        }
        function showEditError(msg) {
            var el = document.getElementById('lblEditError');
            if (!el) return;
            el.style.display = 'block';
            el.innerText = msg || '';
        }
        function hideEditError() {
            var el = document.getElementById('lblEditError');
            if (!el) return;
            el.style.display = 'none';
            el.innerText = '';
        }
        function markError(el) { if (el) el.classList.add('field-error'); }
        function focusFirst(els) { for (var i = 0; i < els.length; i++) { if (els[i]) { els[i].focus(); return; } } }

        function validateEditBeforePostback() {
            if (window.tinymce) tinymce.triggerSave();

            clearFieldErrors();
            hideEditError();

            var content = document.getElementById('txtEditContent');
            var contentVal = content ? (content.value || '').trim() : '';
            if (!contentVal) {
                showEditError('❌ Content cannot be empty.');
                markError(content);
                focusFirst([content]);
                return false;
            }

            var pqPanel = document.getElementById('<%= pnlPQEdit.ClientID %>');
            var pqVisible = pqPanel && pqPanel.offsetParent !== null;

            if (pqVisible) {
                var q = document.getElementById('<%= txtPQQuestion.ClientID %>');
                var a = document.getElementById('<%= txtPQA.ClientID %>');
                var b = document.getElementById('<%= txtPQB.ClientID %>');
                var c = document.getElementById('<%= txtPQC.ClientID %>');
                var d = document.getElementById('<%= txtPQD.ClientID %>');
                var ans = document.getElementById('<%= ddlPQAns.ClientID %>');

                var qv = (q && q.value ? q.value : '').trim();
                var av = (a && a.value ? a.value : '').trim();
                var bv = (b && b.value ? b.value : '').trim();
                var cv = (c && c.value ? c.value : '').trim();
                var dv = (d && d.value ? d.value : '').trim();
                var ansVal = (ans && ans.value ? ans.value : '').trim().toUpperCase();

                var bad = [];
                if (!qv) { markError(q); bad.push(q); }
                if (!av) { markError(a); bad.push(a); }
                if (!bv) { markError(b); bad.push(b); }
                if (!cv) { markError(c); bad.push(c); }
                if (!dv) { markError(d); bad.push(d); }
                if (!['A','B','C','D'].includes(ansVal)) { markError(ans); bad.push(ans); }

                if (bad.length) {
                    showEditError('❌ Practice Question cannot be empty. Fill Question + A-D and choose Answer.');
                    focusFirst(bad);
                    return false;
                }
            }
            return true;
        }
    </script>

    <script>
        (function () {
            if (window.__VIDEO_MODAL_BIND__) return;
            window.__VIDEO_MODAL_BIND__ = true;

            function openVideo(url) {
                var modal = document.getElementById('videoModal');
                var frame = document.getElementById('videoFrame');
                if (!modal || !frame) return;
                frame.src = url || '';
                modal.style.display = 'block';
                document.body.style.overflow = 'hidden';
            }
            function closeVideo() {
                var modal = document.getElementById('videoModal');
                var frame = document.getElementById('videoFrame');
                if (!modal || !frame) return;
                frame.src = '';
                modal.style.display = 'none';
                document.body.style.overflow = '';
            }

            document.addEventListener('click', function (e) {
                var btn = e.target.closest('.js-video');
                if (btn) {
                    e.preventDefault();
                    var url = btn.getAttribute('data-video') || '';
                    if (!url) return;
                    openVideo(url);
                    return;
                }
                if (e.target.closest('.video-close') || e.target.classList.contains('video-backdrop')) {
                    e.preventDefault();
                    closeVideo();
                }
            }, true);

            document.addEventListener('keydown', function (e) {
                if (e.key === 'Escape') closeVideo();
            });
        })();
    </script>

    <script>
        (function () {
            function clamp(n, min, max) { return Math.max(min, Math.min(max, n)); }

            document.addEventListener("DOMContentLoaded", function () {

                const split = document.querySelector(".lesson-split");
                const left = document.querySelector(".lesson-left");
                const vBar = document.querySelector(".resizer-vertical");
                const right = document.querySelector(".lesson-right");

                if (split && left && vBar && right) {
                    const MIN_LEFT = 420;
                    const MIN_RIGHT = 520;

                    function fitLearningPanes() {
                        if (getComputedStyle(split).flexDirection === "column") {
                            left.style.flex = "";
                            return;
                        }
                        const available = split.clientWidth - vBar.offsetWidth;
                        const maxLeft = Math.max(MIN_LEFT, available - MIN_RIGHT);
                        const current = left.getBoundingClientRect().width;
                        if (current > maxLeft) left.style.flex = `0 0 ${maxLeft}px`;
                    }
                    if (window.ResizeObserver) {
                        new ResizeObserver(fitLearningPanes).observe(split);
                    } else {
                        window.addEventListener("resize", fitLearningPanes);
                    }
                    fitLearningPanes();

                    let startX = 0;
                    let startLeftW = 0;
                    let draggingV = false;

                    vBar.addEventListener("mousedown", function (e) {
                        draggingV = true;
                        document.body.classList.add("resizing");
                        startX = e.clientX;
                        startLeftW = left.getBoundingClientRect().width;
                        e.preventDefault();
                    });

                    window.addEventListener("mousemove", function (e) {
                        if (!draggingV) return;

                        const dx = e.clientX - startX;
                        const splitW = split.getBoundingClientRect().width;
                        const barW = vBar.getBoundingClientRect().width;

                        const maxLeft = splitW - barW - MIN_RIGHT;
                        const newLeft = clamp(startLeftW + dx, MIN_LEFT, maxLeft);
                        left.style.flex = `0 0 ${newLeft}px`;
                    });

                    window.addEventListener("mouseup", function () {
                        if (!draggingV) return;
                        draggingV = false;
                        document.body.classList.remove("resizing");
                        window.dispatchEvent(new Event("resize"));
                    });
                }

                const tryPanel = document.querySelector(".try-panel");
                const hBar = document.querySelector(".resizer-horizontal");
                const bottomBar = document.querySelector(".ide-bottombar");
                const consoleEl = document.querySelector(".ide-console");

                if (tryPanel && hBar && bottomBar && consoleEl) {
                    const MIN_CONSOLE = 140;
                    const MIN_EDITOR = 180;

                    let startY = 0;
                    let startConsoleH = 0;
                    let draggingH = false;

                    hBar.addEventListener("mousedown", function (e) {
                        draggingH = true;
                        document.body.classList.add("resizing");
                        startY = e.clientY;
                        startConsoleH = consoleEl.getBoundingClientRect().height;
                        e.preventDefault();
                    });

                    window.addEventListener("mousemove", function (e) {
                        if (!draggingH) return;

                        const dy = e.clientY - startY;

                        const panelH = tryPanel.getBoundingClientRect().height;
                        const topH = (document.querySelector(".ide-topbar") || { offsetHeight: 0 }).offsetHeight;
                        const botH = bottomBar.offsetHeight;
                        const barH2 = hBar.getBoundingClientRect().height;

                        const usable = panelH - topH - botH - barH2;
                        const maxConsole = usable - MIN_EDITOR;

                        const newConsole = clamp(startConsoleH - dy, MIN_CONSOLE, maxConsole);
                        consoleEl.style.flex = `0 0 ${newConsole}px`;
                        window.dispatchEvent(new Event("resize"));
                    });

                    window.addEventListener("mouseup", function () {
                        if (!draggingH) return;
                        draggingH = false;
                        document.body.classList.remove("resizing");
                        window.dispatchEvent(new Event("resize"));
                    });
                }
            });
        })();
    </script>

    <script>
        (function () {
            if (window.__LM_TOGGLE_INIT__) return;
            window.__LM_TOGGLE_INIT__ = true;

            function setExpanded(btn, expanded) {
                btn.setAttribute('aria-expanded', expanded ? 'true' : 'false');
                btn.textContent = expanded ? ' ▼ Hide Video Explanation ' : ' ▶ Show Video Explanation ';
            }

            function openPanel(btn, panel) {
                panel.classList.remove('is-collapsed');
                panel.classList.add('is-open');
                panel.setAttribute('aria-hidden', 'false');
                panel.style.display = 'block';
                setExpanded(btn, true);

                var iframe = panel.querySelector('iframe');
                if (iframe && iframe.dataset.src && !iframe.src) iframe.src = iframe.dataset.src;
            }

            function closePanel(btn, panel) {
                panel.classList.add('is-collapsed');
                panel.classList.remove('is-open');
                panel.setAttribute('aria-hidden', 'true');
                panel.style.display = 'none';
                setExpanded(btn, false);
            }

            document.addEventListener('click', function (e) {
                var btn = e.target.closest('.js-toggle');
                if (!btn) return;

                e.preventDefault();
                e.stopPropagation();

                var sel = btn.getAttribute('data-target');
                if (!sel) return;

                var panel = document.querySelector(sel);
                if (!panel) return;

                var expanded = (btn.getAttribute('aria-expanded') === 'true');

                if (expanded) closePanel(btn, panel);
                else openPanel(btn, panel);
            }, true);

            document.addEventListener('DOMContentLoaded', function () {
                document.querySelectorAll('.lm-togglePanel').forEach(function (p) {
                    if (!p.classList.contains('is-collapsed')) p.classList.add('is-collapsed');
                    p.setAttribute('aria-hidden', 'true');
                    p.style.display = 'none';
                });
                document.querySelectorAll('.js-toggle[aria-expanded]').forEach(function (b) {
                    setExpanded(b, b.getAttribute('aria-expanded') === 'true');
                });
            });
        })();
    </script>

    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const sidebar = document.getElementById("dashSidebar");
            if (!sidebar) return;
            const dashboard = Array.from(sidebar.querySelectorAll("a.side-nav-item")).find(function (link) {
                const url = new URL(link.href, location.href);
                return /\/(Student|Tutor|Admin)Dashboard\.aspx$/i.test(url.pathname)
                    && url.searchParams.get("tab") !== "game";
            });
            if (!dashboard) return;
            <% string dashboardRole = Convert.ToString(Session["role"]).Trim().ToLowerInvariant(); %>
            dashboard.href = '<%= ResolveUrl(dashboardRole == "tutor" ? "~/Asm_WebPage/TutorDashboard.aspx" : dashboardRole == "admin" ? "~/Asm_WebPage/AdminDashboard.aspx" : "~/Asm_WebPage/StudentDashboard.aspx") %>';
            dashboard.addEventListener("click", function (event) {
                if (!window.confirm("Leave this learning page and return to Dashboard? Unsaved code or edits may be lost.")) {
                    event.preventDefault();
                    event.stopImmediatePropagation();
                }
            });
        });
    </script>
</asp:Content>