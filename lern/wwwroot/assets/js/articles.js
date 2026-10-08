(() => {
    const body = document.querySelector('[data-article-body]');
    if (!body) return;
    const toc = document.querySelector('[data-article-toc]');
    const list = document.querySelector('[data-toc-list]');
    const headings = [...body.querySelectorAll('h2, h3')];
    const links = [];
    headings.forEach((heading, index) => {
        heading.id = `article-section-${index + 1}`;
        const item = document.createElement('li');
        const link = document.createElement('a');
        link.href = `#${heading.id}`;
        link.textContent = heading.textContent;
        if (heading.tagName === 'H3') item.classList.add('is-subheading');
        item.append(link);
        list.append(item);
        links.push(link);
    });
    toc.hidden = headings.length === 0;
    const progress = document.querySelector('[data-reading-progress]');
    const header = document.querySelector('#topHeader');
    const root = document.querySelector('.journal-detail');
    let headerHeight = header?.getBoundingClientRect().height || 0;
    const sizeHeader = () => {
        headerHeight = header?.getBoundingClientRect().height || 0;
        root.style.setProperty('--article-header-offset', `${headerHeight + 24}px`);
    };
    if (header && typeof ResizeObserver !== 'undefined') new ResizeObserver(sizeHeader).observe(header);
    sizeHeader();
    let scheduled = false;
    const update = () => {
        scheduled = false;
        const rect = body.getBoundingClientRect();
        const available = Math.max(1, rect.height - window.innerHeight + headerHeight);
        const ratio = Math.max(0, Math.min(1, (headerHeight - rect.top) / available));
        progress.style.transform = `scaleX(${ratio})`;
        let active = -1;
        headings.forEach((heading, index) => {
            if (heading.getBoundingClientRect().top <= headerHeight + 100) active = index;
        });
        links.forEach((link, index) => {
            if (index === active) link.setAttribute('aria-current', 'location');
            else link.removeAttribute('aria-current');
        });
    };
    const schedule = () => {
        if (!scheduled) { scheduled = true; requestAnimationFrame(update); }
    };
    window.addEventListener('scroll', schedule, { passive: true });
    window.addEventListener('resize', schedule);
    window.addEventListener('load', schedule, { once: true });
    if (typeof ResizeObserver !== 'undefined') new ResizeObserver(schedule).observe(body);
    update();
})();
