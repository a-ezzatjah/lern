(() => {
    const page = document.getElementById('shop-page');
    const form = document.getElementById('shop-filter-form');
    const results = document.getElementById('shop-ajax-results');
    const status = document.getElementById('shop-filter-status');
    if (!page || !form || !results || !status) return;

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
        form.elements.q.value = params.get('q') || '';
        form.elements.sort.value = params.get('sort') || 'newest';
        form.elements.minPrice.value = params.get('minPrice') || '';
        form.elements.maxPrice.value = params.get('maxPrice') || '';
        form.elements.available.checked = params.get('available') === 'true';
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

    form.addEventListener('submit', event => { event.preventDefault(); clearTimeout(timer); load(formUrl()); });
    form.addEventListener('change', event => {
        if (event.target.matches('input[type="checkbox"], input[type="number"]')) { clearTimeout(timer); load(formUrl()); }
    });
    form.elements.q.addEventListener('input', () => {
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
