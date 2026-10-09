(() => {
    const page = document.getElementById('shop-page');
    const form = document.getElementById('shop-filter-form');
    const results = document.getElementById('shop-ajax-results');
    const status = document.getElementById('shop-filter-status');
    if (!page || !form || !results || !status) return;

    const mobile = matchMedia('(max-width: 60rem)');
    const drawer = document.getElementById('shop-filter-drawer');
    const sidebar = page.querySelector('.shop-filters');
    const sidebarHome = sidebar.parentElement;
    const track = page.querySelector('.shop-filter-track');
    const availableToggle = document.getElementById('shop-available-toggle');
    const colorSearch = document.getElementById('shop-color-search');
    const normalizeColor = value => value.normalize('NFKC').replace(/ي/g, 'ی').replace(/ك/g, 'ک').replace(/[\s\u200c]/g, '').toLocaleLowerCase('fa');
    const filterColors = () => {
        const query = normalizeColor(colorSearch.value);
        const options = form.querySelectorAll('[data-color-name]');
        let visible = 0;
        options.forEach(option => {
            option.hidden = !normalizeColor(option.dataset.colorName).includes(query);
            if (!option.hidden) visible++;
        });
        const empty = form.querySelector('.shop-color-empty');
        empty.hidden = visible > 0;
        empty.textContent = options.length ? 'رنگی با این نام پیدا نشد.' : 'رنگی برای نمایش وجود ندارد.';
    };
    colorSearch.addEventListener('input', filterColors);
    let opener = null;
    let previousOverflow = '';
    const closeDrawer = () => { if (drawer.open) drawer.close(); };
    drawer.addEventListener('close', () => {
        document.body.style.overflow = previousOverflow;
        if (mobile.matches) opener?.focus();
    });
    drawer.querySelector('[data-close-filter]').addEventListener('click', closeDrawer);
    drawer.addEventListener('click', event => {
        if (event.target !== drawer) return;
        const rect = drawer.getBoundingClientRect();
        if (event.clientX < rect.left || event.clientX > rect.right || event.clientY < rect.top || event.clientY > rect.bottom) closeDrawer();
    });
    const updateArrows = () => {
        const overflowing = track.scrollWidth > track.clientWidth + 1;
        page.querySelectorAll('[data-scroll]').forEach(button => {
            button.hidden = !overflowing;
            button.disabled = button.dataset.scroll === 'right' ? Math.abs(track.scrollLeft) < 2
                : Math.abs(track.scrollLeft) >= track.scrollWidth - track.clientWidth - 2;
        });
    };
    page.querySelectorAll('[data-scroll]').forEach(button => button.addEventListener('click', () => {
        track.scrollBy({ left: (button.dataset.scroll === 'right' ? 1 : -1) * track.clientWidth * .7,
            behavior: matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth' });
    }));
    track.addEventListener('scroll', updateArrows, { passive: true });
    new ResizeObserver(updateArrows).observe(track);
    const syncMobile = () => {
        availableToggle.setAttribute('aria-pressed', String(form.elements.available.checked));
        const active = {
            category: !!form.querySelector('input[name="category"]:checked'), color: !!form.querySelector('input[name="color"]:checked'), search: !!form.elements.q.value.trim(),
            price: !!(form.elements.minPrice.value || form.elements.maxPrice.value), sort: form.elements.sort.value !== 'newest'
        };
        page.querySelectorAll('[data-open-filter]').forEach(button => button.classList.toggle('is-active', active[button.dataset.openFilter]));
    };
    const placeSidebar = () => {
        closeDrawer();
        if (mobile.matches) drawer.append(sidebar);
        else sidebarHome.prepend(sidebar);
        updateArrows();
    };
    mobile.addEventListener('change', placeSidebar);
    page.classList.add('shop-mobile-ready');
    placeSidebar();
    syncMobile();
    page.querySelectorAll('[data-open-filter]').forEach(button => button.addEventListener('click', () => {
        if (!mobile.matches) return;
        opener = button;
        sidebar.querySelectorAll('[data-filter-section]').forEach(section => section.classList.toggle('is-visible', section.dataset.filterSection === button.dataset.openFilter));
        document.getElementById('shop-drawer-title').textContent = button.textContent;
        previousOverflow = document.body.style.overflow;
        drawer.showModal();
        document.body.style.overflow = 'hidden';
        sidebar.querySelector('.is-visible input, .is-visible select')?.focus();
    }));
    availableToggle.addEventListener('click', () => {
        form.elements.available.checked = !form.elements.available.checked;
        form.elements.available.dispatchEvent(new Event('change', { bubbles: true }));
    });
    let request = null;
    let appendRequest = null;
    let observer = null;
    let timer = null;
    let sequence = 0;
    let loadingMore = false;
    let fallbackScroll = null;

    const formUrl = () => {
        const url = new URL(form.action, location.origin);
        for (const [name, value] of new FormData(form)) {
            const trimmed = typeof value === 'string' ? value.trim() : value;
            if (trimmed !== '') url.searchParams.append(name, trimmed);
        }
        return url;
    };

    const syncForm = url => {
        const params = url.searchParams;
        form.elements.discounted.value = params.get('discounted') === 'true' ? 'true' : '';
        form.querySelector('.shop-clear').href = params.get('discounted') === 'true' ? '/shop?discounted=true' : '/shop';
        form.elements.q.value = params.get('q') || '';
        form.elements.sort.value = params.get('sort') || 'newest';
        form.elements.minPrice.value = params.get('minPrice') || '';
        form.elements.maxPrice.value = params.get('maxPrice') || '';
        form.elements.available.checked = params.get('available') === 'true';
        const selectedColors = new Set(params.getAll('color'));
        form.querySelectorAll('input[name="color"]').forEach(input => { input.checked = selectedColors.has(input.value); });
        const selected = new Set(params.getAll('category'));
        form.querySelectorAll('input[name="category"]').forEach(input => {
            input.checked = selected.has(input.value);
            if (input.checked) {
                let parent = input.closest('details');
                while (parent) {
                    parent.open = true;
                    parent = parent.parentElement.closest('details');
                }
            }
        });
    };

    const stopLoading = () => {
        syncMobile();
        results.removeAttribute('aria-busy');
        status.hidden = true;
    };

    const loadMore = async () => {
        const marker = results.querySelector('#shop-load-more');
        const grid = results.querySelector('#shop-product-grid');
        if (!marker || !grid || loadingMore || request && results.hasAttribute('aria-busy')) return;
        const offset = grid.querySelectorAll('.store-product-card').length;
        if (offset >= Number(marker.dataset.total)) { marker.remove(); observer?.disconnect(); return; }
        loadingMore = true;
        let succeeded = false;
        const current = sequence;
        const url = new URL(location.href);
        url.searchParams.set('append', 'true');
        url.searchParams.set('offset', String(offset));
        url.searchParams.set('take', matchMedia('(max-width: 60rem)').matches ? '2' : '3');
        appendRequest = new AbortController();
        marker.textContent = 'در حال بارگذاری محصولات…';
        try {
            const response = await fetch(url, { signal: appendRequest.signal, headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' });
            if (!response.ok) throw new Error('بارگذاری محصولات انجام نشد.');
            const html = await response.text();
            if (current !== sequence) return;
            const template = document.createElement('template');
            template.innerHTML = html;
            const cards = template.content.querySelectorAll('.store-product-card').length;
            grid.append(template.content);
            succeeded = true;
            if (!cards || offset + cards >= Number(marker.dataset.total)) {
                observer?.disconnect();
                marker.remove();
            } else marker.textContent = '';
            window.storeFavorites?.refresh?.().catch(console.error);
        } catch (error) {
            if (error.name !== 'AbortError' && current === sequence) {
                marker.textContent = 'بارگذاری انجام نشد؛ برای تلاش دوباره اینجا بزنید.';
            }
        } finally {
            loadingMore = false;
            if (succeeded && current === sequence && marker.isConnected && marker.getBoundingClientRect().top < innerHeight + 220)
                queueMicrotask(loadMore);
        }
    };

    const watchMore = () => {
        observer?.disconnect();
        if (fallbackScroll) window.removeEventListener('scroll', fallbackScroll);
        fallbackScroll = null;
        const marker = results.querySelector('#shop-load-more');
        if (!marker) return;
        marker.addEventListener('click', loadMore);
        if ('IntersectionObserver' in window) {
            observer = new IntersectionObserver(entries => {
                if (entries.some(entry => entry.isIntersecting)) loadMore();
            }, { rootMargin: '0px 0px 220px 0px' });
            observer.observe(marker);
        } else {
            const check = () => { if (marker.isConnected && marker.getBoundingClientRect().top < innerHeight + 220) loadMore(); };
            fallbackScroll = check;
            window.addEventListener('scroll', check, { passive: true });
            check();
        }
    };

    const load = async (url, updateHistory = true) => {
        request?.abort();
        appendRequest?.abort();
        observer?.disconnect();
        request = new AbortController();
        const current = ++sequence;
        status.hidden = false;
        status.textContent = 'در حال دریافت محصولات…';
        results.setAttribute('aria-busy', 'true');
        try {
            url.searchParams.delete('append');
            url.searchParams.delete('offset');
            url.searchParams.delete('take');
            const response = await fetch(url, { signal: request.signal, headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' });
            if (!response.ok) throw new Error('دریافت محصولات انجام نشد. دوباره تلاش کنید.');
            const template = document.createElement('template');
            template.innerHTML = await response.text();
            if (current !== sequence) return;
            const breadcrumb = template.content.querySelector('#shop-breadcrumb');
            const nextResults = template.content.querySelector('#shop-fragment-results');
            if (!breadcrumb || !nextResults) throw new Error('نمایش محصولات انجام نشد.');
            page.querySelector('#shop-breadcrumb').replaceWith(breadcrumb);
            results.replaceChildren(...nextResults.childNodes);
            const nextColors = template.content.querySelector('#shop-color-options');
            if (nextColors) {
                const previousColors = form.querySelector('#shop-color-options');
                const focusedColor = previousColors.contains(document.activeElement) ? document.activeElement.value : null;
                const scrollTop = previousColors.scrollTop;
                previousColors.replaceWith(nextColors);
                nextColors.scrollTop = scrollTop;
                if (focusedColor) [...nextColors.querySelectorAll('input[name="color"]')]
                    .find(input => input.value === focusedColor)?.focus({ preventScroll: true });
                // Use the server's valid selections when a category/search removes a color.
                url.searchParams.delete('color');
                form.querySelectorAll('input[name="color"]:checked').forEach(input => url.searchParams.append('color', input.value));
                filterColors();
            }
            if (updateHistory && url.href !== location.href) history.pushState(null, '', url);
            syncForm(url);
            document.title = `${breadcrumb.textContent.trim()} - Kohestani`;
            stopLoading();
            watchMore();
            window.storeFavorites?.refresh?.().catch(console.error);
        } catch (error) {
            if (error.name !== 'AbortError' && current === sequence) {
                status.textContent = error.message;
                results.removeAttribute('aria-busy');
                watchMore();
            }
        }
    };

    form.addEventListener('submit', event => { event.preventDefault(); clearTimeout(timer); syncMobile(); load(formUrl()); closeDrawer(); });
    form.addEventListener('change', event => {
        syncMobile();
        if (event.target.matches('input[type="checkbox"], input[type="number"], input[type="radio"]')) { clearTimeout(timer); load(formUrl()); }
    });
    form.elements.q.addEventListener('input', () => {
        syncMobile();
        clearTimeout(timer);
        request?.abort();
        appendRequest?.abort();
        observer?.disconnect();
        sequence++;
        timer = setTimeout(() => load(formUrl()), 400);
    });
    page.addEventListener('click', event => {
        const link = event.target.closest('.shop-sort-options a, .shop-clear, .category-breadcrumbs a[href^="/shop"]');
        if (!link || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey || event.button !== 0) return;
        event.preventDefault();
        clearTimeout(timer);
        load(new URL(link.href));
    });
    window.addEventListener('popstate', () => {
        clearTimeout(timer);
        syncForm(new URL(location.href));
        load(new URL(location.href), false);
    });
    watchMore();
})();
