<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HomePage.aspx.cs" Inherits="WAPP_Asm.Asm_WebPage.HomePage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>KeyCode - Home</title>
    <link href="<%= ResolveUrl("~/Asm_StyleSheet/HomeStyle.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="home-page-wrapper">
        <!-- Hero Section -->
        <section class="hero-section">
            <div class="blob" style="width: 400px; height: 400px; top: -100px; left: -100px;"></div>
            <div class="blob" style="width: 500px; height: 500px; bottom: -200px; right: -100px; background: #e3f2fd;"></div>

            <div class="container" style="position: relative; z-index: 1;">
                <div class="row align-items-center">

                    <!-- Text Content -->
                    <div class="col-lg-6 mb-5 mb-lg-0">
                        <h1 class="hero-title">
                            Unlock your potential with <span class="highlight">KEYCODE</span>
                        </h1>
                        <p class="hero-description fw-bold" style="color: var(--primary-dark-blue);">
                            A secure web-based platform specially designed for beginner Python learners.
                        </p>
                        <p class="hero-description">
                            KEYCODE offers structured learning materials, quizzes, assessments, and progress tracking.
                            It currently covers three core Python chapters in the English language to build your
                            foundational programming skills securely and efficiently.
                        </p>
                        <div class="d-flex gap-3 mt-4 align-items-center flex-wrap">
                            <a href="<%= ResolveUrl("~/Asm_WebPage/Login.aspx") %>" class="btn-login" style="padding: 12px 32px; font-size: 1rem;">Log In</a>
                            <asp:Button ID="btnGuest" runat="server"
                                Text="Continue as Guest"
                                CssClass="btn-guest"
                                OnClick="btnGuest_Click"
                                CausesValidation="false"
                                style="padding: 12px 32px; font-size: 1rem;" />
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
    <pre style="margin: 0;">
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

        <!-- Features Section -->
        <section id="features" class="features-section">
            <div class="container">
                <div class="text-center mb-5">
                    <h2 style="font-weight: 800; color: var(--primary-dark-blue);">Everything You Need to Succeed</h2>
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

        <!-- Footer -->
        <footer class="home-footer">
            <div class="container">
                <h4 class="footer-brand">KEYCODE</h4>
                <p class="footer-desc">A secure web-based platform for beginner Python learners.</p>
                <div class="footer-copy">&copy; 2026 KEYCODE Platform. All rights reserved.</div>
            </div>
        </footer>
    </div>
</asp:Content>