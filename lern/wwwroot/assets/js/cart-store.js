(() => {
    const apiUrl = '/api/cart';
    const fallbackImage = '/assets/images/logo.png';
    const formatPrice = value => `${Number(value || 0).toLocaleString('fa-IR')} تومان`;
    const errorText = async response => (await response.text()) || 'عملیات سبد خرید انجام نشد.';
    let pageItemTemplate;

    const getCart = async () => {
        const response = await fetch(apiUrl, { credentials: 'same-origin' });
        if (!response.ok) throw new Error(await errorText(response));
        return response.json();
    };

    const changeQuantity = async (id, quantity) => {
        const response = await fetch(`${apiUrl}/${id}`, {
            method: 'PUT',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ quantity })
        });
        if (!response.ok) throw new Error(await errorText(response));
    };

    const removeItem = async id => {
        const response = await fetch(`${apiUrl}/${id}`, { method: 'DELETE', credentials: 'same-origin' });
        if (!response.ok) throw new Error(await errorText(response));
    };

    const createItem = (item, compact) => {
        if (!compact) return createPageItem(item);

        const element = document.createElement('article');
        element.className = 'flex gap-3 py-3';

        const image = document.createElement('img');
        image.src = item.imageUrl || fallbackImage;
        image.alt = item.productName;
        image.className = 'h-20 w-20 rounded-lg object-contain';
        image.onerror = () => { image.src = fallbackImage; };

        const details = document.createElement('div');
        details.className = 'min-w-0 flex-1';
        const name = document.createElement('h3');
        name.className = 'font-bold leading-7 dark:text-white';
        name.textContent = item.productName;
        const price = document.createElement('p');
        price.className = 'mt-1 font-bold text-green-600';
        price.textContent = formatPrice(item.price);

        if (Number(item.discountPercent) > 0) {
            const discount = document.createElement('span');
            discount.className = 'mt-1 inline-block text-xs font-bold text-primary';
            discount.textContent = `${Number(item.discountPercent).toLocaleString('fa-IR')}٪ تخفیف`;
            details.append(discount);
        }

        const controls = document.createElement('div');
        controls.className = 'mt-3 flex items-center gap-2';
        const decrease = document.createElement('button');
        decrease.type = 'button';
        decrease.className = 'rounded-md border px-3 py-1 dark:text-white';
        decrease.textContent = '−';
        decrease.disabled = item.quantity <= 1;
        const quantity = document.createElement('span');
        quantity.className = 'min-w-8 text-center text-sm dark:text-white';
        quantity.textContent = item.quantity.toLocaleString('fa-IR');
        const increase = document.createElement('button');
        increase.type = 'button';
        increase.className = 'rounded-md border px-3 py-1 dark:text-white';
        increase.textContent = '+';
        const remove = document.createElement('button');
        remove.type = 'button';
        remove.className = 'mr-auto text-sm text-red-600';
        remove.textContent = 'حذف';

        const run = async action => {
            [decrease, increase, remove].forEach(button => button.disabled = true);
            try {
                await action();
                await refresh();
            } catch (error) {
                showPageMessage(error.message, true);
                [decrease, increase, remove].forEach(button => button.disabled = false);
            }
        };
        decrease.addEventListener('click', () => run(() => changeQuantity(item.id, item.quantity - 1)));
        increase.addEventListener('click', () => run(() => changeQuantity(item.id, item.quantity + 1)));
        remove.addEventListener('click', () => run(() => removeItem(item.id)));

        controls.append(decrease, quantity, increase, remove);
        details.append(name, price, controls);
        element.append(image, details);
        return element;
    };

    const createPageItem = item => {
        const row = pageItemTemplate.cloneNode(true);
        const image = row.querySelector('img');
        const name = row.querySelector('h3');
        const priceValues = row.querySelectorAll('[itemprop="price"]');
        const originalPrice = row.querySelector('.cart-original-price');
        const discount = row.querySelector('.cart-discount');
        const variant = row.querySelector('.cart-variant');
        const color = row.querySelector('.cart-color');
        const quantity = row.querySelector('[id^="count"]');
        const quantityButtons = quantity?.parentElement.querySelectorAll('button') || [];
        const increase = quantityButtons[0];
        const decrease = quantityButtons[1];
        const remove = row.querySelector('a.bg-primary-grad');
        const lineTotal = Number(item.price) * item.quantity;

        if (image) {
            image.src = item.imageUrl || fallbackImage;
            image.alt = item.productName;
            image.onerror = () => { image.src = fallbackImage; };
        }
        if (name) name.textContent = item.productName;
        if (priceValues[0]) priceValues[0].textContent = formatPrice(lineTotal);
        if (priceValues[1]) {
            priceValues[1].textContent = formatPrice(lineTotal);
            priceValues[1].setAttribute('content', lineTotal);
        }
        if (originalPrice) {
            const originalLineTotal = Number(item.originalPrice) * item.quantity;
            originalPrice.textContent = formatPrice(originalLineTotal);
            originalPrice.classList.toggle('hidden', Number(item.discountAmount) <= 0);
        }
        if (discount) {
            discount.textContent = Number(item.discountPercent) > 0 ? `${Number(item.discountPercent).toLocaleString('fa-IR')}٪` : '';
            discount.classList.toggle('hidden', Number(item.discountPercent) <= 0);
        }
        if (variant) variant.textContent = item.saleOptionTitle || '—';
        if (color) {
            color.textContent = item.color || '—';
            color.closest('.cart-product-color')?.classList.toggle('hidden', !item.color);
        }
        if (quantity) {
            quantity.removeAttribute('id');
            quantity.textContent = item.quantity.toLocaleString('fa-IR');
        }

        [increase, decrease].forEach(button => button?.removeAttribute('onclick'));
        if (decrease) decrease.disabled = item.quantity <= 1;
        if (remove) {
            remove.href = '#';
            remove.setAttribute('aria-label', `حذف ${item.productName} از سبد خرید`);
        }

        const run = async action => {
            [increase, decrease].forEach(button => { if (button) button.disabled = true; });
            remove?.classList.add('pointer-events-none', 'opacity-60');
            try {
                await action();
                await refresh();
            } catch (error) {
                showPageMessage(error.message, true);
                if (increase) increase.disabled = false;
                if (decrease) decrease.disabled = item.quantity <= 1;
                remove?.classList.remove('pointer-events-none', 'opacity-60');
            }
        };

        increase?.addEventListener('click', () => run(() => changeQuantity(item.id, item.quantity + 1)));
        decrease?.addEventListener('click', () => run(() => changeQuantity(item.id, item.quantity - 1)));
        remove?.addEventListener('click', event => {
            event.preventDefault();
            run(() => removeItem(item.id));
        });
        return row;
    };

    const render = items => {
        const total = items.reduce((sum, item) => sum + (Number(item.price) * item.quantity), 0);
        const subtotal = items.reduce((sum, item) => sum + (Number(item.originalPrice) * item.quantity), 0);
        const discountTotal = subtotal - total;
        const count = items.reduce((sum, item) => sum + item.quantity, 0);
        const countElement = document.getElementById('cart-count');
        if (countElement) countElement.textContent = count.toLocaleString('fa-IR');

        const drawerItems = document.getElementById('cart-drawer-items');
        const drawerTotal = document.getElementById('cart-drawer-total');
        if (drawerItems) {
            drawerItems.replaceChildren();
            if (!items.length) {
                const empty = document.createElement('p');
                empty.className = 'py-8 text-center text-sm text-gray-500';
                empty.textContent = 'سبد خرید شما خالی است.';
                drawerItems.append(empty);
            } else items.forEach(item => drawerItems.append(createItem(item, true)));
        }
        if (drawerTotal) drawerTotal.textContent = formatPrice(total);

        const pageItems = document.getElementById('cart-page-items');
        const pageTotal = document.getElementById('cart-page-total');
        const pageSubtotal = document.getElementById('cart-page-subtotal');
        const summaryValues = document.querySelectorAll('#cart-page-summary p');
        if (pageItems && pageItemTemplate) {
            pageItems.replaceChildren();
            if (!items.length) {
                const empty = document.createElement('li');
                empty.className = 'rounded-lg border border-gray-300 bg-white p-8 text-center text-gray-500 drop-shadow-lg dark:bg-gray-800 dark:text-gray-300';
                empty.textContent = 'سبد خرید شما خالی است.';
                pageItems.append(empty);
            } else items.forEach(item => pageItems.append(createPageItem(item)));
        }
        if (pageTotal) pageTotal.textContent = formatPrice(total);
        if (pageSubtotal) pageSubtotal.textContent = formatPrice(total);
        if (summaryValues.length) {
            summaryValues[0].textContent = formatPrice(subtotal);
            if (summaryValues.length > 1) summaryValues[1].textContent = formatPrice(discountTotal);
            if (summaryValues.length > 2) summaryValues[2].textContent = formatPrice(total);
        }
        const pageContent = document.getElementById('cart-page-content');
        pageContent?.classList.remove('hidden');
        pageContent?.style.removeProperty('display');
    };

    const showPageMessage = (message, isError = false) => {
        const target = document.getElementById('cart-page-message');
        if (!target) return;
        target.textContent = message || '';
        target.className = isError ? 'mb-3 text-sm text-red-600' : 'mb-3 text-sm text-green-600';
    };

    const openDrawer = () => {
        if (typeof window.toggleOffcanvas === 'function') window.toggleOffcanvas('offcanvas-left');
    };

    const refresh = async ({ open = false } = {}) => {
        try {
            const items = await getCart();
            render(items);
            if (open) openDrawer();
            return items;
        } catch (error) {
            showPageMessage(error.message, true);
            const pageContent = document.getElementById('cart-page-content');
            pageContent?.classList.remove('hidden');
            pageContent?.style.removeProperty('display');
            return [];
        }
    };

    window.storeCart = { refresh };
    document.addEventListener('DOMContentLoaded', () => {
        const pageItems = document.getElementById('cart-page-items');
        pageItemTemplate = pageItems?.firstElementChild?.cloneNode(true);
        pageItems?.replaceChildren();
        document.getElementById('cart-drawer-button')?.addEventListener('click', async () => {
            await refresh();
            openDrawer();
        });
        refresh();
    });
})();
