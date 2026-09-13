<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="StudentDashboard.aspx.cs" Inherits="WAPP_Asm.StudentDashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <link href="<%= ResolveUrl("~/Asm_StyleSheet/StudentDashboardStyle.css?v=" + DateTime.Now.Ticks) %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Asm_StyleSheet/JourneyStyle.css?v=" + DateTime.Now.Ticks) %>" rel="stylesheet" />

    <div class="dash-shell">

        <!-- ---------- STUDY TAB ---------- -->
        <section class="dash-tab-panel" id="tab-study">

            <div class="hero-clean">
                <h2 class="hero-clean-title">
                        <svg class="journey-code-symbol" viewBox="0 0 24 24" width="32" height="32" fill="none" stroke="#2563eb" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false" style="width:.95em;height:.95em;vertical-align:-.1em;margin-right:.22em;display:inline-block;"><path d="M7 6 1 12l6 6M17 6l6 6-6 6M14 4l-4 16" /></svg>
                        Start your <span>Python</span> journey
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

                <asp:Panel ID="pnlOverall" runat="server" Visible="true">
                    <div class="journey-card">
                        <div class="journey-head">
                            <div class="journey-title">🎮 Your Learning Journey</div>
                            <div class="journey-percent">
                                <asp:Literal ID="litOverallPercent" runat="server" />
                            </div>
                        </div>

                        <div class="journey-stats">
                            <span class="xp-pill">⭐ <span id="totalXpDisplay">0</span> XP</span>
                            <div class="badge-shelf" id="badgeShelf"></div>
                        </div>

                        <div class="journey-track" id="journeyTrack" data-progress="<%= OverallProgressPercent %>">
                            <div class="track-line"></div>
                            <div class="track-fill" style='<%= "width:" + OverallProgressPercent + "%;" %>'></div>

                            <div class="track-node start" style="left:0%">
                                <div class="node-dot">🚩</div>
                            </div>
                            <div class="track-node milestone" data-percent="20" style="left:20%">
                                <div class="node-dot">🎁</div>
                            </div>
                            <div class="track-node milestone" data-percent="40" style="left:40%">
                                <div class="node-dot">🎁</div>
                            </div>
                            <div class="track-node milestone" data-percent="60" style="left:60%">
                                <div class="node-dot">🎁</div>
                            </div>
                            <div class="track-node milestone" data-percent="80" style="left:80%">
                                <div class="node-dot">🎁</div>
                            </div>
                            <div class="track-node finish" style="left:100%">
                                <div class="node-dot">🏆</div>
                            </div>

                            <div class="journey-character" id="journeyChar" style='<%= "left:" + OverallProgressPercent + "%;" %>'>
                                🧑‍💻
                            </div>
                        </div>

                        <div class="journey-legend">
                            <span><i class="dot done"></i> Completed</span>
                            <span><i class="dot pend"></i> Pending</span>
                            <span class="legend-hint">🎁 = claim a badge, XP, and a bonus quiz</span>
                        </div>
                    </div>
                </asp:Panel>

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

                <!-- ===== Bonus Quizzes (unlocked by journey milestones) ===== -->
                <asp:Panel ID="pnlBonusQuizzes" runat="server" Visible="true">
                    <div class="bonus-quiz-section">
                        <div class="bonus-quiz-head">
                            <div class="bonus-quiz-title">🎁 Bonus Quizzes</div>
                            <div class="bonus-quiz-sub">Unlocked as you hit milestones on your journey above</div>
                        </div>
                        <div class="bonus-quiz-grid" id="bonusQuizGrid"></div>
                    </div>
                </asp:Panel>

            </section>

    </div>

    <!-- ===== Milestone Reward Reveal Modal (global overlay, outside the tabs) ===== -->
    <div class="reward-modal-overlay" id="rewardOverlay">
        <div class="reward-modal">
            <button type="button" class="reward-close" onclick="closeRewardModal()">✕</button>
            <div class="reward-badge-icon" id="rewardBadgeIcon">🏅</div>
            <h3 class="reward-modal-title">Milestone Reached!</h3>
            <div class="reward-badge-name" id="rewardBadgeName"></div>
            <div class="reward-xp" id="rewardXp"></div>
            <div class="reward-quiz-hint" id="rewardQuizHint"></div>

            <div class="reward-modal-actions">
                <button type="button" class="btn-solid" id="rewardTakeQuizBtn">Take Bonus Quiz</button>
                <button type="button" class="btn-ghost" onclick="closeRewardModal()">Close</button>
            </div>
        </div>
    </div>

    <!-- ===== Bonus Quiz Modal ===== -->
    <div class="quiz-modal-overlay" id="quizOverlay">
        <div class="quiz-modal">
            <button type="button" class="quiz-close" onclick="closeQuizModal()">✕</button>
            <h3 class="quiz-modal-title" id="quizModalTitle">Bonus Quiz</h3>

            <div class="quiz-questions" id="quizQuestions"></div>

            <button type="button" class="btn-solid" id="quizSubmitBtn">Submit Quiz</button>
            <div class="quiz-score" id="quizScore"></div>
        </div>
    </div>


    <script>
        // ===== Journey milestones: badges + XP + bonus quizzes (backed by StudentDashboard.aspx?ajax=... ) =====
        (function () {
            var track = document.getElementById("journeyTrack");
            var xpDisplay = document.getElementById("totalXpDisplay");
            var badgeShelf = document.getElementById("badgeShelf");
            var bonusGrid = document.getElementById("bonusQuizGrid");

            var rewardOverlay = document.getElementById("rewardOverlay");
            var badgeIconEl = document.getElementById("rewardBadgeIcon");
            var badgeNameEl = document.getElementById("rewardBadgeName");
            var xpEl = document.getElementById("rewardXp");
            var quizHintEl = document.getElementById("rewardQuizHint");
            var takeQuizBtn = document.getElementById("rewardTakeQuizBtn");

            var milestones = [20, 40, 60, 80];
            var claimedData = {}; // percent -> { badgeName, badgeIcon, xpAwarded }

            function renderBadgeShelf() {
                badgeShelf.innerHTML = "";
                milestones.forEach(function (p) {
                    if (claimedData[p]) {
                        var span = document.createElement("span");
                        span.className = "badge-chip";
                        span.title = claimedData[p].badgeName;
                        span.textContent = claimedData[p].badgeIcon;
                        badgeShelf.appendChild(span);
                    }
                });
            }

            function renderBonusQuizGrid() {
                bonusGrid.innerHTML = "";
                milestones.forEach(function (p) {
                    var unlocked = !!claimedData[p];
                    var card = document.createElement("div");
                    card.className = "bonus-quiz-card " + (unlocked ? "unlocked" : "locked");

                    card.innerHTML =
                        '<div class="bq-milestone">' + p + '% Milestone</div>' +
                        (unlocked
                            ? '<div class="bq-badge">' + claimedData[p].badgeIcon + ' ' + claimedData[p].badgeName + '</div>' +
                              '<button type="button" class="btn-solid bq-btn">Take Quiz</button>'
                            : '<div class="bq-locked-text">🔒 Reach ' + p + '% and claim the reward to unlock</div>');

                    if (unlocked) {
                        card.querySelector(".bq-btn").onclick = function () { openBonusQuiz(p); };
                    }

                    bonusGrid.appendChild(card);
                });
            }

            function renderMilestoneNodes(progress) {
                var nodes = track.querySelectorAll(".track-node.milestone");
                nodes.forEach(function (node) {
                    var p = parseInt(node.getAttribute("data-percent"), 10);
                    var dot = node.querySelector(".node-dot");
                    node.classList.remove("locked", "unclaimed", "claimed");

                    if (claimedData[p]) {
                        node.classList.add("claimed");
                        dot.textContent = claimedData[p].badgeIcon;
                        node.onclick = function () { openRewardModal(claimedData[p], p); };
                    } else if (progress >= p) {
                        node.classList.add("unclaimed");
                        dot.textContent = "🎁";
                        node.onclick = function () { claimMilestone(p); };
                    } else {
                        node.classList.add("locked");
                        dot.textContent = "🔒";
                        node.onclick = null;
                    }
                });
            }

            function loadClaimedRewards() {
                if (!track) return;
                var progress = parseInt(track.getAttribute("data-progress"), 10) || 0;

                fetch("StudentDashboard.aspx?ajax=list", { credentials: "same-origin" })
                    .then(function (res) { return res.ok ? res.json() : { rewards: [], totalXp: 0 }; })
                    .then(function (data) {
                        claimedData = {};
                        (data.rewards || []).forEach(function (r) { claimedData[r.percent] = r; });

                        xpDisplay.textContent = data.totalXp || 0;
                        renderBadgeShelf();
                        renderBonusQuizGrid();
                        renderMilestoneNodes(progress);
                    })
                    .catch(function () { /* reward system is a bonus feature — fail quietly */ });
            }

            function claimMilestone(percent) {
                fetch("StudentDashboard.aspx?ajax=claim", {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    credentials: "same-origin",
                    body: "percent=" + encodeURIComponent(percent)
                })
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        if (data.error) return;

                        claimedData[percent] = {
                            badgeName: data.badgeName,
                            badgeIcon: data.badgeIcon,
                            xpAwarded: data.xpAwarded
                        };

                        if (!data.alreadyClaimed) {
                            var totalXp = parseInt(xpDisplay.textContent, 10) || 0;
                            xpDisplay.textContent = totalXp + data.xpAwarded;
                        }

                        renderBadgeShelf();
                        renderBonusQuizGrid();

                        var progress = parseInt(track.getAttribute("data-progress"), 10) || 0;
                        renderMilestoneNodes(progress);

                        openRewardModal(claimedData[percent], percent);
                    })
                    .catch(function () { /* ignore — reward stays claimable, they can retry */ });
            }

            function openRewardModal(reward, percent) {
                badgeIconEl.textContent = reward.badgeIcon;
                badgeNameEl.textContent = reward.badgeName;
                xpEl.textContent = "+" + reward.xpAwarded + " XP";
                quizHintEl.textContent = "🎁 A bonus quiz for this milestone is unlocked!";
                takeQuizBtn.style.display = "inline-block";
                takeQuizBtn.onclick = function () {
                    closeRewardModal();
                    openBonusQuiz(percent);
                };
                rewardOverlay.classList.add("open");
            }

            window.closeRewardModal = function () {
                rewardOverlay.classList.remove("open");
            };

            document.addEventListener("DOMContentLoaded", loadClaimedRewards);
            loadClaimedRewards();
        })();
    </script>

    <script>
        // ===== Bonus quiz modal: fetch, answer, and grade a milestone's quiz =====
        (function () {
            var quizOverlay = document.getElementById("quizOverlay");
            var quizTitleEl = document.getElementById("quizModalTitle");
            var quizQuestionsEl = document.getElementById("quizQuestions");
            var quizSubmitBtn = document.getElementById("quizSubmitBtn");
            var quizScoreEl = document.getElementById("quizScore");
            var currentQuizPercent = null;

            function escapeHtml(s) {
                var div = document.createElement("div");
                div.textContent = s;
                return div.innerHTML;
            }

            function renderQuizQuestions(questions) {
                quizQuestionsEl.innerHTML = "";
                questions.forEach(function (q, idx) {
                    var block = document.createElement("div");
                    block.className = "quiz-question-block";
                    block.setAttribute("data-qid", q.questionID);

                    var html = '<div class="quiz-question-text">' + (idx + 1) + '. ' + escapeHtml(q.questionText) + '</div>';
                    ["A", "B", "C", "D"].forEach(function (letter) {
                        html += '<label class="quiz-option">' +
                            '<input type="radio" name="q_' + q.questionID + '" value="' + letter + '"> ' +
                            escapeHtml(q["option" + letter]) +
                            '</label>';
                    });

                    block.innerHTML = html;
                    quizQuestionsEl.appendChild(block);
                });
            }

            window.openBonusQuiz = function (percent) {
                currentQuizPercent = percent;
                quizTitleEl.textContent = "🎁 Bonus Quiz — " + percent + "% Milestone";
                quizQuestionsEl.innerHTML = "<p>Loading...</p>";
                quizScoreEl.textContent = "";
                quizSubmitBtn.style.display = "inline-block";
                quizSubmitBtn.disabled = false;
                quizOverlay.classList.add("open");

                fetch("StudentDashboard.aspx?ajax=bonusquiz&percent=" + percent, { credentials: "same-origin" })
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        if (data.error) {
                            quizQuestionsEl.innerHTML = "<p>Couldn't load this quiz. Please try again.</p>";
                            quizSubmitBtn.style.display = "none";
                            return;
                        }
                        renderQuizQuestions(data.questions || []);
                    })
                    .catch(function () {
                        quizQuestionsEl.innerHTML = "<p>Network error — please try again.</p>";
                        quizSubmitBtn.style.display = "none";
                    });
            };

            window.closeQuizModal = function () {
                quizOverlay.classList.remove("open");
            };

            document.getElementById("quizSubmitBtn").onclick = function () {
                var blocks = quizQuestionsEl.querySelectorAll(".quiz-question-block");
                var answers = [];

                blocks.forEach(function (block) {
                    var qid = block.getAttribute("data-qid");
                    var checked = block.querySelector('input[type="radio"]:checked');
                    answers.push({ questionID: parseInt(qid, 10), selected: checked ? checked.value : "" });
                });

                quizSubmitBtn.disabled = true;

                fetch("StudentDashboard.aspx?ajax=bonusquizsubmit", {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    credentials: "same-origin",
                    body: "percent=" + encodeURIComponent(currentQuizPercent) + "&answers=" + encodeURIComponent(JSON.stringify(answers))
                })
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        if (data.error) {
                            quizScoreEl.textContent = "Couldn't submit — please try again.";
                            quizSubmitBtn.disabled = false;
                            return;
                        }

                        (data.results || []).forEach(function (r) {
                            var block = quizQuestionsEl.querySelector('.quiz-question-block[data-qid="' + r.questionID + '"]');
                            if (!block) return;

                            var radios = block.querySelectorAll('input[type="radio"]');
                            radios.forEach(function (radio) {
                                radio.disabled = true;
                                var label = radio.closest("label");
                                if (radio.value === r.correctOption) {
                                    label.classList.add("correct");
                                } else if (radio.checked) {
                                    label.classList.add("wrong");
                                }
                            });
                        });

                        quizScoreEl.textContent = "Score: " + data.score + " / " + data.total;
                        quizSubmitBtn.style.display = "none";
                    })
                    .catch(function () {
                        quizScoreEl.textContent = "Network error — please try again.";
                        quizSubmitBtn.disabled = false;
                    });
            };
        })();
    </script>


</asp:Content>
