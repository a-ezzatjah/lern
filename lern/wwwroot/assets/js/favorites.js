(() => {
    let ids = new Set(), authenticated = false;
    const pending = new Set();
    const buttons = () => [...document.querySelectorAll('[data-favorite-id]')];
    function render() {
        buttons().forEach(button => {
            const id = Number(button.dataset.favoriteId), active = ids.has(id);
            button.classList.toggle('is-favorite', active);
            button.setAttribute('aria-pressed', String(active));
            button.setAttribute('aria-label', active ? 'حذف از علاقه‌مندی‌ها' : 'افزودن به علاقه‌مندی‌ها');
            button.title = button.getAttribute('aria-label');
            button.querySelector('svg')?.setAttribute('fill', active ? 'currentColor' : 'none');
            button.disabled = pending.has(id);
        });
        document.querySelectorAll('[data-favorite-count]').forEach(el => el.textContent = ids.size.toLocaleString('fa-IR'));
        document.dispatchEvent(new CustomEvent('favorites:changed', { detail: [...ids] }));
    }
    async function refresh() {
        const response = await fetch('/api/favorites', { cache: 'no-store' });
        if (response.status === 401) { authenticated = false; ids = new Set(); render(); return; }
        if (!response.ok) throw new Error('دریافت علاقه‌مندی‌ها انجام نشد. دوباره تلاش کنید.');
        ids = new Set(await response.json()); authenticated = true; render();
    }
    const ready = refresh().catch(error => { console.error(error); });
    async function save(id, active) {
        await ready;
        if (!authenticated) {
            await refresh();
            if (!authenticated) { location.href = '/login?returnUrl=' + encodeURIComponent(location.pathname + location.search); return; }
        }
        if (pending.has(id)) return;
        pending.add(id); render();
        try {
            const token = document.querySelector('#favorite-security input').value;
            const response = await fetch('/api/favorites/' + id, {
                method: active ? 'PUT' : 'DELETE',
                headers: { RequestVerificationToken: token }
            });
            if (response.status === 401) { location.href = '/login?returnUrl=' + encodeURIComponent(location.pathname); return; }
            if (!response.ok) throw new Error('ذخیره علاقه‌مندی انجام نشد. دوباره تلاش کنید.');
            if (active) ids.add(id); else ids.delete(id);
            render();
            await refresh();
        } finally { pending.delete(id); render(); }
    }
    document.addEventListener('click', async event => {
        const button = event.target.closest('[data-favorite-id]');
        if (!button) return;
        event.preventDefault();
        try { await ready; const id = Number(button.dataset.favoriteId); await save(id, !ids.has(id)); }
        catch (error) { alert(error.message); }
    });
    window.addEventListener('focus', () => refresh().catch(console.error));
    window.addEventListener('pageshow', event => { if (event.persisted) refresh().catch(console.error); });
    window.storeFavorites = { save, ready };
})();