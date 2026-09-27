/* =====================================================================
   FanHub Plus - UI behaviour layer
   Handles the interactive patterns shared by every page: banner slider +
   thumbnail rail, generic content rails, tabs, accordions, billing period
   switch, quantity steppers, password reveal, counters and scroll reveals.
   ===================================================================== */
(function () {
    'use strict';

    var prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    /* ---------- 1. Banner slider + thumbnail rail ------------------- */
    function initBanner() {
        var banner = document.querySelector('[data-fh-banner]');
        if (!banner || typeof Swiper === 'undefined') return;

        var thumbs = banner.querySelector('[data-fh-banner-thumbs]');
        var slider = new Swiper(banner.querySelector('.swiper'), {
            loop: true,
            speed: 900,
            autoplay: prefersReduced ? false : { delay: 6500, disableOnInteraction: false },
            effect: 'fade',
            fadeEffect: { crossFade: true },
            pagination: { el: banner.querySelector('[data-fh-banner-dots]'), clickable: true },
            navigation: {
                nextEl: banner.querySelector('[data-fh-banner-next]'),
                prevEl: banner.querySelector('[data-fh-banner-prev]')
            }
        });

        if (!thumbs) return;

        var thumbSwiper = new Swiper(thumbs.querySelector('.swiper'), {
            slidesPerView: 2.4,
            spaceBetween: 10,
            watchSlidesProgress: true,
            breakpoints: { 640: { slidesPerView: 3.4 }, 1024: { slidesPerView: 4.4 } }
        });

        thumbs.querySelectorAll('.swiper-slide').forEach(function (slide, i) {
            slide.setAttribute('role', 'button');
            slide.setAttribute('tabindex', '0');
            slide.addEventListener('click', function () { slider.slideToLoop(i); });
            slide.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); slider.slideToLoop(i); }
            });
        });

        var syncActive = function () {
            thumbs.querySelectorAll('.swiper-slide').forEach(function (slide, i) {
                slide.classList.toggle('swiper-slide-thumb-active', i === slider.activeIndex);
            });
            if (thumbSwiper && thumbSwiper.slides.length) {
                thumbSwiper.slideTo(Math.max(0, slider.activeIndex - 1));
            }
        };
        slider.on('slideChangeTransitionStart', syncActive);
        slider.on('init', syncActive);
        slider.on('afterInit', syncActive);
    }

    /* ---------- 2. Generic content rails ---------------------------- */
    var railPresets = {
        poster: { slidesPerView: 2, spaceBetween: 14, minWidth: 150 },
        still: { slidesPerView: 1.35, spaceBetween: 16, minWidth: 260 }
    };

    function initRails() {

    /* ---------- 3. Tabs --------------------------------------------- */
    function initTabs() {
        document.querySelectorAll('[data-fh-tabs]').forEach(function (group) {
            var tabs = Array.prototype.slice.call(group.querySelectorAll('.fh-tab'));
            if (!tabs.length) return;

            var show = function (target) {
                tabs.forEach(function (tab) {
                    var match = tab.getAttribute('data-fh-tab') === target;
                    tab.classList.toggle('is-active', match);
                    tab.setAttribute('aria-selected', match ? 'true' : 'false');
                    var pane = document.getElementById(target);
                    if (pane) pane.classList.toggle('is-active', match);
                });
            };

            tabs.forEach(function (tab, index) {
                tab.setAttribute('role', 'tab');
                tab.addEventListener('click', function () { show(tab.getAttribute('data-fh-tab')); });
                tab.addEventListener('keydown', function (e) {
                    var delta = e.key === 'ArrowRight' ? 1 : e.key === 'ArrowLeft' ? -1 : 0;
                    if (!delta) return;
                    e.preventDefault();
                    var next = tabs[(index + delta + tabs.length) % tabs.length];
                    next.focus();
                    show(next.getAttribute('data-fh-tab'));
                });
            });

            var initial = tabs.filter(function (t) { return t.classList.contains('is-active'); })[0] || tabs[0];
            show(initial.getAttribute('data-fh-tab'));
        });
    }

    /* ---------- 4. Accordions --------------------------------------- */
    function initAccordions() {
        document.querySelectorAll('.fh-acc-q').forEach(function (button) {
            button.addEventListener('click', function () {
                var item = button.closest('.fh-acc');
                if (!item) return;
                var open = item.classList.contains('is-open');
                var group = item.parentElement;
                if (group && group.hasAttribute('data-fh-single')) {
                    group.querySelectorAll('.fh-acc.is-open').forEach(function (other) {
                        other.classList.remove('is-open');
                        other.querySelector('.fh-acc-q').setAttribute('aria-expanded', 'false');
                    });
                }
                item.classList.toggle('is-open', !open);
                button.setAttribute('aria-expanded', !open ? 'true' : 'false');
            });
        });
    }

    /* ---------- 5. Billing period switch ---------------------------- */
    function initBilling() {
        document.querySelectorAll('[data-fh-billing]').forEach(function (toggle) {
            var buttons = toggle.querySelectorAll('button[data-fh-period]');
            buttons.forEach(function (button) {
                button.addEventListener('click', function () {
                    var period = button.getAttribute('data-fh-period');
                    buttons.forEach(function (b) { b.classList.toggle('is-active', b === button); });
                    document.querySelectorAll('[data-fh-price]').forEach(function (node) {
                        var value = node.getAttribute('data-fh-' + period);
                        if (value) node.textContent = value;
                    });
                    document.querySelectorAll('[data-fh-price-note]').forEach(function (node) {
                        var value = node.getAttribute('data-fh-' + period);
                        if (value) node.textContent = value;
                    });
                });
            });
        });
    }

    /* ---------- 6. Quantity steppers -------------------------------- */
    function initSteppers() {
        document.querySelectorAll('.fh-stepper').forEach(function (stepper) {
            var input = stepper.querySelector('input');
            if (!input) return;
            var min = parseInt(input.getAttribute('min') || '1', 10);
            var max = parseInt(input.getAttribute('max') || '99', 10);
            var write = function (value) {
                var next = Math.min(max, Math.max(min, isNaN(value) ? min : value));
                input.value = next;
                input.dispatchEvent(new Event('change', { bubbles: true }));
            };
            stepper.querySelectorAll('button[data-fh-step]').forEach(function (button) {
                button.addEventListener('click', function () {
                    write(parseInt(input.value, 10) + parseInt(button.getAttribute('data-fh-step'), 10));
                });
            });
            input.addEventListener('change', function () { write(parseInt(input.value, 10)); });
        });
    }

    /* ---------- 7. Password reveal ---------------------------------- */
    function initPasswordReveal() {
        document.querySelectorAll('[data-fh-reveal]').forEach(function (button) {
            button.addEventListener('click', function () {
                var field = document.getElementById(button.getAttribute('data-fh-reveal'));
                if (!field) return;
                var hidden = field.type === 'password';
                field.type = hidden ? 'text' : 'password';
                button.innerHTML = hidden ? '<i class="ri-eye-off-line"></i>' : '<i class="ri-eye-line"></i>';
            });
        });
    }

        if (typeof Swiper === 'undefined') return;
        document.querySelectorAll('[data-fh-rail]').forEach(function (rail) {
            var swiperEl = rail.querySelector('.swiper');
            if (!swiperEl) return;
            var preset = railPresets[rail.getAttribute('data-fh-rail')] || railPresets.poster;
            var perView = parseFloat(preset.slidesPerView);
            new Swiper(swiperEl, {
                slidesPerView: perView,
                spaceBetween: preset.spaceBetween,
                grabCursor: true,
                watchOverflow: true,
                navigation: {
                    nextEl: rail.querySelector('[data-fh-next]'),
                    prevEl: rail.querySelector('[data-fh-prev]')
                },
                breakpoints: {
                    480: { slidesPerView: perView + 0.4, spaceBetween: 16 },
                    768: { slidesPerView: perView + 1.4, spaceBetween: 18 },
                    1024: { slidesPerView: perView + 2.4, spaceBetween: 20 },
                    1280: { slidesPerView: perView + 3, spaceBetween: 22 }
                }
            });
        });
    }

    /* ---------- 8. Animated counters -------------------------------- */
    function initCounters() {
        var nodes = document.querySelectorAll('[data-fh-count]');
        if (!nodes.length) return;

        var run = function (node) {
            var target = parseFloat(node.getAttribute('data-fh-count')) || 0;
            var suffix = node.getAttribute('data-fh-suffix') || '';
            if (prefersReduced) { node.textContent = target.toLocaleString() + suffix; return; }
            var start = performance.now();
            var duration = 1200;
            var tick = function (now) {
                var progress = Math.min(1, (now - start) / duration);
                var eased = 1 - Math.pow(1 - progress, 3);
                node.textContent = Math.round(target * eased).toLocaleString() + suffix;
                if (progress < 1) requestAnimationFrame(tick);
            };
            requestAnimationFrame(tick);
        };

        if (!('IntersectionObserver' in window)) { nodes.forEach(run); return; }
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) { run(entry.target); observer.unobserve(entry.target); }
            });
        }, { threshold: 0.4 });
        nodes.forEach(function (node) { observer.observe(node); });
    }

    /* ---------- 9. Scroll reveals ----------------------------------- */
    function initReveals() {
        var nodes = document.querySelectorAll('.fh-reveal');
        if (!nodes.length) return;
        if (!('IntersectionObserver' in window)) {
            nodes.forEach(function (n) { n.classList.add('is-in'); });
            return;
        }
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) { entry.target.classList.add('is-in'); observer.unobserve(entry.target); }
            });
        }, { threshold: 0.12, rootMargin: '0px 0px -60px' });
        nodes.forEach(function (n) { observer.observe(n); });
    }

    /* ---------- 10. Keyboard access for dropdowns ------------------- */
    function initDropdowns() {
        document.querySelectorAll('#navbar .nav-item > .dropdown-toggle').forEach(function (toggle) {
            toggle.setAttribute('aria-haspopup', 'true');
            toggle.addEventListener('focus', function () {
                if (toggle.parentElement) toggle.parentElement.classList.add('focus-within');
            });
            toggle.addEventListener('blur', function () {
                if (toggle.parentElement) toggle.parentElement.classList.remove('focus-within');
            });
        });
    }

    /* ---------- Boot ------------------------------------------------- */
    function boot() {
        initBanner();
        initRails();
        initTabs();
        initAccordions();
        initBilling();
        initSteppers();
        initPasswordReveal();
        initCounters();
        initReveals();
        initDropdowns();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }
})();

