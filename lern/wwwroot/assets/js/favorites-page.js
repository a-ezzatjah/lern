(() => {
    const root = document.getElementById('favorites-page');
    if (!root) return;
    let page = 1;
    const rows = [...root.querySelectorAll('[data-favorite-row]')];
    const category = root.querySelector('#filter-category'), price = root.querySelector('#filter-price'), sort = root.querySelector('#filter-sort');
    const selected = root.querySelector('#favorite-remove-selected');
    const selectAll = root.querySelector('thead input');
    selectAll.removeAttribute('onchange');
    function render() {
        const [min, max] = price.value.split('-');
        let filtered = rows.filter(row => !row.dataset.removed &&
            (!category.value || row.dataset.categories.split(',').includes(category.value)) &&
            (!price.value || (row.dataset.price !== '' && Number(row.dataset.price) >= Number(min) && (!max || Number(row.dataset.price) < Number(max)))));
        if (sort.value === 'oldest') filtered.reverse();
        if (sort.value.startsWith('price_')) filtered.sort((a,b) => {
            if (!a.dataset.price) return 1;
            if (!b.dataset.price) return -1;
            return (Number(a.dataset.price) - Number(b.dataset.price)) * (sort.value === 'price_asc' ? 1 : -1);
        });
        const pages = Math.max(1, Math.ceil(filtered.length / 10));
        page = Math.min(page, pages);
        rows.forEach(row => { row.hidden = true; });
        const visible = filtered.slice((page - 1) * 10, page * 10);
        visible.forEach(row => { row.hidden = false; row.parentElement.append(row); });
        rows.filter(row => row.hidden).forEach(row => row.querySelector('input').checked = false);
        selectAll.checked = visible.length > 0 && visible.every(row => row.querySelector('input').checked);
        selectAll.indeterminate = !selectAll.checked && visible.some(row => row.querySelector('input').checked);
        selected.disabled = !visible.some(row => row.querySelector('input').checked);
        root.querySelector('#favorite-empty').hidden = filtered.length > 0;
        root.querySelector('#favorite-page-info').textContent = 'نمایش ' + (filtered.length ? (page - 1) * 10 + 1 : 0) + ' تا ' + Math.min(page * 10, filtered.length) + ' از ' + filtered.length + ' محصول';
        root.querySelector('#favorite-page').textContent = page.toLocaleString('fa-IR');
        root.querySelector('#favorite-prev').disabled = page === 1;
        root.querySelector('#favorite-next').disabled = page === pages;
    }
    [category, price, sort].forEach(el => el.addEventListener('change', () => { page = 1; render(); }));
    root.querySelector('#favorite-prev').onclick = () => { page--; render(); };
    root.querySelector('#favorite-next').onclick = () => { page++; render(); };
    selectAll.onchange = () => { rows.filter(row => !row.hidden).forEach(row => row.querySelector('input').checked = selectAll.checked); render(); };
    rows.forEach(row => row.querySelector('input').addEventListener('change', render));
    selected.onclick = async () => {
        selected.disabled = true;
        try {
            for (const row of rows.filter(row => !row.hidden && row.querySelector('input').checked))
                await window.storeFavorites.save(Number(row.dataset.favoriteRow), false);
        } catch (error) { alert(error.message); }
        finally { render(); }
    };
    document.addEventListener('favorites:changed', event => {
        rows.forEach(row => { row.dataset.removed = event.detail.includes(Number(row.dataset.favoriteRow)) ? '' : 'true'; });
        // Products added from suggestions need their full server-rendered row.
        if (event.detail.some(id => !rows.some(row => Number(row.dataset.favoriteRow) === id))) { location.reload(); return; }
        render();
    });
    root.querySelector('#favorite-cart-all').onclick = async event => {
        const button = event.currentTarget;
        const available = rows.filter(row => !row.dataset.removed);
        const eligible = available.filter(row => row.dataset.cartVariant);
        if (!available.length) { alert('لیست علاقه‌مندی‌ها خالی است.'); return; }
        button.disabled = true;
        let added = 0;
        try {
            for (const row of eligible) {
                const response = await fetch('/api/cart', {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ productVariantId: Number(row.dataset.cartVariant), quantity: Number(row.dataset.cartQuantity) })
                });
                if (response.ok) added++;
            }
            const skipped = available.length - added;
            alert(added + ' محصول به سبد خرید اضافه شد.' + (skipped ? '\n' + skipped + ' محصول نیاز به انتخاب مدل یا بررسی موجودی در صفحه محصول دارد.' : ''));
        } catch { alert('ارتباط قطع شد. قبل از تلاش دوباره، سبد خرید را بررسی کنید.'); }
        finally { button.disabled = false; }
    };
    render();
})();