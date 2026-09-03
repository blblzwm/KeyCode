<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Analytics.aspx.cs"
    Inherits="WAPP_Asm.Asm_WebPage.Analytics" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <link href="../Asm_StyleSheet/AnalyticsStyle.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="container-fluid" style="max-width:1400px; padding:24px;">

    <asp:Button ID="btnBack" runat="server"
        OnClick="btnBack_Click"
        CssClass="btn-back"
        Text="« Back to Dashboard"
        CausesValidation="false" />

    <h1 style="margin:16px 0 24px; font-weight:800;">Analytics</h1>

    <!-- TAB BAR -->
    <div class="tab-bar">
        <button type="button" class="tab-btn active" onclick="switchTab('performance', this)">Performance</button>
        <button type="button" class="tab-btn"        onclick="switchTab('forum', this)">Forum</button>
    </div>

    <!-- PERFORMANCE-->
    <div id="tab-performance" class="tab-content active">

        <div class="row mb-2">
            <div class="col-md-6">
                <div class="summary-card">
                    <div class="value"><asp:Literal ID="litTotalStudents" runat="server" Text="0" /></div>
                    <div class="label">Students Attempted</div>
                </div>
            </div>

            <div class="col-md-6">
                <div class="summary-card">
                    <div class="value"><asp:Literal ID="litPassRate" runat="server" Text="0" />%</div>
                    <div class="label">Chapter Pass Rate (≥60%)</div>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-md-7">
                <div class="analytics-card">
                    <div class="chart-title">Average Score per Chapter</div>
                    <canvas id="chartAvgScore" height="120"></canvas>
                </div>
            </div>
            <div class="col-md-5">
                <div class="analytics-card">
                    <div class="chart-title">Score Distribution</div>
                    <canvas id="chartDistribution" height="120"></canvas>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <div class="analytics-card">
                    <div class="chart-title">Pass vs Fail</div>
                    <canvas id="chartPassFail" height="180"></canvas>
                </div>
            </div>
            <div class="col-md-8">
                <div class="analytics-card">
                    <div class="chart-title">Top 5 Students by Avg Score</div>
                    <canvas id="chartTopStudents" height="180"></canvas>
                </div>
            </div>
        </div>

    </div>

    <!-- FORUM TAB-->
    <div id="tab-forum" class="tab-content">

        <div class="row mb-2">
            <div class="col-md-3">
                <div class="summary-card">
                    <div class="value"><asp:Literal ID="litTotalPosts" runat="server" Text="0" /></div>
                    <div class="label">Total Posts</div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="summary-card">
                    <div class="value"><asp:Literal ID="litTotalComments" runat="server" Text="0" /></div>
                    <div class="label">Total Comments</div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="summary-card">
                    <div class="value"><asp:Literal ID="litAnnouncements" runat="server" Text="0" /></div>
                    <div class="label">Announcements</div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="summary-card">
                    <div class="value"><asp:Literal ID="litActiveUsers" runat="server" Text="0" /></div>
                    <div class="label">Active Users</div>
                </div>
            </div>
        </div>

        <div class="analytics-card">
            <div class="chart-title">Forum Activity Over Time</div>
            <canvas id="chartActivity" height="80"></canvas>
        </div>

        <div class="row">
            <div class="col-md-4">
                <div class="analytics-card">
                    <div class="chart-title">Posts by Role</div>
                    <canvas id="chartPostsByRole" height="180"></canvas>
                </div>
            </div>
            <div class="col-md-8">
                <div class="analytics-card">
                    <div class="chart-title">Top 5 Most Active Users</div>
                    <canvas id="chartActiveUsers" height="180"></canvas>
                </div>
            </div>
        </div>

    </div>

</div>

<script>
    // tab switching logic
    function switchTab(tab, btn) {
        document.querySelectorAll('.tab-content').forEach(el => el.classList.remove('active'));
        document.querySelectorAll('.tab-btn').forEach(el => el.classList.remove('active'));
        document.getElementById('tab-' + tab).classList.add('active');
        btn.classList.add('active');
    }

    // assessment
    var barLabels = [<asp:Literal ID="litBarLabels" runat="server" />];
    var barScores = [<asp:Literal ID="litBarScores" runat="server" />];
    var distribution = [<asp:Literal ID="litDistribution" runat="server" />];
    var passFailData = [<asp:Literal ID="litPassFail" runat="server" />];
    var topNames = [<asp:Literal ID="litTopNames" runat="server" />];
    var topScores = [<asp:Literal ID="litTopScores" runat="server" />];

    // forum
    var lineLabels = [<asp:Literal ID="litLineLabels" runat="server" />];
    var linePosts = [<asp:Literal ID="litLinePosts" runat="server" />];
    var lineComments = [<asp:Literal ID="litLineComments" runat="server" />];
    var postsByRole = [<asp:Literal ID="litPostsByRole" runat="server" />];
    var activeNames = [<asp:Literal ID="litActiveNames" runat="server" />];
    var activeTotals = [<asp:Literal ID="litActiveTotals" runat="server" />];

    // performance charts
    new Chart(document.getElementById('chartAvgScore'), {
        type: 'bar',
        data: {
            labels: barLabels,
            datasets: [{
                label: 'Avg Score (%)', data: barScores,
                backgroundColor: 'rgba(99,102,241,0.8)', borderRadius: 6
            }]
        },
        options: {
            scales: {
                y: { beginAtZero: true, max: 100, ticks: { callback: v => v + '%' }, grid: { color: '#f0f0f0' } },
                x: { grid: { display: false } }
            },
            plugins: { legend: { display: false } }
        }
    });

    new Chart(document.getElementById('chartDistribution'), {
        type: 'bar',
        data: {
            labels: ['0–20', '21–40', '41–60', '61–80', '81–100'],
            datasets: [{
                data: distribution,
                backgroundColor: ['#ef4444', '#f97316', '#eab308', '#22c55e', '#6366f1'],
                borderRadius: 6
            }]
        },
        options: {
            scales: {
                y: { beginAtZero: true, ticks: { stepSize: 1 }, grid: { color: '#f0f0f0' } },
                x: { grid: { display: false } }
            },
            plugins: { legend: { display: false } }
        }
    });

    new Chart(document.getElementById('chartPassFail'), {
        type: 'doughnut',
        data: {
            labels: ['Pass (≥60%)', 'Fail (<60%)'],
            datasets: [{
                data: passFailData,
                backgroundColor: ['#22c55e', '#ef4444'],
                borderWidth: 2, borderColor: '#fff'
            }]
        },
        options: { plugins: { legend: { position: 'bottom' } } }
    });

    new Chart(document.getElementById('chartTopStudents'), {
        type: 'bar',
        data: {
            labels: topNames,
            datasets: [{
                label: 'Avg Score (%)', data: topScores,
                backgroundColor: 'rgba(16,185,129,0.8)', borderRadius: 6
            }]
        },
        options: {
            indexAxis: 'y',
            scales: {
                x: { beginAtZero: true, max: 100, ticks: { callback: v => v + '%' }, grid: { color: '#f0f0f0' } },
                y: { grid: { display: false } }
            },
            plugins: { legend: { display: false } }
        }
    });

    // forum charts
    new Chart(document.getElementById('chartActivity'), {
        type: 'line',
        data: {
            labels: lineLabels,
            datasets: [
                {
                    label: 'Posts', data: linePosts,
                    borderColor: '#6366f1', backgroundColor: 'rgba(99,102,241,0.1)',
                    tension: 0.4, fill: true, pointRadius: 4
                },
                {
                    label: 'Comments', data: lineComments,
                    borderColor: '#10b981', backgroundColor: 'rgba(16,185,129,0.1)',
                    tension: 0.4, fill: true, pointRadius: 4
                }
            ]
        },
        options: {
            scales: {
                y: { beginAtZero: true, ticks: { stepSize: 1 }, grid: { color: '#f0f0f0' } },
                x: { grid: { display: false } }
            },
            plugins: { legend: { position: 'top', labels: { usePointStyle: true } } }
        }
    });

    new Chart(document.getElementById('chartPostsByRole'), {
        type: 'doughnut',
        data: {
            labels: ['Students', 'Tutors'],
            datasets: [{
                data: postsByRole,
                backgroundColor: ['#6366f1', '#f59e0b'],
                borderWidth: 2, borderColor: '#fff'
            }]
        },
        options: { plugins: { legend: { position: 'bottom' } } }
    });

    new Chart(document.getElementById('chartActiveUsers'), {
        type: 'bar',
        data: {
            labels: activeNames,
            datasets: [{
                label: 'Posts + Comments', data: activeTotals,
                backgroundColor: 'rgba(245,158,11,0.8)', borderRadius: 6
            }]
        },
        options: {
            indexAxis: 'y',
            scales: {
                x: { beginAtZero: true, ticks: { stepSize: 1 }, grid: { color: '#f0f0f0' } },
                y: { grid: { display: false } }
            },
            plugins: { legend: { display: false } }
        }
    });
</script>
</asp:Content>
