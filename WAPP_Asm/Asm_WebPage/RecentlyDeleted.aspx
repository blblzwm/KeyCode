<%@ Page Title="Recently Deleted" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="RecentlyDeleted.aspx.cs"
    Inherits="WAPP_Asm.Asm_WebPage.RecentlyDeleted" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        :root{
            --blue:#2563eb;
            --ink:#0f172a;
            --muted:#64748b;
            --border: rgba(15,23,42,.10);
            --border-strong: rgba(15,23,42,.12);
            --card:#ffffff;
            --soft:#f8fafc;
            --shadow: 0 8px 22px rgba(2,6,23,.06);
        }

        .rd-page{
            max-width: 1100px;
            margin: 14px auto 30px;
            padding: 0 16px;
        }

        .rd-top{
            display:flex;
            align-items:center;
            justify-content:space-between;
            gap:12px;
            margin-bottom: 14px;
        }

        .rd-titleblock{
            display:flex;
            flex-direction:column;
            gap:4px;
        }

        .rd-title{
            font-size: 26px;
            font-weight: 900;
            color: var(--ink);
            letter-spacing: .2px;
        }
        .rd-sub{
            color: var(--muted);
            font-weight: 600;
        }

        .rd-back{
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
        .rd-back:hover{        
            background-color: #08619c !important;
            color: #ffffff !important;
            transform: translateY(-1px);
            box-shadow: 0 4px 10px rgba(8, 97, 156, 0.25); 

        }

        .rd-bar{
            display:flex;
            gap:10px;
            align-items:center;
            flex-wrap:wrap;
            margin: 10px 0 18px;
        }
        .rd-in{
            padding: 11px 12px;
            border-radius: 12px;
            border: 1px solid var(--border-strong);
            min-width: 320px;
            outline: none;
            font-weight: 700;
        }

        .rd-btn{
            padding: 11px 14px;
            border-radius: 12px;
            border: 1px solid var(--border-strong);
            background:#fff;
            font-weight: 900;
            cursor:pointer;
        }
        .rd-btn:hover{ background:#f1f5f9; }

        .rd-card{
            margin-top: 14px;
            background: var(--card);
            border: 1px solid var(--border);
            border-radius: 16px;
            overflow:hidden;
            box-shadow: var(--shadow);
        }
        .rd-card-h{
            padding: 14px 16px;
            font-weight: 900;
            color: var(--ink);
            border-bottom: 1px solid var(--border);
            background: var(--soft);
        }

        .rd-grid{
            width:100%;
            border-collapse:collapse;
        }
        .rd-grid th, .rd-grid td{
            padding: 12px 12px;
            border-bottom: 1px solid var(--border);
            text-align:left;
            vertical-align: top;
        }
        .rd-grid th{
            font-weight: 900;
            background:#fff;
            color: var(--ink);
        }

        .rd-link{
            font-weight: 900;
            color: var(--blue);
            text-decoration:none;
        }
        .rd-link:hover{ text-decoration: underline; }

        .rd-grid .EmptyDataRowStyle td{
            color: var(--muted);
            font-weight: 700;
            padding: 14px 12px;
        }
    </style>

    <div class="rd-page">

        <div class="rd-top">
            <asp:LinkButton ID="btnBack" runat="server" CssClass="rd-back" OnClick="btnBack_Click" style="text-decoration:none;">
                « Back to Dashboard
            </asp:LinkButton>
            
        </div>

        <div class="rd-bar">
            <div class="rd-titleblock">
                <div class="rd-title">Recently Deleted</div>
                <div class="rd-sub">Shows only items deleted within the last 30 days. </div>
            </div>

            <div style="width:180px"></div>

            <asp:TextBox ID="txtSearch" runat="server" CssClass="rd-in" placeholder="Search by ID or title..." />
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="rd-btn" OnClick="btnSearch_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="rd-btn" OnClick="btnClear_Click" />

   

        </div>

        <div class="rd-card">
            <div class="rd-card-h">Deleted Subtopics</div>

            <asp:GridView ID="gvSubtopics" runat="server"
                AutoGenerateColumns="false"
                CssClass="rd-grid"
                GridLines="None"
                OnRowCommand="gvSubtopics_RowCommand"
                EmptyDataText="No deleted subtopics found.">
                <Columns>
                    <asp:BoundField HeaderText="Subtopic ID" DataField="subtopicID" />
                    <asp:BoundField HeaderText="Title" DataField="title" />
                    <asp:BoundField HeaderText="Deleted At" DataField="deleted_at" DataFormatString="{0:yyyy-MM-dd HH:mm}" />

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:LinkButton runat="server"
                                    Text="Restore"
                                    CssClass="rd-link"
                                    CausesValidation="false"
                                    CommandName="RESTORE_SUB"
                                    CommandArgument='<%# Eval("subtopicID") %>'
                                    OnClientClick="return confirm('Are you sure you want to restore this subtopic?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="rd-card">
            <div class="rd-card-h">Deleted Practice Questions</div>

            <asp:GridView ID="gvPQ" runat="server"
                AutoGenerateColumns="false"
                CssClass="rd-grid"
                GridLines="None"
                OnRowCommand="gvPQ_RowCommand"
                EmptyDataText="No deleted practice questions found.">
                <Columns>
                    <asp:BoundField HeaderText="Subtopic ID" DataField="subtopicID" />
                    <asp:BoundField HeaderText="Question" DataField="question" />
                    <asp:BoundField HeaderText="Deleted At" DataField="deleted_at" DataFormatString="{0:yyyy-MM-dd HH:mm}" />

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:LinkButton runat="server"
                                    Text="Restore"
                                    CssClass="rd-link"
                                    CausesValidation="false"
                                    CommandName="RESTORE_PQ"
                                    CommandArgument='<%# Eval("subtopicID") %>'
                                    OnClientClick="return confirm('Are you sure you want to restore this practice question?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="rd-card">
            <div class="rd-card-h">Deleted Self Assessments</div>

            <asp:GridView ID="gvSA" runat="server"
                AutoGenerateColumns="false"
                CssClass="rd-grid"
                GridLines="None"
                OnRowCommand="gvSA_RowCommand"
                EmptyDataText="No deleted self assessments found.">
                <Columns>
                    <asp:BoundField HeaderText="AssID" DataField="assID" />
                    <asp:BoundField HeaderText="Question" DataField="question" />
                    <asp:BoundField HeaderText="Deleted At" DataField="deleted_at" DataFormatString="{0:yyyy-MM-dd HH:mm}" />

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:LinkButton runat="server"
                                    Text="Restore"
                                    CssClass="rd-link"
                                    CausesValidation="false"
                                    CommandName="RESTORE_SA"
                                    CommandArgument='<%# Eval("assID") %>'
                                    OnClientClick="return confirm('Are you sure you want to restore this self assessment question?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

    </div>

</asp:Content>