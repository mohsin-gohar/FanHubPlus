// Fan Hub Plus - speech helpers for the FanBot chatbot
// Uses the Web Speech API (no backend / no paid service):
//   - window.speechSynthesis  -> read bot replies aloud
//   - webkitSpeechRecognition -> transcribe the user's spoken question
// Persisted preferences: localStorage key "fhp_speech" = "1|voiceName"
//   where the first field is the auto-read-on toggle (0/1).

(function () {
    'use strict';

    var STORAGE_KEY = 'fhp_speech';

    function readPref() {
        try {
            var raw = localStorage.getItem(STORAGE_KEY);
            if (!raw) return { auto: false, voice: '' };
            var parts = raw.split('|');
            return { auto: parts[0] === '1', voice: parts[1] || '' };
        } catch (e) { return { auto: false, voice: '' }; }
    }

    function writePref(pref) {
        try {
            localStorage.setItem(STORAGE_KEY, (pref.auto ? '1' : '0') + '|' + (pref.voice || ''));
        } catch (e) { /* ignore */ }
    }

    // ---- voice (text-to-speech) ----
    var voices = [];
    var voiceReady = false;

    function loadVoices() {
        voices = (typeof speechSynthesis !== 'undefined')
            ? speechSynthesis.getVoices()
            : [];
    }

    function onVoicesChanged() {
        loadVoices();
        voiceReady = true;
        window.dispatchEvent(new CustomEvent('fhp:voiceschanged'));
    }

    function initVoices() {
        if (typeof speechSynthesis !== 'undefined') {
            loadVoices();
            if (speechSynthesis.onvoiceschanged !== undefined) {
                speechSynthesis.onvoiceschanged = onVoicesChanged;
            }
            // Safari doesn't fire onvoiceschanged; fire a poll that resolves quickly.
            if (voices.length === 0) {
                var tries = 0;
                var iv = setInterval(function () {
                    loadVoices();
                    tries++;
                    if (voices.length > 0 || tries > 20) {
                        clearInterval(iv);
                        voiceReady = true;
                        window.dispatchEvent(new CustomEvent('fhp:voiceschanged'));
                    }
                }, 50);
            } else {
                voiceReady = true;
            }
        }
    }

    function pickVoice(desired) {
        if (!voices.length) return null;
        if (desired) {
            var match = voices.find(function (v) { return v.name === desired; });
            if (match) return match;
        }
        // Prefer a native en-US voice if available, then the first voice.
        var en = voices.find(function (v) { return v.lang === 'en-US'; });
        return en || voices[0];
    }

    function speak(text, onEnd) {
        var synth = typeof speechSynthesis !== 'undefined' ? speechSynthesis : null;
        if (!synth) { if (typeof onEnd === 'function') onEnd(false); return false; }

        synth.cancel(); // stop anything currently playing
        var utter = new SpeechSynthesisUtterance(text);
        utter.voice = pickVoice(readPref().voice);
        utter.rate = 1.0;
        utter.pitch = 1.0;
        utter.volume = 1.0;

        utter.onend = function () { if (typeof onEnd === 'function') onEnd(true); };
        utter.onerror = function () { if (typeof onEnd === 'function') onEnd(false); };

        synth.speak(utter);
        return true;
    }

    function stopSpeaking() {
        if (typeof speechSynthesis !== 'undefined') {
            speechSynthesis.cancel();
        }
    }

    function isSpeaking() {
        return typeof speechSynthesis !== 'undefined'
            ? speechSynthesis.speaking
            : false;
    }

    // ---- recognition (speech-to-text) ----
    var RecognitionCtor =
        (typeof window !== 'undefined') &&
        (window.SpeechRecognition || window.webkitSpeechRecognition);

    function startRecognition(onResult, onError) {
        if (!RecognitionCtor) { if (typeof onError === 'function') onError('unsupported'); return null; }
        var recog = new RecognitionCtor();
        recog.continuous = false;
        recog.interimResults = false;
        recog.lang = 'en-US';

        recog.onresult = function (e) {
            var transcript = '';
            for (var i = 0; i < e.results.length; i++) {
                transcript += e.results[i][0].transcript;
            }
            if (typeof onResult === 'function') onResult(transcript.trim());
        };
        recog.onerror = function (e) {
            if (typeof onError === 'function') onError(e.error || 'error');
        };
        recog.start();
        return recog;
    }

    // ---- public API ----
    window.fhpSpeech = {
        speak: speak,
        stop: stopSpeaking,
        isSpeaking: isSpeaking,
        getVoices: function () { return voices; },
        isReady: function () { return voiceReady; },
        recognize: startRecognition,
        isRecognitionSupported: !!RecognitionCtor,
        // preference helpers
        pref: {
            read: readPref,
            setAuto: function (v) { var p = readPref(); p.auto = v; writePref(p); },
            setVoice: function (v) { var p = readPref(); p.voice = v; writePref(p); }
        }
    };

    if (typeof document !== 'undefined') {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initVoices);
        } else {
            initVoices();
        }
    }
})();
