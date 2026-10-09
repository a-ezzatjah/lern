(() => {
    const feed = document.querySelector('[data-comment-feed]');
    const status = document.querySelector('[data-comment-load-status]');
    if (!feed || !status) return;
    let loading = false;
    feed.addEventListener('click', async event => {
        const link = event.target.closest('[data-comments-more]');
        if (!link || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
        event.preventDefault();
        if (loading) return;
        loading = true;
        link.setAttribute('aria-disabled', 'true');
        feed.setAttribute('aria-busy', 'true');
        status.textContent = 'در حال دریافت نظرات…';
        try {
            const url = new URL(link.href, window.location.href);
            url.searchParams.set('partial', 'true');
            const response = await fetch(url, { credentials: 'same-origin' });
            if (!response.ok || response.redirected) throw new Error('Request failed');
            const documentFragment = new DOMParser().parseFromString(await response.text(), 'text/html');
            const batch = documentFragment.querySelector('[data-comment-batch]');
            const nextPagination = batch?.querySelector('[data-comment-pagination]');
            if (!batch || !nextPagination) throw new Error('Invalid batch');
            const currentPagination = feed.querySelector('[data-comment-pagination]');
            const existing = new Set([...feed.querySelectorAll('[data-comment-id]')].map(item => item.dataset.commentId));
            let added = 0;
            batch.querySelectorAll('[data-comment-id]').forEach(item => {
                if (existing.has(item.dataset.commentId)) return;
                existing.add(item.dataset.commentId);
                currentPagination.before(item);
                added++;
            });
            currentPagination.replaceWith(nextPagination);
            const nextLink = nextPagination.querySelector('[data-comments-more]');
            if (nextLink) nextLink.focus({ preventScroll: true });
            status.textContent = added ? added.toLocaleString('fa-IR') + ' نظر دیگر نمایش داده شد.' : 'نظر دیگری برای نمایش وجود ندارد.';
        } catch {
            status.textContent = 'دریافت نظرات انجام نشد؛ برای تلاش دوباره، نمایش نظرات بعدی را بزنید.';
        } finally {
            loading = false;
            link.removeAttribute('aria-disabled');
            feed.removeAttribute('aria-busy');
        }
    });
})();
