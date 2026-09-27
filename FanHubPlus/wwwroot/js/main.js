document.addEventListener('DOMContentLoaded', function () {
    // Mobile navigation toggle
    const mobileMenuBtn = document.getElementById('mobileMenuBtn');
    const mobileMenu = document.getElementById('mobileMenu');
    if (mobileMenuBtn && mobileMenu) {
        mobileMenuBtn.addEventListener('click', function () {
            mobileMenu.classList.toggle('hidden');
        });
    }

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
