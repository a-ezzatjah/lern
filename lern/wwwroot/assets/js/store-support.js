(() => {
    document.querySelectorAll('[data-ks-back-top]').forEach(link => {
        link.addEventListener('click', event => {
            event.preventDefault();
            const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            window.scrollTo({ top: 0, left: 0, behavior: reducedMotion ? 'instant' : 'smooth' });
        });
    });

    const search = document.getElementById('ks-faq-search');
    if (!search) return;
    const items = [...document.querySelectorAll('.ks-faq-item')];
    const normalize = value => value.replace(/ي/g, 'ی').replace(/ك/g, 'ک').replace(/\s+/g, ' ').trim().toLowerCase();
    search.addEventListener('input', () => {
        const query = normalize(search.value);
        let visible = 0;
        items.forEach(item => {
            const match = normalize(item.textContent).includes(query);
            item.hidden = !match;
            if (match) visible++;
        });
        document.getElementById('ks-faq-empty').hidden = visible > 0;
    });
})();
