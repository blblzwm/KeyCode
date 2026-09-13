<%@ Page Title="Python Runner" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MiniGame.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.BattleGame" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Asm_StyleSheet/MiniGameStyle.css?v=runner-1") %>" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <section id="runnerGame" class="rg" data-guest='<%= IsGuestForPage ? "true" : "false" %>' data-game-url='<%= ResolveUrl("~/Asm_WebPage/BattleGame.aspx") %>'>
        <header class="rg-heading">
            <div>
                <span class="rg-eyebrow">&lt;/&gt; KEYCODE ARCADE</span>
                <h1 class="rg-title">Think. Jump. <span>Repeat.</span></h1>
                <p class="rg-subtitle">A little Python. A perfect jump. How far can you go?</p>
            </div>
            <div class="rg-best"><span>BEST IN THIS TAB</span><strong id="rgBest">0</strong><small>points</small></div>
        </header>

        <div class="rg-layout">
            <section class="rg-run-card" aria-label="Runner game">
                <div class="rg-scoreboard">
                    <div><span class="rg-micro">CURRENT SCORE</span><strong id="rgScore">000</strong></div>
                    <div class="rg-score-detail"><span id="rgCleared">0 obstacles cleared</span><span id="rgGuestInfo" hidden></span></div>
                    <button type="button" id="rgMotion" class="rg-icon-button" aria-pressed="false" title="Reduce motion">Reduce motion</button>
                </div>
                <div id="rgScene" class="rg-scene" aria-hidden="true">
                    <div class="rg-sun"></div>
                    <div class="rg-cloud rg-cloud-one"></div><div class="rg-cloud rg-cloud-two"></div>
                    <div class="rg-skyline rg-skyline-back"></div><div class="rg-skyline rg-skyline-front"></div>
                    <span class="rg-scene-caption" id="rgSceneCaption">READY WHEN YOU ARE</span>
                    <div id="rgPlayer" class="rg-player">
                        <!-- The approved original PNG is unchanged. This SVG clip hides
                             its baked checkerboard at render time, without redrawing the avatar. -->
                        <svg id="rgAvatar" viewBox="180 88 1160 855" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" focusable="false">
                            <defs><clipPath id="rgAvatarClip" clipPathUnits="userSpaceOnUse">
                                <path d="M558 115H976Q995 115 1012 130L1082 194Q1096 207 1100 230L1126 694Q1128 724 1101 729H424Q396 727 401 695L427 234Q429 213 443 199L531 125Q542 115 558 115Z" />
                                <path d="M411 467C350 479 297 512 270 565L260 590L278 599L298 558C326 519 360 502 411 488Z" />
                                <ellipse cx="266" cy="607" rx="51" ry="51" />
                                <path d="M1118 520C1173 521 1205 497 1226 464L1243 476C1214 526 1171 548 1120 548Z" />
                                <ellipse cx="1253" cy="422" rx="51" ry="51" />
                                <path d="M567 729L522 784L493 802L512 823L544 803L601 733Z" />
                                <ellipse cx="440" cy="841" rx="88" ry="46" transform="rotate(33 440 841)" />
                                <path d="M937 728L996 810L1021 801L969 729Z" />
                                <ellipse cx="1104" cy="856" rx="96" ry="46" transform="rotate(-27 1104 856)" />
                            </clipPath></defs>
                            <image id="rgAvatarImage" width="1536" height="1024" x="0" y="0" xlink:href="<%= ResolveUrl("~/Images/keycap-original.png") %>" clip-path="url(#rgAvatarClip)" />
                        </svg>
                    </div>
                    <div id="rgObstacle" class="rg-obstacle"><span>&lt;!&gt;</span></div>
                    <span id="rgPointsPop" class="rg-points-pop" hidden>+10</span>
                    <div class="rg-ground"><div id="rgTrack" class="rg-track"></div></div>
                </div>
                <div class="rg-scene-footer">
                    <span><i class="rg-status-dot"></i><span id="rgStateLabel">Ready to run</span></span>
                    <span>1 correct answer = 1 jump</span>
                </div>
                <div class="rg-run-controls">
                    <button type="button" id="rgStart" class="rg-primary">Start running <span aria-hidden="true">&#8594;</span></button>
                    <button type="button" id="rgPause" class="rg-secondary" disabled>Pause</button>
                </div>
                <div class="rg-how">
                    <span><b>01</b> Read the code</span><span><b>02</b> Pick its output</span><span><b>03</b> Clear the obstacle</span>
                </div>
            </section>

            <section class="rg-question-card" aria-labelledby="rgQuestionTitle">
                <div class="rg-question-top"><span class="rg-eyebrow">THE NEXT JUMP</span><span id="rgQuestionNumber" class="rg-badge">Python basics</span></div>
                <h2 id="rgQuestionTitle">Your answer is the jump button.</h2>
                <p id="rgQuestionHint" class="rg-question-hint">Start a run to reveal your first question. There is no time limit.</p>
                <div class="rg-code-window"><div class="rg-code-bar"><span><i></i><i></i><i></i></span><span>runner.py</span><span>PYTHON 3</span></div><pre><code id="rgCode"># Ready to take the first leap?
print("Let's run!")</code></pre></div>
                <div id="rgOptions" class="rg-options" role="group" aria-labelledby="rgQuestionTitle"></div>
                <div id="rgFeedback" class="rg-feedback" role="status" aria-live="polite">Correct answers earn 10 points. One wrong answer ends the run.</div>
                <button type="button" id="rgNext" class="rg-primary rg-next" hidden>Next obstacle <span aria-hidden="true">&#8594;</span></button>
                <p class="rg-keyboard">Use <kbd>1</kbd>–<kbd>4</kbd> to answer, or tap an option.</p>
            </section>
        </div>
        <p class="rg-note">Practice for fun. Runner points do not change your course progress or assessment marks.</p>
        <noscript><p class="rg-feedback">Please enable JavaScript to play Python Runner.</p></noscript>

        <dialog id="rgResult" class="rg-result" aria-labelledby="rgResultTitle" aria-describedby="rgResultMessage">
            <button type="button" id="rgCloseResult" class="rg-close" aria-label="Close score panel">&#215;</button>
            <div class="rg-result-symbol" aria-hidden="true">&lt;/&gt;</div>
            <span class="rg-eyebrow">PYTHON RUNNER</span>
            <h2 id="rgResultTitle">Game over</h2>
            <p id="rgResultMessage">A small stumble. Another chance to learn.</p>
            <div class="rg-final-score"><strong id="rgFinalScore">0</strong><span>POINTS THIS RUN</span></div>
            <div class="rg-result-stats"><span><b id="rgFinalCleared">0</b> obstacles cleared</span><span><b id="rgFinalBest">0</b> best in this tab</span></div>
            <div id="rgAnswerReview" class="rg-review" hidden><strong>The correct output</strong><pre id="rgCorrectOutput"></pre><p id="rgExplanation"></p></div>
            <div class="rg-result-actions"><button type="button" id="rgReplay" class="rg-primary">Play again &#8594;</button><a id="rgLogin" class="rg-primary" href="<%= ResolveUrl("~/Asm_WebPage/Login.aspx") %>" hidden>Log in to keep running</a><button type="button" id="rgReview" class="rg-secondary">Review my answer</button></div>
        </dialog>
    </section>
    <script src="<%= ResolveUrl("~/Asm_Scripts/MiniGameQuestions.js?v=runner-1") %>"></script>
    <script src="<%= ResolveUrl("~/Asm_Scripts/PythonRunner.js?v=runner-1") %>"></script>
</asp:Content>
