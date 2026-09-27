document.addEventListener('DOMContentLoaded', function () {
    // ===== Misao style navbar : sticky state, search box, sidebar =====
    const navbarEl = document.getElementById('navbar');
    if (navbarEl) {
        const setSticky = function () {
            navbarEl.classList.toggle('navbar-sticky', window.scrollY > 20);
        };
        setSticky();
        window.addEventListener('scroll', setSticky, { passive: true });
    }

    const searchBtn = document.getElementById('searchBtn');
    const searchBox = document.getElementById('searchBox');
    if (searchBtn && searchBox) {
        const setSearch = function (open) {
            searchBox.classList.toggle('open', open);
            searchBtn.setAttribute('aria-expanded', open ? 'true' : 'false');
            if (open) {
                const input = searchBox.querySelector('input');
                if (input) input.focus();
            }
        };
        searchBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            setSearch(!searchBox.classList.contains('open'));
        });
        document.addEventListener('click', function (e) {
            if (!searchBox.contains(e.target) && !searchBtn.contains(e.target)) {
                setSearch(false);
            }
        });
    }

    const navbarBurger = document.getElementById('navbarBurgerToggle');
    const navbarSidebar = document.getElementById('navbarSidebar');
    const navbarBackdrop = document.getElementById('navbarBackdrop');
    const navbarSidebarClose = document.getElementById('navbarSidebarClose');
    const setSidebar = function (open) {
        if (!navbarSidebar) return;
        navbarSidebar.classList.toggle('open', open);
        if (navbarBackdrop) navbarBackdrop.classList.toggle('open', open);
        if (navbarBurger) {
            navbarBurger.classList.toggle('active', open);
            navbarBurger.setAttribute('aria-expanded', open ? 'true' : 'false');
        }
        document.body.classList.toggle('navbar-sidebar-open', open);
    };
    if (navbarBurger) {
        navbarBurger.addEventListener('click', function () {
            setSidebar(!navbarSidebar.classList.contains('open'));
        });
    }
    if (navbarSidebarClose) {
        navbarSidebarClose.addEventListener('click', function () { setSidebar(false); });
    }
    if (navbarBackdrop) {
        navbarBackdrop.addEventListener('click', function () { setSidebar(false); });
    }

    // Sidebar dropdown accordions
    document.querySelectorAll('.sidebar-navbar-nav .nav-link.dropdown-toggle').forEach(function (link) {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            const menu = link.nextElementSibling;
            if (!menu || !menu.classList.contains('dropdown-menu')) return;
            const wasOpen = !menu.classList.contains('hidden');
            menu.classList.toggle('hidden', wasOpen);
            link.classList.toggle('open', !wasOpen);
        });
    });

    // Close the sidebar when a plain sidebar link is chosen
    if (navbarSidebar) {
        navbarSidebar.querySelectorAll('a').forEach(function (link) {
            if (!link.classList.contains('dropdown-toggle')) {
                link.addEventListener('click', function () { setSidebar(false); });
            }
        });
    }

    // ESC closes search box / sidebar
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            if (searchBox) searchBox.classList.remove('open');
            setSidebar(false);
        }
    });

    // Hero Swiper Slider
    if (document.querySelector('.hero-swiper')) {
        new Swiper('.hero-swiper', {
            loop: true,
            autoplay: {
                delay: 6000,
                disableOnInteraction: false,
            },
            effect: 'fade',
            fadeEffect: {
                crossFade: true
            },
            pagination: {
                el: '.hero-pagination',
                clickable: true,
            },
            navigation: {
                nextEl: '.hero-next',
                prevEl: '.hero-prev',
            },
        });
    }

    // Carousel for Trending Videos
    if (document.querySelector('.trending-swiper')) {
        new Swiper('.trending-swiper', {
            slidesPerView: 1.2,
            spaceBetween: 16,
            loop: false,
            navigation: {
                nextEl: '.trending-next',
                prevEl: '.trending-prev',
            },
            breakpoints: {
                480: {
                    slidesPerView: 2.2,
                    spaceBetween: 18,
                },
                768: {
                    slidesPerView: 3.2,
                    spaceBetween: 20,
                },
                1024: {
                    slidesPerView: 4.2,
                    spaceBetween: 24,
                },
                1280: {
                    slidesPerView: 5,
                    spaceBetween: 24,
                }
            }
        });
    }

    // Carousel for Movies
    if (document.querySelector('.movies-swiper')) {
        new Swiper('.movies-swiper', {
            slidesPerView: 1.2,
            spaceBetween: 16,
            loop: false,
            navigation: {
                nextEl: '.movies-next',
                prevEl: '.movies-prev',
            },
            breakpoints: {
                480: {
                    slidesPerView: 2.2,
                    spaceBetween: 18,
                },
                768: {
                    slidesPerView: 3.2,
                    spaceBetween: 20,
                },
                1024: {
                    slidesPerView: 4.2,
                    spaceBetween: 24,
                },
                1280: {
                    slidesPerView: 5,
                    spaceBetween: 24,
                }
            }
        });
    }

    // Carousel for TV Shows
    if (document.querySelector('.tvshows-swiper')) {
        new Swiper('.tvshows-swiper', {
            slidesPerView: 1.2,
            spaceBetween: 16,
            loop: false,
            navigation: {
                nextEl: '.tvshows-next',
                prevEl: '.tvshows-prev',
            },
            breakpoints: {
                480: {
                    slidesPerView: 2.2,
                    spaceBetween: 18,
                },
                768: {
                    slidesPerView: 3.2,
                    spaceBetween: 20,
                },
                1024: {
                    slidesPerView: 4.2,
                    spaceBetween: 24,
                },
                1280: {
                    slidesPerView: 5,
                    spaceBetween: 24,
                }
            }
        });
    }

    // Carousel for Latest Releases
    if (document.querySelector('.latest-swiper')) {
        new Swiper('.latest-swiper', {
            slidesPerView: 1.2,
            spaceBetween: 16,
            loop: false,
            navigation: {
                nextEl: '.latest-next',
                prevEl: '.latest-prev',
            },
            breakpoints: {
                480: {
                    slidesPerView: 2.2,
                    spaceBetween: 18,
                },
                768: {
                    slidesPerView: 3.2,
                    spaceBetween: 20,
                },
                1024: {
                    slidesPerView: 4.2,
                    spaceBetween: 24,
                },
                1280: {
                    slidesPerView: 5,
                    spaceBetween: 24,
                }
            }
        });
    }

    // Carousel for Testimonials
    if (document.querySelector('.testimonial-swiper')) {
        new Swiper('.testimonial-swiper', {
            slidesPerView: 1,
            spaceBetween: 24,
            loop: true,
            autoplay: {
                delay: 5000,
            },
            pagination: {
                el: '.testimonial-pagination',
                clickable: true,
            },
            breakpoints: {
                768: {
                    slidesPerView: 2,
                    spaceBetween: 24,
                },
                1024: {
                    slidesPerView: 3,
                    spaceBetween: 24,
                }
            }
        });
    }

    // Back to top button
    const backToTopBtn = document.getElementById('backToTopBtn');
    if (backToTopBtn) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 400) {
                backToTopBtn.classList.add('show');
            } else {
                backToTopBtn.classList.remove('show');
            }
        });
        backToTopBtn.addEventListener('click', function () {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });
    }
});
