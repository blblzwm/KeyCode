<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="selfassessment.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.SelfAssessment" MasterPageFile="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <link href="../Content/SelfAssessmentStyle.css" rel="stylesheet" type="text/css" />

    <style>
        .aspNetDisabled, [disabled] {
            opacity: 0.4 !important;
            cursor: not-allowed !important;
            pointer-events: none;
        }
        .q-wrap { margin-bottom: 20px; padding: 15px; border-radius: 10px; border: 1px solid #f1f5f9; }
        .edit-box { background: #f8fafc; border: 1px dashed #3b82f6; padding: 15px; border-radius: 8px; margin-top: 10px; }
        .edit-input { width: 100%; padding: 8px; border: 1px solid #cbd5e1; border-radius: 6px; margin-bottom: 10px; }
        #btnBackToDash:hover { background: #08619c !important;color: #fff !important;}
    </style>

    <div class="container" style="padding-top: 30px;">
        
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
            <asp:Button ID="btnBackToDash" runat="server"
                ClientIDMode="Static"
                Text="« Back to Dashboard"
                OnClick="btnBackDash_Click"
                CssClass="pm-btn"
                style="font-size:13px; padding:8px 18px; background:transparent; border:2px solid #08619c; color:#08619c; border-radius:12px; cursor:pointer; font-weight:700;" />
        </div>

        <div class="assessment-header" style="text-align: center; margin-bottom: 30px;">
            <h1 style="font-size: 32px; font-weight: 900;"><asp:Literal ID="litBigTitle" runat="server" /></h1>
            <p style="font-size: 18px; color: #64748b;"><asp:Literal ID="litSubTitle" runat="server" /></p>
        </div>

        <div style="margin-bottom: 25px;">
            
            <asp:PlaceHolder ID="phNormalMode" runat="server">
                <div style="text-align: right;" id="divAdminActions" runat="server">
                    <asp:LinkButton ID="lnkEditMode" runat="server" OnClick="lnkEditMode_Click" 
                        style="background: #f1f5f9; border: 1px solid #cbd5e1; padding: 10px 20px; border-radius: 8px; color: #1e293b; font-weight: 600; text-decoration: none; display: inline-block;">
                        ✏️ Enter Edit Mode
                    </asp:LinkButton>
                </div>
            </asp:PlaceHolder>

            <asp:Literal ID="litReviewStatus" runat="server" />

            <asp:Panel ID="pnlEditControls" runat="server" Visible="false" 
                style="display: flex; justify-content: space-between; align-items: center; background: #f8fafc; padding: 15px; border-radius: 12px; border: 1px solid #e2e8f0;">
                
                <div>
                    <asp:Button ID="btnDeleteSelected" runat="server" Text="Delete Selected" 
                        OnClick="btnBulkDelete_Click" 
                        OnClientClick="return confirm('Delete selected items?');"
                        style="background:#ef4444; color:white; border:none; padding: 10px 20px; border-radius: 8px; cursor:pointer; font-weight:bold;" />
                </div>

                <div style="display: flex; gap: 12px;">
                    <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" OnClick="btnCancel_Click" 
                        style="background:white; border:1px solid #cbd5e1; color:#64748b; padding: 10px 20px; border-radius: 8px; cursor:pointer;" />

                    <asp:Button ID="btnSaveAll" runat="server" Text="SAVE ALL CHANGES" OnClick="btnSaveAll_Click" 
                        style="background:dodgerblue; color:white; border:none; padding: 10px 24px; border-radius: 8px; cursor:pointer; font-weight:bold;" />
                </div>
            </asp:Panel>
        </div>

        <div class="quiz-card">
            <asp:Repeater ID="rptAssessment" runat="server" OnItemDataBound="rptAssessment_ItemDataBound">
                <ItemTemplate>
                    <div class="q-wrap" style="position:relative; margin-bottom:20px;">
                
                        <asp:PlaceHolder ID="phAdminDelete" runat="server">
                            <div style="position:absolute; left:-35px; top:15px;">
                                <input type="checkbox" id="chkDelete" runat="server" style="width:18px; height:18px;" />
                            </div>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder ID="phInputs" runat="server">
                            <p class="q-text" style="padding-right:110px;">
                                <span class="pm-role" style="margin-right:10px;"><%# Container.ItemIndex + 1 %>.</span>
                                <strong><%# Eval("question") %></strong>
                            </p>
                            <asp:RadioButtonList ID="rblOptions" runat="server" CssClass="quiz-options" />
                    
                            <div class="error-msg" style="color: #dc2626; font-size: 13px; font-weight: 700; margin-top: 10px; display: none;">
                                * Please answer Question <%# Container.ItemIndex + 1 %> before submitting.
                            </div>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder ID="phPlainText" runat="server" Visible="false">
                            <div class="edit-box" style="background:#f8fafc; padding:15px; border-radius:8px;">
                                <asp:HiddenField ID="hfAssID" runat="server" Value='<%# Eval("assID") %>' />
                        
                                <label style="font-weight:bold;"><%# Container.ItemIndex + 1 %>. Question Text:</label>
                                <asp:TextBox ID="txtEditQ" runat="server" Text='<%# Eval("question") %>' CssClass="edit-input" TextMode="MultiLine" Rows="2" style="width:100%;" />
                        
                                <div style="display:grid; grid-template-columns: 1fr 1fr; gap:10px; margin-top:10px;">
                                    <div><label>Option A:</label><asp:TextBox ID="txtA" runat="server" Text='<%# Eval("optionA") %>' CssClass="edit-input" style="width:100%;" /></div>
                                    <div><label>Option B:</label><asp:TextBox ID="txtB" runat="server" Text='<%# Eval("optionB") %>' CssClass="edit-input" style="width:100%;" /></div>
                                    <div><label>Option C:</label><asp:TextBox ID="txtC" runat="server" Text='<%# Eval("optionC") %>' CssClass="edit-input" style="width:100%;" /></div>
                                    <div><label>Option D:</label><asp:TextBox ID="txtD" runat="server" Text='<%# Eval("optionD") %>' CssClass="edit-input" style="width:100%;" /></div>
                                </div>
                        
                                <div style="margin-top:10px;">
                                    <label style="font-weight:bold;">Correct Answer:</label>
                                    <asp:DropDownList ID="ddlAns" runat="server">
                                        <asp:ListItem Value="A">A</asp:ListItem>
                                        <asp:ListItem Value="B">B</asp:ListItem>
                                        <asp:ListItem Value="C">C</asp:ListItem>
                                        <asp:ListItem Value="D">D</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </asp:PlaceHolder>

                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <div style="text-align: center; margin-top: 30px;">
                <asp:Button ID="btnSubmit" runat="server"
                     ClientIDMode="Static"
                     Text="Submit Assessment"
                     OnClick="btnSubmit_Click"
                     CssClass="pm-btn"
                     style="font-size:13px; padding:8px 18px; background:transparent; border:2px solid #08619c; color:#08619c; border-radius:12px; cursor:pointer; font-weight:700;" />
            </div>
        </div>

        <div style="text-align: center; margin: 40px 0;">
            <a href="#" class="pm-role" style="text-decoration: none; font-size: 14px; font-weight:800;">↑ Back to Top</a>
        </div>
    </div>

    <script>
        function toggleMenu() { document.getElementById("myDropdown").classList.toggle("show"); }
        window.onclick = function (e) {
            if (!e.target.matches('.dots-btn')) {
                var d = document.getElementsByClassName("dropdown-content");
                for (var i = 0; i < d.length; i++) d[i].classList.remove('show');
            }
        }

        function validateQuiz() {
            var isValid = true;
            var groups = document.querySelectorAll('.q-wrap');

            groups.forEach(function (group) {
                // Find all radio inputs in this specific question block
                var radios = group.querySelectorAll('input[type="radio"]');
                var errorMsg = group.querySelector('.error-msg');

                // Check if at least one is checked
                var isChecked = Array.from(radios).some(r => r.checked);

                if (!isChecked) {
                    isValid = false;
                    if (errorMsg) errorMsg.style.display = 'block'; // Show "Please select an answer"
                    group.style.border = "1px solid #ef4444"; // Optional: highlight red
                } else {
                    if (errorMsg) errorMsg.style.display = 'none';
                    group.style.border = "1px solid #f1f5f9";
                }
            });

            if (!isValid) {
                alert("Please answer all questions before submitting.");
                window.scrollTo(0, 0); // Scroll up so they see what they missed
            }

            return isValid; // If false, C# code will not run
        }
    </script>
</asp:Content>
