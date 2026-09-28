// Fan Hub Plus - global JavaScript
// Theme (dark mode) + font size + page spinner + chat widget + AJAX rating/bookmark.
// Preferences are persisted in the cookie "fhp_pref" = "<dark 0|1>|<fontSize>"
// so the server can render the correct theme on the very next request (no flash).

(function () {
    'use strict';

    // ---------- Preference helpers ----------
    function readPref() {
        var m = document.cookie.match(/(?:^|;\s*)fhp_pref=([^;]+)/);
        if (m) {
            var parts = decodeURIComponent(m[1]).split('|');
            return { dark: parts[0] === '1', font: parseInt(parts[1] || '16', 10) };
        }
        var ls = localStorage.getItem('fhp_pref');
        if (ls) {
            var p = ls.split('|');
            return { dark: p[0] === '1', font: parseInt(p[1] || '16', 10) };
        }
        return { dark: false, font: 16 };
    }

    function writePref(pref) {
        var value = (pref.dark ? '1' : '0') + '|' + pref.font;
        document.cookie = 'fhp_pref=' + value + ';path=/;max-age=31536000;SameSite=Lax';
        localStorage.setItem('fhp_pref', value);
    }

    function applyPref(pref) {
        // Both shells lock the FanHubPlus dark theme via data-theme-lock="dark"
        // (public site and admin alike), so the stored preference only decides
        // the font size there; a fresh pref still lights up an unlocked page.
        var locked = document.documentElement.getAttribute('data-theme-lock') === 'dark';
        document.documentElement.classList.toggle('dark-mode', locked || pref.dark);
        document.documentElement.style.fontSize = pref.font + 'px';
    }

    // Exposed to inline onclick handlers in the navbar
    window.fhpToggleTheme = function () {
        var pref = readPref();
        pref.dark = !pref.dark;
        writePref(pref); applyPref(pref);
        var btn = document.getElementById('themeToggle');
        if (btn) btn.textContent = pref.dark ? 'â˜€ï¸' : 'ðŸŒ™';
    };
    window.fhpFontStep = function (delta) {
        var pref = readPref();
        pref.font = Math.min(22, Math.max(14, pref.font + delta));
        writePref(pref); applyPref(pref);
    };

    // Guests: apply stored localStorage preference before anything renders
    applyPref(readPref());

    // ---------- Page spinner ----------
    function hideLoader() {
        var loader = document.getElementById('pageLoader');
        if (loader) loader.classList.add('hidden');
    }
    document.addEventListener('DOMContentLoaded', hideLoader);
    window.addEventListener('load', hideLoader);

    // ---------- Shared AJAX helper (attaches the antiforgery token) ----------
    function token() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    // ---------- Chat widget ----------
    document.addEventListener('DOMContentLoaded', function () {
        var widget = document.getElementById('chatWidget');
        var toggle = document.getElementById('chatToggle');
        if (!widget || !toggle) return;

        var voiceToggle = document.getElementById('chatVoiceToggle');
        var voiceStop = document.getElementById('chatVoiceStop');
        var micBtn = document.getElementById('chatMicBtn');
        var _speech = window.fhpSpeech;

        function open() {
            widget.classList.remove('d-none');
            toggle.style.display = 'none';
            var body = document.getElementById('chatBody');
            if (body) body.scrollTop = body.scrollHeight;
            updateVoiceToggle();
        }
        function close() { widget.classList.add('d-none'); toggle.style.display = ''; }

        toggle.addEventListener('click', open);
        var closeBtn = document.getElementById('chatClose');
        if (closeBtn) closeBtn.addEventListener('click', close);

        var body = document.getElementById('chatBody');
        var suggestions = document.getElementById('chatSuggestions');

        function updateVoiceToggle() {
            if (!voiceToggle) return;
            var pref = _speech ? _speech.pref.read() : { auto: false };
            voiceToggle.classList.toggle('active', pref.auto);
        }

        function addSpeakBtn(msgEl, text) {
            if (!msgEl) return;
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'chat-msg__voice';
            btn.setAttribute('aria-label', 'Read aloud');
            btn.innerHTML = '<i class="ri-speak-line" aria-hidden="true"></i>';
            btn.addEventListener('click', function () { speakMsg(msgEl, text); });
            msgEl.appendChild(btn);
        }

        function speakMsg(msgEl, text) {
            if (!_speech) return;
            if (_speech.isSpeaking()) { _speech.stop(); return; }
            msgEl.classList.add('is-speaking');
            var icon = msgEl.querySelector('.chat-msg__voice i');
            if (icon) icon.className = 'ri-stop-fill';
            _speech.speak(text, function () {
                msgEl.classList.remove('is-speaking');
                if (icon) icon.className = 'ri-speak-line';
            });
        }

        function queueAutoSpeak(text) {
            if (!_speech) return;
            var pref = _speech.pref.read();
            if (!pref.auto) return;
            if (_speech.isSpeaking()) return;
            _speech.speak(text, function () {});
        }

        function escapeHtml(str) {
            return String(str)
                .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
        }

        function renderResults(msgEl, results) {
            if (!msgEl || !results || results.length === 0) return;
            var list = document.createElement('div');
            list.className = 'chat-results';
            results.forEach(function (r) {
                var a = document.createElement('a');
                a.href = r.url;
                a.className = 'chat-result-item';
                a.innerHTML = '<span class="chat-result-title">' + escapeHtml(r.title) + '</span>' +
                              (r.subtitle ? '<span class="chat-result-sub"> ' + escapeHtml(r.subtitle) + '</span>' : '');
                list.appendChild(a);
            });
            msgEl.appendChild(list);
        }

        function addMsg(text, who) {
            var div = document.createElement('div');
            div.className = 'chat-msg ' + who;
            div.textContent = text;
            body.appendChild(div);
            body.scrollTop = body.scrollHeight;

            if (who.indexOf('bot') !== -1 && who.indexOf('typing') === -1) {
                addSpeakBtn(div, text);
            }
            return div;
        }

        function renderSuggestions(list) {
            suggestions.innerHTML = '';
            (list || []).forEach(function (s) {
                var b = document.createElement('button');
                b.type = 'button'; b.className = 'chat-suggestion'; b.textContent = s;
                b.addEventListener('click', function () { send(s); });
                suggestions.appendChild(b);
            });
        }

        function send(text) {
            if (!text) return;
            addMsg(text, 'user');
            var typing = addMsg('typingâ€¦', 'bot typing');
            var fd = new FormData();
            fd.append('message', text);
            fd.append('__RequestVerificationToken', token());

            fetch('/Chatbot/Ask', { method: 'POST', body: fd, credentials: 'same-origin' })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    typing.remove();
                    var replyText = data.reply || '(empty reply)';
                    var botMsg = addMsg(replyText, 'bot');
                    renderResults(botMsg, data.results);
                    queueAutoSpeak(replyText);
                    renderSuggestions(data.suggestions);
                })
                .catch(function () { typing.remove(); var m = addMsg('Sorry, I could not reach the server.', 'bot'); queueAutoSpeak(m.textContent); });
        }

        document.getElementById('chatForm').addEventListener('submit', function (e) {
            e.preventDefault();
            var input = document.getElementById('chatText');
            send(input.value.trim());
            input.value = '';
        });

        if (voiceToggle) {
            voiceToggle.addEventListener('click', function () {
                var pref = _speech ? _speech.pref.read() : { auto: false };
                pref.auto = !pref.auto;
                if (_speech) _speech.pref.setAuto(pref.auto);
                updateVoiceToggle();
            });
        }

        if (voiceStop) {
            voiceStop.addEventListener('click', function () {
                if (_speech) _speech.stop();
            });
        }

        if (micBtn && _speech && _speech.isRecognitionSupported) {
            micBtn.addEventListener('click', function () {
                var micInput = document.getElementById('chatText');
                if (!micInput) return;
                micBtn.disabled = true;
                micBtn.querySelector('i').className = 'ri-stop-fill';
                _speech.recognize(
                    function (text) { send(text); },
                    function (err) {
                        fhpToast(err === 'unsupported'
                            ? 'Speech recognition is not available in your browser.'
                            : 'Speech recognition error: ' + err);
                    }
                );
                setTimeout(function () {
                    micBtn.disabled = false;
                    var i = micBtn.querySelector('i');
                    if (i) i.className = 'ri-mic-line';
                }, 8000);
            });
        } else if (micBtn) {
            micBtn.style.display = 'none';
        }
    });

    // ---------- Star rating (AJAX) ----------
    // Supports the component markup (.fhp-rate button[data-star]) and the legacy
    // .star spans, so pages can migrate one at a time without breaking either.
    document.addEventListener('DOMContentLoaded', function () {
        var wrap = document.getElementById('ratingWidget');
        if (!wrap) return;
        var contentId = wrap.getAttribute('data-content-id');
        var stars = Array.prototype.slice.call(wrap.querySelectorAll('[data-star]'));

        function paint(n, hover) {
            stars.forEach(function (s, i) { s.classList.toggle(hover ? 'hover' : 'is-on', i < n); });
        }

        function commit(value) {
            var fd = new FormData();
            fd.append('contentId', contentId);
            fd.append('stars', String(value));
            fd.append('__RequestVerificationToken', token());
            fetch('/Explore/Rate', { method: 'POST', body: fd, credentials: 'same-origin' })
                .then(function (r) { return r.json(); })
                .then(function (res) {
                    if (res.auth === false) {
                        window.location = '/Account/Login?returnUrl=' + encodeURIComponent(location.pathname);
                        return;
                    }
                    if (!res.ok) { if (res.message) fhpToast(res.message); return; }
                    wrap.setAttribute('data-my-stars', res.myStars);
                    paint(res.myStars, false);
                    var avg = document.getElementById('ratingAvg');
                    var cnt = document.getElementById('ratingCount');
                    var row = document.getElementById('ratingStars');
                    if (avg) avg.textContent = res.avg;
                    if (cnt) cnt.textContent = '(' + res.count + ' ratings)';
                    if (row) {
                        Array.prototype.forEach.call(row.querySelectorAll('i'), function (icon, i) {
                            icon.style.opacity = i < Math.round(res.avg) ? '1' : '.35';
                        });
                    }
                });
        }

        stars.forEach(function (star, i) {
            var value = parseInt(star.getAttribute('data-star'), 10) || (i + 1);
            star.addEventListener('mouseenter', function () { paint(value, true); });
            star.addEventListener('mouseleave', function () {
                stars.forEach(function (s) { s.classList.remove('hover'); });
                paint(parseInt(wrap.getAttribute('data-my-stars') || '0', 10), false);
            });
            star.addEventListener('click', function () { commit(value); });
        });
        paint(parseInt(wrap.getAttribute('data-my-stars') || '0', 10), false);
    });

    // ---------- Bookmark toggle (AJAX, icon preserving) ----------
    document.addEventListener('DOMContentLoaded', function () {
        Array.prototype.forEach.call(document.querySelectorAll('.bookmark-btn'), function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                btn.disabled = true;
                var fd = new FormData();
                fd.append('type', btn.getAttribute('data-type'));
                fd.append('itemId', btn.getAttribute('data-id'));
                fd.append('__RequestVerificationToken', token());
                fetch('/Bookmarks/Toggle', { method: 'POST', body: fd, credentials: 'same-origin' })
                    .then(function (r) { return r.json(); })
                    .then(function (res) {
                        btn.disabled = false;
                        if (res.auth === false) {
                            window.location = '/Account/Login?returnUrl=' + encodeURIComponent(location.pathname);
                            return;
                        }
                        if (!res.ok) return;
                        btn.classList.toggle('is-saved', res.bookmarked);
                        btn.setAttribute('aria-pressed', res.bookmarked ? 'true' : 'false');
                        var icon = btn.querySelector('i');
                        if (icon) icon.className = res.bookmarked ? 'ri-heart-fill' : 'ri-heart-line';
                        var label = btn.querySelector('.bookmark-btn__label');
                        if (label) label.textContent = res.bookmarked ? 'Bookmarked' : 'Bookmark';
                        if (res.message) fhpToast(res.message);
                    })
                    .catch(function () { btn.disabled = false; fhpToast('Could not reach the server. Please try again.'); });
            });
        });
    });

    // ---------- Share (Web Share API with clipboard fallback) ----------
    document.addEventListener('DOMContentLoaded', function () {
        Array.prototype.forEach.call(document.querySelectorAll('.fhp-share'), function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                var url = new URL(btn.getAttribute('data-share-url') || location.href, location.origin).href;
                var title = btn.getAttribute('data-share-title') || document.title;
                if (navigator.share) {
                    navigator.share({ title: title, url: url }).catch(function () { });
                    return;
                }
                if (navigator.clipboard) {
                    navigator.clipboard.writeText(url).then(
                        function () { fhpToast('Link copied to clipboard'); },
                        function () { fhpToast(url); });
                } else { fhpToast(url); }
            });
        });
    });

    // ---------- Toast ----------
    var fhpToastTimer;
    function fhpToast(message) {
        if (!message) return;
        var el = document.querySelector('.fhp-toast');
        if (!el) {
            el = document.createElement('div');
            el.className = 'fhp-toast';
            el.setAttribute('role', 'status');
            el.setAttribute('aria-live', 'polite');
            document.body.appendChild(el);
        }
        el.textContent = message;
        el.classList.add('is-visible');
        clearTimeout(fhpToastTimer);
        fhpToastTimer = setTimeout(function () { el.classList.remove('is-visible'); }, 2600);
    }
    window.fhpToast = fhpToast;

    // ---------- In-page video player (modal) ----------
    // Any [data-fhp-video] button (trailer cards, Explorer cards, the showreel)
    // opens #fhpVideoModal, and the player is built only then: a <video> for a
    // file this site hosts, an <iframe> for an external embed. Nothing is
    // requested while the visitor browses, and dropping the node again on close
    // is what stops the sound.
    document.addEventListener('DOMContentLoaded', function () {
        var modalEl = document.getElementById('fhpVideoModal');
        if (!modalEl || typeof bootstrap === 'undefined') return;

        var stage = modalEl.querySelector('.fhp-vmodal__stage');
        var titleEl = modalEl.querySelector('.fhp-vmodal__title');
        if (!stage) return;

        function clearPlayer() {
            var media = stage.querySelector('video');
            if (media) { try { media.pause(); } catch (e) { /* ignore */ } }
            stage.innerHTML = '';
        }

        function buildLocal(src, poster, title) {
            var video = document.createElement('video');
            video.className = 'fhp-vmodal__video';
            video.controls = true;
            video.autoplay = true;
            video.playsInline = true;
            video.preload = 'metadata';
            video.setAttribute('playsinline', '');
            video.setAttribute('webkit-playsinline', '');
            if (poster) video.poster = poster;
            video.setAttribute('title', title);
            video.addEventListener('error', function () {
                fhpToast('This video could not be loaded. Please try again.');
            });
            video.src = src;
            return video;
        }

        function buildEmbed(src, title) {
            var frame = document.createElement('iframe');
            frame.className = 'fhp-vmodal__frame';
            frame.src = src + (src.indexOf('?') === -1 ? '?' : '&') + 'autoplay=1&rel=0';
            frame.title = title;
            frame.setAttribute('allow', 'accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture');
            frame.setAttribute('allowfullscreen', '');
            return frame;
        }

        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-fhp-video]');
            if (!btn) return;

            var src = btn.getAttribute('data-src');
            if (!src) return;
            e.preventDefault();

            var title = btn.getAttribute('data-title') || 'Video';
            if (titleEl) titleEl.textContent = title;

            clearPlayer();
            stage.appendChild(btn.getAttribute('data-kind') === 'local'
                ? buildLocal(src, btn.getAttribute('data-poster'), title)
                : buildEmbed(src, title));

            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        });

        modalEl.addEventListener('hidden.bs.modal', clearPlayer);
    });

    // ---------- Footer "back to top" ----------
    // Lenis drives the scroll, so prefer its instance and only fall back to
    // the native API when the script did not boot.
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-fhp-totop]');
        if (!btn) return;
        if (window.fhpLenis && typeof window.fhpLenis.scrollTo === 'function') {
            window.fhpLenis.scrollTo(0, { duration: 1.1 });
        } else {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    });

    // ---------- Dismissible notices (_Alerts) ----------
    // One delegated listener so a partial can be dropped anywhere in the tree.
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.fhp-alert__close');
        if (!btn) return;
        var box = btn.closest('.fhp-alert');
        if (!box) return;
        box.style.transition = 'opacity .25s, transform .25s';
        box.style.opacity = '0';
        box.style.transform = 'translateY(-6px)';
        window.setTimeout(function () { box.remove(); }, 250);
    });
})();

// ---------- Header (fhp-nav) -------------------------------------------
// Drives the pieces the stylesheet cannot: the reading-progress hairline,
// the burger's X state (it mirrors .show on the sidebar modal) and the
// search button's aria-expanded.
(function () {
    'use strict';

    var header = document.getElementById('navbar');
    if (!header) return;

    // Reading progress: 0 at the top, 1 at the very bottom of the document.
    var bar = header.querySelector('.fhp-nav__progress span');
    if (bar) {
        var paint = function () {
            var doc = document.documentElement;
            var scrollable = (doc.scrollHeight || 0) - window.innerHeight;
            var ratio = scrollable > 0 ? window.scrollY / scrollable : 0;
            bar.style.transform = 'scaleX(' + Math.min(1, Math.max(0, ratio)) + ')';
        };
        window.addEventListener('scroll', paint, { passive: true });
        window.addEventListener('resize', paint);
        paint();
    }

    // custom.js puts .show on the sidebar modal; mirror it onto the burger
    // so the three bars fold into an X while the drawer is open.
    var burger = document.getElementById('navbarBurgerToggle');
    var sidebar = document.querySelector('.sidebar-modal');
    if (burger && sidebar && typeof MutationObserver !== 'undefined') {
        new MutationObserver(function () {
            burger.classList.toggle('is-open', sidebar.classList.contains('show'));
        }).observe(sidebar, { attributes: true, attributeFilter: ['class'] });
    }

    // custom.js toggles .active on #searchBox; keep the button's ARIA in step.
    var searchBtn = document.getElementById('searchBtn');
    var searchBox = document.getElementById('searchBox');
    if (searchBtn && searchBox) {
        searchBtn.addEventListener('click', function () {
            searchBtn.setAttribute('aria-expanded',
                searchBox.classList.contains('active') ? 'true' : 'false');
        });
    }
})();

