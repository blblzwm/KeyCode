<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HomePage.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.HomePage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>KeyCode - Home</title>
    <link href="<%= ResolveUrl("~/Asm_StyleSheet/HomeStyle.css?v=" + DateTime.Now.Ticks) %>" rel="stylesheet" />
    <!-- Page-local banner styles also override an older HomeStyle.css. -->
    <style>
/* --- Pixel Title Banner --- */
#keycode-hero {
    position: relative;
    isolation: isolate;
    box-sizing: border-box;
    min-height: 580px;
    min-height: clamp(540px, 76svh, 760px);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 76px 24px;
    background: #07152f;
    color: #ffffff;
    text-align: center;
    overflow: hidden;
    container-type: inline-size;
}

/* Layered light and a drifting grid keep the navy base visible. */
#keycode-hero::before,
#keycode-hero::after {
    content: "";
    position: absolute;
    pointer-events: none;
    z-index: -1;
}

#keycode-hero::before {
    inset: -25%;
    background:
        radial-gradient(ellipse at 25% 30%, rgba(37, 99, 235, .38), transparent 45%),
        radial-gradient(ellipse at 80% 65%, rgba(14, 165, 233, .24), transparent 42%);
    animation: bannerAurora 16s ease-in-out infinite alternate;
}

#keycode-hero::after {
    inset: -48px;
    background-image:
        linear-gradient(rgba(147, 197, 253, .07) 1px, transparent 1px),
        linear-gradient(90deg, rgba(147, 197, 253, .07) 1px, transparent 1px);
    background-size: 48px 48px;
    mask-image: linear-gradient(transparent, #000 70%);
    animation: bannerGrid 14s linear infinite;
}

@keyframes bannerAurora {
    to { transform: translate(5%, -4%) rotate(8deg) scale(1.08); }
}

@keyframes bannerGrid {
    to { transform: translateY(48px); }
}

#keycode-hero .code-bg-particles {
    position: absolute;
    inset: 0;
    pointer-events: none;
    z-index: -1;
}

#keycode-hero .code-particle {
    position: absolute;
    top: 110%;
    font-family: 'Fira Code', monospace;
    font-weight: 700;
    font-size: 16px;
    color: rgba(186, 220, 255, .3);
    white-space: nowrap;
    animation-name: codeFloatUp;
    animation-timing-function: linear;
    animation-iteration-count: infinite;
}

@keyframes codeFloatUp {
    0% { top: 110%; transform: translateX(0); opacity: 0; }
    12%, 85% { opacity: 1; }
    100% { top: -10%; transform: translateX(-24px); opacity: 0; }
}

#keycode-hero .banner-content {
    position: relative;
    width: 100%;
    max-width: 1000px;
}

#keycode-hero .banner-eyebrow {
    margin: 0 0 30px;
    color: #b9d7ff;
    font-family: 'Orbitron', sans-serif;
    font-size: .8rem;
    font-weight: 700;
    letter-spacing: .2em;
    text-transform: uppercase;
}

#keycode-hero .pixel-title {
    font-family: 'Press Start 2P', 'Orbitron', monospace;
    font-size: clamp(1.35rem, 4.8vw, 5rem);
    font-size: clamp(1rem, 6.6cqi, 5rem);
    font-weight: 400;
    line-height: 1.4;
    color: #ffffff;
    letter-spacing: .08em;
    white-space: nowrap;
    margin: 0 0 44px;
    text-shadow: 4px 4px 0 #163963, 8px 8px 0 #020b1c, 0 0 44px rgba(125, 190, 255, .25);
}

/* Underline only the C; no underscore character or extra letter spacing. */
#keycode-hero .pixel-title .pixel-c-underline {
    position: relative;
    display: inline-block;
    border: 0;
    padding: 0;
    font: inherit;
    color: inherit;
    line-height: inherit;
}
#keycode-hero .pixel-title .pixel-c-underline::after {
    content: "";
    position: absolute;
    left: 0;
    right: .08em;
    bottom: -.06em;
    height: .075em;
    min-height: 3px;
    background: currentColor;
    box-shadow: 3px 3px 0 #163963;
}

#keycode-hero .banner-tagline {
    margin: 0 0 14px;
    color: #ffffff;
    font-family: 'Orbitron', sans-serif;
    font-size: clamp(1.5rem, 3vw, 2.6rem);
    font-weight: 800;
    line-height: 1.35;
}

#keycode-hero .banner-description {
    max-width: 620px;
    margin: 0 auto;
    color: #d7e8ff;
    font-family: 'Poppins', 'Segoe UI', sans-serif;
    font-weight: 500;
    font-size: clamp(1rem, 1.5vw, 1.2rem);
    line-height: 1.8;
}

#keycode-hero .banner-scroll {
    display: inline-flex;
    align-items: center;
    gap: 14px;
    min-height: 44px;
    margin-top: 36px;
    padding: 10px 20px;
    border: 1px solid rgba(186, 220, 255, .4);
    border-radius: 6px;
    background: #d9efff;
    color: #07152f;
    font-family: 'Poppins', 'Segoe UI', sans-serif;
    box-shadow: 0 8px 30px rgba(80, 170, 255, .2);
    font-size: .95rem;
    font-weight: 700;
    text-decoration: none;
    transition: background .2s, border-color .2s;
}

#keycode-hero .banner-scroll:hover {
    color: #ffffff;
    background: #193963;
    border-color: #b9d7ff;
}

#keycode-hero .banner-scroll:focus-visible {
    outline: 3px solid #ffffff;
    outline-offset: 5px;
}

#keycode-hero .banner-scroll-arrow {
    display: inline-block;
    animation: bannerArrow 2s ease-in-out infinite;
}

@keyframes bannerArrow {
    50% { transform: translateY(4px); }
}

#explore-keycode { scroll-margin-top: 90px; }

@media (max-width: 768px) {
    #keycode-hero {
        min-height: 520px;
        padding: 64px 20px;
    }
    #keycode-hero .pixel-title {
        text-shadow: 2px 2px 0 #163963, 4px 4px 0 #020b1c;
    }
    #keycode-hero .banner-eyebrow { font-size: .65rem; letter-spacing: .12em; }
    #keycode-hero .banner-description { max-width: 420px; }
}

@media (prefers-reduced-motion: reduce) {
    #keycode-hero::before,
    #keycode-hero::after,
    #keycode-hero .code-particle,
    #keycode-hero .banner-scroll-arrow { animation: none; }
    #keycode-hero .code-particle { top: 18%; opacity: .5; }
    #keycode-hero .code-particle:nth-child(even) { top: 82%; }
    #keycode-hero .banner-scroll { transition: none; }
}


#keycode-hero .banner-tagline .banner-accent { color: #8edcff; }
#keycode-hero .banner-eyebrow {
    display: inline-block;
    padding: 9px 16px;
    border: 1px solid rgba(185, 215, 255, .35);
    border-radius: 999px;
    background: rgba(21, 49, 91, .65);
}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="home-page-wrapper">
        <!-- Pixel Title Banner -->
        <div class="pixel-title-banner" id="keycode-hero">
            <div class="code-bg-particles" aria-hidden="true">
                <span class="code-particle" style="left:3%; animation-duration:14s; animation-delay:0s;">def</span>
                <span class="code-particle" style="left:11%; animation-duration:18s; animation-delay:-3s;">print()</span>
                <span class="code-particle" style="left:19%; animation-duration:16s; animation-delay:-7s;">0101</span>
                <span class="code-particle" style="left:28%; animation-duration:20s; animation-delay:-2s;">{ }</span>
                <span class="code-particle" style="left:37%; animation-duration:15s; animation-delay:-9s;">import</span>
                <span class="code-particle" style="left:47%; animation-duration:19s; animation-delay:-5s;">True</span>
                <span class="code-particle" style="left:56%; animation-duration:13s; animation-delay:-1s;">class</span>
                <span class="code-particle" style="left:65%; animation-duration:17s; animation-delay:-11s;">==</span>
                <span class="code-particle" style="left:74%; animation-duration:21s; animation-delay:-6s;">return</span>
                <span class="code-particle" style="left:83%; animation-duration:16s; animation-delay:-4s;">[::-1]</span>
                <span class="code-particle" style="left:91%; animation-duration:18s; animation-delay:-8s;">lambda</span>
                <span class="code-particle" style="left:97%; animation-duration:15s; animation-delay:-12s;">for i in</span>
            </div>
            <div class="banner-content">
                <p class="banner-eyebrow">Your Python journey starts here</p>
                <h1 class="pixel-title">KEY<span class="pixel-c-underline">C</span>ODE</h1>
                <p class="banner-tagline">Learn Python.<br /><span class="banner-accent">Unlock your potential.</span></p>
                <p class="banner-description">Go from your first line of code to real confidence.<br />Guided lessons, hands-on quizzes, and progress you can see.
                    <br />Made for beginners. Built around your pace.</p>
                <a class="banner-scroll" href="#explore-keycode">Scroll to explore <span class="banner-scroll-arrow" aria-hidden="true">&#8595;</span></a>
            </div>
        </div>

        <!-- Hero Section -->
        <section class="hero-section" id="explore-keycode">
            <div class="blob" style="width: 400px; height: 400px; top: -100px; left: -100px;"></div>
            <div class="blob" style="width: 500px; height: 500px; bottom: -200px; right: -100px; background: #e3f2fd;"></div>

            <div class="container" style="position: relative; z-index: 1;">
                <div class="row align-items-center">

                    <!-- Text Content -->
                    <div class="col-lg-6 mb-5 mb-lg-0">
                        <h1 class="hero-title">
                            Build your <span class="highlight">Python skills,</span> one step at a time
                        </h1>
                        <p class="hero-description fw-bold" style="color: var(--primary-dark-blue);">
                            A secure web-based platform specially designed for beginner Python learners.
                        </p>
                        <p class="hero-description">
                            KEYCODE offers structured learning materials, quizzes, assessments, and progress tracking.
                            It currently covers three core Python chapters in the English language to build your
                            foundational programming skills securely and efficiently.
                        </p>
                        <div class="hero-stats">
                            <div class="hero-stat">
                                <div class="hero-stat-num">3</div>
                                <div class="hero-stat-label">Core Chapters</div>
                            </div>
                            <div class="hero-stat-divider"></div>
                            <div class="hero-stat">
                                <div class="hero-stat-num">100%</div>
                                <div class="hero-stat-label">Free to Start</div>
                            </div>
                            <div class="hero-stat-divider"></div>
                            <div class="hero-stat">
                                <div class="hero-stat-num">24/7</div>
                                <div class="hero-stat-label">Self-Paced</div>
                            </div>
                        </div>
                    </div>

                    <!-- Mock Code Editor -->
                    <div class="col-lg-6">
                        <div class="code-editor-container">
                            <div class="editor-header">
                                <div class="dot dot-red"></div>
                                <div class="dot dot-yellow"></div>
                                <div class="dot dot-green"></div>
                                <div class="editor-title">learn_python.py</div>
                            </div>
                            <div class="editor-body">
    <pre style="margin: 0;" id="heroCodeBlock">
    <span class="syn-keyword">class</span> <span class="syn-class">KeyCodePlatform</span>:
        <span class="syn-keyword">def</span> <span class="syn-function">__init__</span>(<span class="syn-keyword">self</span>, student):
            <span class="syn-keyword">self</span>.student = student
            <span class="syn-keyword">self</span>.modules = [<span class="syn-string">"Variables"</span>, <span class="syn-string">"Loops"</span>, <span class="syn-string">"Functions"</span>]
            <span class="syn-keyword">self</span>.is_secure = <span class="syn-keyword">True</span>

        <span class="syn-keyword">def</span> <span class="syn-function">start_learning</span>(<span class="syn-keyword">self</span>):
            <span class="syn-comment"># Designed specially for beginners</span>
            <span class="syn-keyword">if</span> <span class="syn-keyword">self</span>.student.level == <span class="syn-string">"Beginner"</span>:
                <span class="syn-keyword">return</span> <span class="syn-string">"Welcome to KEYCODE!"</span>

        <span class="syn-keyword">def</span> <span class="syn-function">track_progress</span>(<span class="syn-keyword">self</span>, quiz_score):
            <span class="syn-keyword">if</span> quiz_score >= <span class="syn-string">80</span>:
                <span class="syn-keyword">print</span>(<span class="syn-string">"Assessment passed! Next chapter."</span>)
    </pre>
                            </div>
                        </div>
                    </div>

                </div>
            </div>
        </section>

        <!-- How It Works Section -->
        <section id="how-it-works" class="how-section">
            <div class="container">
                <div class="text-center mb-5">
                    <h2 style="font-weight: 800; color: var(--primary-dark-blue);">How KEYCODE Works</h2>
                    <p class="text-secondary mt-2">Three simple steps from zero to your first working Python program.</p>
                </div>
                <div class="row g-4">
                    <div class="col-md-4">
                        <div class="how-step">
                            <div class="how-step-num">1</div>
                            <h3 class="how-step-title">Log In</h3>
                            <p class="how-step-text">Create your account and pick up right where you left off, on any device.</p>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="how-step">
                            <div class="how-step-num">2</div>
                            <h3 class="how-step-title">Learn & Practice</h3>
                            <p class="how-step-text">Work through structured chapters, practice questions, and quizzes at your own pace.</p>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="how-step">
                            <div class="how-step-num">3</div>
                            <h3 class="how-step-title">Track & Earn Badges</h3>
                            <p class="how-step-text">Watch your progress bar fill up and unlock badges and bonus quizzes along the way.</p>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- Features Section -->
        <section id="features" class="features-section">
            <div class="container">
                <div class="text-center mb-5">
                    <h2 style="font-weight: 900; color: var(--primary-dark-blue);">Everything You Need to Succeed</h2>
                    <p class="text-secondary mt-2">Built-in tools to guide you from your first line of code to confidence.</p>
                </div>
                <div class="row g-4">
                    <div class="col-md-3">
                        <div class="feature-card">
                            <div class="feature-icon">📚</div>
                            <h3 class="feature-title">Structured Materials</h3>
                            <p class="feature-text">Step-by-step learning paths designed specifically for absolute beginners. No overwhelming jargon, just clear concepts.</p>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="feature-card">
                            <div class="feature-icon">📝</div>
                            <h3 class="feature-title">Quizzes & Assessments</h3>
                            <p class="feature-text">Test your knowledge immediately after learning. Our assessments ensure you truly understand the syntax and logic.</p>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="feature-card">
                            <div class="feature-icon">📈</div>
                            <h3 class="feature-title">Progress Tracking</h3>
                            <p class="feature-text">Visualize your learning journey. Watch your skills grow as you complete modules and conquer challenging quizzes.</p>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="feature-card">
                            <div class="feature-icon">🎯</div>
                            <h3 class="feature-title">3 Core Chapters</h3>
                            <p class="feature-text">Master the essentials. We cover the three most vital foundational chapters of Python to kickstart your coding journey.</p>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- Closing CTA Section -->
        <section class="closing-cta-section">
            <div class="container">
                <div class="closing-cta-banner">
                    <div>
                        <h2 class="closing-cta-title">Ready to write your first line of code?</h2>
                        <p class="closing-cta-desc">Jump in now — no experience needed, no cost to get started.</p>
                    </div>
                    <% if (IsGuestForHome) { %>
                    <a href="<%= ResolveUrl("~/Asm_WebPage/Login.aspx") %>" class="btn-login closing-cta-btn">Log In</a>
                    <% } else { %>
                    <a href="<%= ResolveUrl("~/Asm_WebPage/StudentDashboard.aspx") %>" class="btn-login closing-cta-btn">Go to Dashboard</a>
                    <% } %>
                </div>
            </div>
        </section>

        <!-- Footer -->
        <footer class="home-footer">
            <div class="container">
                <h4 class="footer-brand">KEYCODE</h4>
                <p class="footer-desc">A secure web-based platform for beginner Python learners.</p>
                <div class="footer-copy">&copy; 2026 KEYCODE Platform. All rights reserved.</div>
            </div>
        </footer>
    </div>

    <script>
        // Subtle typewriter effect for the mock code editor — types out the
        // existing highlighted markup character-by-character, preserving spans.
        (function () {
            function typeOutCode(rootEl, msPerChar) {
                if (!rootEl) return;
                var source = rootEl.cloneNode(true);
                rootEl.innerHTML = "";

                var ops = [];
                function buildOps(node) {
                    if (node.nodeType === Node.TEXT_NODE) {
                        var text = node.textContent;
                        for (var i = 0; i < text.length; i++) {
                            ops.push({ type: "char", ch: text[i] });
                        }
                    } else if (node.nodeType === Node.ELEMENT_NODE) {
                        ops.push({ type: "open", tag: node.tagName, className: node.className });
                        for (var j = 0; j < node.childNodes.length; j++) {
                            buildOps(node.childNodes[j]);
                        }
                        ops.push({ type: "close" });
                    }
                }
                for (var k = 0; k < source.childNodes.length; k++) {
                    buildOps(source.childNodes[k]);
                }

                var cursor = document.createElement("span");
                cursor.className = "typing-cursor";
                cursor.textContent = "\u2588";

                var stack = [rootEl];
                var idx = 0;

                function step() {
                    if (idx >= ops.length) {
                        rootEl.appendChild(cursor);
                        return;
                    }
                    var op = ops[idx++];
                    var current = stack[stack.length - 1];

                    if (op.type === "open") {
                        var el = document.createElement(op.tag);
                        if (op.className) el.className = op.className;
                        current.appendChild(el);
                        stack.push(el);
                    } else if (op.type === "close") {
                        if (stack.length > 1) stack.pop();
                    } else if (op.type === "char") {
                        if (current.lastChild && current.lastChild.nodeType === Node.TEXT_NODE) {
                            current.lastChild.textContent += op.ch;
                        } else {
                            current.appendChild(document.createTextNode(op.ch));
                        }
                    }

                    if (cursor.parentNode) cursor.parentNode.removeChild(cursor);
                    stack[stack.length - 1].appendChild(cursor);

                    setTimeout(step, msPerChar);
                }
                step();
            }

            document.addEventListener("DOMContentLoaded", function () {
                typeOutCode(document.getElementById("heroCodeBlock"), 6);
            });
        })();
    </script>
</asp:Content>