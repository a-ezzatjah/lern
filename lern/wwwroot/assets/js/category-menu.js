(() => {
    const trigger = document.getElementById('category-menu-trigger');
    const button = trigger?.querySelector('button');
    const panel = document.getElementById('category-menu-panel');
    const mobile = document.getElementById('mobile-category-content');
    if (!button || !panel || !mobile) return;
    let closeTimer;
    const setOpen = open => {
        clearTimeout(closeTimer);
        panel.hidden = !open;
        button.setAttribute('aria-expanded', String(open));
    };
    button.addEventListener('click', () => setOpen(panel.hidden));
    trigger.addEventListener('pointerenter', () => clearTimeout(closeTimer));
    trigger.addEventListener('pointerleave', () => {
        closeTimer = setTimeout(() => {
            if (!trigger.contains(document.activeElement)) setOpen(false);
        }, 180);
    });
    trigger.addEventListener('focusout', event => {
        if (!trigger.contains(event.relatedTarget)) setOpen(false);
    });
    document.addEventListener('click', event => {
        if (!trigger.contains(event.target)) setOpen(false);
    });
    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && !panel.hidden) {
            setOpen(false);
            button.focus();
        }
        if (event.key === 'Escape' && document.getElementById('offcanvas-right')?.classList.contains('visible')) {
            window.closeOffcanvas();
            document.querySelector('.category-mobile-trigger')?.focus();
        }
    });
    window.addEventListener('resize', () => setOpen(false));
    async function loadCategories() {
        try {
            const response = await fetch('/menu/categories');
            if (!response.ok) throw new Error('Category request failed');
            const html = await response.text();
            if (!html.trim()) throw new Error('Empty response');
            panel.innerHTML = html;
            mobile.innerHTML = html;
            mobile.querySelectorAll('.category-root').forEach(root => { root.open = false; });
            const roots = [...panel.querySelectorAll('.category-root')];
            const activate = root => roots.forEach(item => { item.open = item === root; });
            roots.forEach(root => {
                const summary = root.querySelector('summary');
                summary.addEventListener('click', event => {
                    if (event.target.closest('a')) return;
                    event.preventDefault();
                    activate(root);
                });
                summary.addEventListener('pointerenter', event => {
                    if (event.pointerType === 'mouse') activate(root);
                });
                summary.addEventListener('focus', () => activate(root));
            });
        } catch {
            [panel, mobile].forEach(container => {
                container.innerHTML = '<div class="category-loading" role="status">دریافت دسته‌بندی‌ها انجام نشد.<button type="button">تلاش دوباره</button></div>';
                container.querySelector('button').addEventListener('click', loadCategories);
            });
        }
    }
    loadCategories();
})();
