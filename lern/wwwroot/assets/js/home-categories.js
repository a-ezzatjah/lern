document.addEventListener('DOMContentLoaded', function () {
    const track = document.getElementById('home-categories');
    if (!track || !window.Swiper) return;
    const carousel = track.closest('.home-category-carousel');
    new Swiper(track, {
        slidesPerView: 2,
        slidesPerGroup: 2,
        spaceBetween: 16,
        speed: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 0 : 300,
        navigation: {
            nextEl: carousel.querySelector('.home-category-next'),
            prevEl: carousel.querySelector('.home-category-prev')
        },
        breakpoints: {
            481: { slidesPerView: 3, slidesPerGroup: 3 },
            769: { slidesPerView: 4, slidesPerGroup: 4 }
        },
        a11y: { prevSlideMessage: 'دسته‌های قبلی', nextSlideMessage: 'دسته‌های بعدی' }
    });
});
