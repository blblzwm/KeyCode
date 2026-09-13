(function () {
    'use strict';
    const root = document.getElementById('runnerGame');
    if (!root) return;
    const el = id => document.getElementById('rg' + id);
    const bank = window.CodeQuestQuestions;
    let state = 'ready', score = 0, cleared = 0, used = 0, deck = [], question;
    let paused = false, elapsed = 0, last = 0, frame = 0, selected = -1;
    let reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const guest = root.dataset.guest === 'true';
    let best = 0;
    try { best = Number(sessionStorage.getItem('keycode.runner.best')) || 0; } catch (_) { /* Storage is optional. */ }
    const shuffle = items => {
        const result = items.slice();
        for (let i = result.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [result[i], result[j]] = [result[j], result[i]];
        }
        return result;
    };
    function scores() {
        el('Score').textContent = String(score).padStart(3, '0');
        el('Cleared').textContent = cleared + ' obstacles cleared';
        el('Best').textContent = best;
        el('GuestInfo').hidden = !guest;
        el('GuestInfo').textContent = Math.max(0, 5 - used) + ' guest answers left';
    }
    function feedback(text, kind) {
        el('Feedback').textContent = text;
        el('Feedback').className = 'rg-feedback' + (kind ? ' ' + kind : '');
    }
    function phase(next) {
        state = next; elapsed = 0; last = 0;
        cancelAnimationFrame(frame);
        if (['approach', 'jump', 'crash'].includes(next) && !paused) frame = requestAnimationFrame(tick);
    }
    function paint(t) {
        const width = el('Scene').clientWidth;
        const playerWidth = el('Player').offsetWidth;
        const hero = width * .15;
        const wait = Math.min(width - 45, hero + playerWidth + 45);
        let x = wait, y = 0;
        if (state === 'approach') { x = width + 40 + (wait - width - 40) * t; y = reduced ? 0 : -Math.abs(Math.sin(t * 22)) * 4; }
        if (state === 'jump') { x = wait + (-50 - wait) * t; y = reduced ? 0 : -Math.sin(Math.PI * t) * 140; }
        if (state === 'crash') x = wait + (hero + playerWidth * .55 - wait) * t;
        el('Obstacle').style.left = x + 'px';
        el('Player').style.transform = 'translateY(' + y + 'px)' + (state === 'crash' && t > .8 ? ' rotate(15deg)' : '');
        if (!reduced) el('Track').style.transform = 'translateX(' + (-t * 150) + 'px)';
    }
    function tick(now) {
        if (paused) return;
        if (last) elapsed += Math.min(now - last, 50);
        last = now;
        const duration = reduced ? 80 : state === 'jump' ? 1100 : state === 'crash' ? 400 : 950;
        const t = Math.min(1, elapsed / duration);
        paint(t);
        if (t < 1) { frame = requestAnimationFrame(tick); return; }
        if (state === 'approach') {
            phase('question');
            el('StateLabel').textContent = 'Choose an answer to jump';
            el('SceneCaption').textContent = 'THINK FIRST. THEN JUMP.';
            el('Options').querySelectorAll('button').forEach(b => b.disabled = false);
            el('Options').querySelector('button').focus();
        } else if (state === 'jump') {
            score += 10; cleared++; best = Math.max(best, score);
            try { sessionStorage.setItem('keycode.runner.best', best); } catch (_) { /* Optional. */ }
            scores();
            el('PointsPop').hidden = false;
            if (guest && used >= 5) finish(true);
            else {
                phase('feedback');
                el('Pause').disabled = true;
                el('StateLabel').textContent = 'Obstacle cleared!';
                feedback('Correct! +10 points. ' + question.explanation, 'good');
                el('Next').hidden = false; el('Next').focus();
            }
        } else if (state === 'crash') finish(false);
    }
    function nextQuestion() {
        if (!deck.length) deck = shuffle(bank);
        question = deck.pop(); selected = -1;
        el('Next').hidden = true; el('PointsPop').hidden = true;
        el('QuestionTitle').textContent = 'What does this Python code print?';
        el('QuestionNumber').textContent = 'Obstacle ' + (cleared + 1);
        el('QuestionHint').textContent = 'The obstacle waits for your answer. Take your time.';
        el('Code').textContent = question.code;
        el('Options').replaceChildren();
        shuffle(question.options.map((text, index) => ({ text, index }))).forEach((option, index) => {
            const button = document.createElement('button'); button.type = 'button';
            button.className = 'rg-option'; button.disabled = true;
            button.dataset.answer = option.index;
            const key = document.createElement('kbd'); key.textContent = index + 1;
            const label = document.createElement('span'); label.className = 'rg-option-text'; label.textContent = option.text;
            button.append(key, label); button.addEventListener('click', () => answer(option.index));
            el('Options').append(button);
        });
        feedback('One correct answer makes your keycap jump over this obstacle.');
        el('Pause').disabled = false;
        el('StateLabel').textContent = 'Approaching an obstacle';
        el('SceneCaption').textContent = 'HERE COMES THE NEXT ONE';
        phase('approach');
    }
    function answer(index) {
        if (state !== 'question' || paused) return;
        selected = index; used++;
        el('Options').querySelectorAll('button').forEach(b => {
            b.disabled = true;
            if (Number(b.dataset.answer) === question.answer) b.classList.add('is-correct');
            else if (Number(b.dataset.answer) === index) b.classList.add('is-wrong');
        });
        scores();
        const correct = index === question.answer;
        feedback(correct ? 'Correct! Jumping…' : 'Not quite. Your run has ended.', correct ? 'good' : 'bad');
        el('StateLabel').textContent = correct ? 'Jump!' : 'Obstacle hit';
        phase(correct ? 'jump' : 'crash');
    }
    function finish(demo) {
        phase('over');
        el('Pause').disabled = true; el('Start').disabled = false;
        el('Start').textContent = 'View score';
        el('StateLabel').textContent = demo ? 'Guest trial complete' : 'Game over';
        el('SceneCaption').textContent = demo ? 'GREAT RUN!' : 'TRY AGAIN. KEEP LEARNING.';
        el('Player').classList.toggle('is-crashed', !demo);
        el('ResultTitle').textContent = demo ? 'Great run!' : 'Game over';
        el('ResultMessage').textContent = demo ? 'You have used your five guest answers. Log in to keep playing.' : 'One wrong answer ends the run—but every attempt helps you learn.';
        el('FinalScore').textContent = score;
        el('FinalCleared').textContent = cleared; el('FinalBest').textContent = best;
        el('AnswerReview').hidden = false;
        el('CorrectOutput').textContent = question.options[question.answer];
        el('Explanation').textContent = question.explanation;
        el('Replay').hidden = guest && used >= 5;
        el('Login').hidden = !(guest && used >= 5);
        el('Result').showModal();
    }
    function start() {
        if (guest && used >= 5) return;
        el('Result').close();
        score = 0; cleared = 0; paused = false; deck = shuffle(bank);
        el('Player').classList.remove('is-crashed');
        el('Player').style.transform = '';
        el('Start').disabled = true; el('Start').textContent = 'Run in progress';
        el('Pause').textContent = 'Pause'; scores(); nextQuestion();
    }
    function pause() {
        if (!['approach', 'question', 'jump', 'crash'].includes(state)) return;
        paused = !paused; last = 0;
        el('Pause').textContent = paused ? 'Resume' : 'Pause';
        el('StateLabel').textContent = paused ? 'Paused' : state === 'question' ? 'Choose an answer to jump' : 'Running';
        el('Options').querySelectorAll('button').forEach(b => b.disabled = paused || state !== 'question');
        cancelAnimationFrame(frame);
        if (!paused && state !== 'question') frame = requestAnimationFrame(tick);
    }
    el('Start').addEventListener('click', () => state === 'over' ? el('Result').showModal() : start());
    el('Replay').addEventListener('click', start);
    el('Next').addEventListener('click', () => { if (state === 'feedback') nextQuestion(); });
    el('Pause').addEventListener('click', pause);
    el('CloseResult').addEventListener('click', () => el('Result').close());
    el('Review').addEventListener('click', () => { el('Result').close(); feedback(question.explanation, selected === question.answer ? 'good' : 'bad'); });
    function motionLabel() {
        el('Motion').setAttribute('aria-pressed', String(reduced));
        el('Motion').textContent = reduced ? 'Motion reduced' : 'Reduce motion';
    }
    el('Motion').addEventListener('click', () => { reduced = !reduced; motionLabel(); });
    document.addEventListener('visibilitychange', () => { if (document.hidden && !paused) pause(); });
    document.addEventListener('keydown', event => {
        if (event.repeat || event.ctrlKey || event.altKey || event.metaKey || el('Result').open || /INPUT|TEXTAREA|SELECT/.test(event.target.tagName) || event.target.isContentEditable) return;
        if (/^[1-4]$/.test(event.key) && state === 'question' && !paused) {
            event.preventDefault(); el('Options').children[Number(event.key) - 1].click();
        }
    });
    scores(); motionLabel();
    if (!Array.isArray(bank) || !bank.length) {
        el('Start').disabled = true;
        feedback('The questions could not load. Please check that MiniGameQuestions.js is installed.', 'bad');
    }
}());
