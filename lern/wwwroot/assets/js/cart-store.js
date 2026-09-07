(() => {
    const apiUrl = '/api/cart';
    const fallbackImage = '/assets/images/logo.png';
    const formatPrice = value => `${Number(value || 0).toLocaleString('fa-IR')} تومان`;
    const errorText = async response => (await response.text()) || 'عملیات سبد خرید انجام نشد.';

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
        const row = document.createElement('li');
        row.className = 'grid grid-cols-4 gap-4 dark:bg-gray-800 dark:text-white bg-white rounded-lg drop-shadow-lg border-gray-300 border p-4';

        const main = document.createElement('div');
        main.className = 'lg:col-span-3 col-span-4 w-full';
        const content = document.createElement('div');
        content.className = 'flex flex-wrap gap-4';
        const image = document.createElement('img');
        image.src = item.imageUrl || fallbackImage;
        image.alt = item.productName;
        image.className = 'size-32 object-contain';
        image.onerror = () => { image.src = fallbackImage; };

        const details = document.createElement('div');
        details.className = 'flex-1 space-y-5';
        const name = document.createElement('h3');
        name.className = 'font-bold leading-7';
        name.textContent = item.productName;
        const quantityBox = document.createElement('div');
        quantityBox.className = 'inline-flex items-center space-x-2 border rounded-full px-4 py-2 dark:bg-zinc-800 bg-white shadow';
        const increase = document.createElement('button');
        increase.type = 'button';
        increase.className = 'bg-gray-500 text-white w-8 h-8 rounded-full flex items-center justify-center text-lg';
        increase.textContent = '+';
        const quantity = document.createElement('span');
        quantity.className = 'text-lg px-5 inline-block';
        quantity.textContent = item.quantity.toLocaleString('fa-IR');
        const decrease = document.createElement('button');
        decrease.type = 'button';
        decrease.className = 'bg-gray-200 text-gray-600 w-8 h-8 rounded-full flex items-center justify-center text-lg';
        decrease.textContent = '−';
        decrease.disabled = item.quantity <= 1;
        const remove = document.createElement('button');
        remove.type = 'button';
        remove.className = 'ms-2 rounded-full bg-primary-grad p-2 text-white';
        remove.textContent = 'حذف';

        const run = async action => {
            [increase, decrease, remove].forEach(button => button.disabled = true);
            try {
                await action();
                await refresh();
            } catch (error) {
                showPageMessage(error.message, true);
                [increase, decrease, remove].forEach(button => button.disabled = false);
            }
        };
        increase.addEventListener('click', () => run(() => changeQuantity(item.id, item.quantity + 1)));
        decrease.addEventListener('click', () => run(() => changeQuantity(item.id, item.quantity - 1)));
        remove.addEventListener('click', () => run(() => removeItem(item.id)));
        quantityBox.append(increase, quantity, decrease);
        details.append(name, quantityBox, remove);
        content.append(image, details);
        main.append(content);

        const priceColumn = document.createElement('div');
        priceColumn.className = 'lg:col-span-1 col-span-4 w-full flex xl:items-end xl:justify-end';
        const price = document.createElement('strong');
        price.className = 'text-xl block font-bold dark:text-white';
        price.textContent = formatPrice(Number(item.price) * item.quantity);
        priceColumn.append(price);
        row.append(main, priceColumn);
        return row;
    };

    const render = items => {
        const total = items.reduce((sum, item) => sum + (Number(item.price) * item.quantity), 0);
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
        if (pageItems) {
            pageItems.replaceChildren();
            if (!items.length) {
                const empty = document.createElement('p');
                empty.className = 'py-10 text-center text-gray-500';
                empty.textContent = 'سبد خرید شما خالی است.';
                pageItems.append(empty);
            } else items.forEach(item => pageItems.append(createItem(item, false)));
        }
        if (pageTotal) pageTotal.textContent = formatPrice(total);
        if (pageSubtotal) pageSubtotal.textContent = formatPrice(total);
        if (summaryValues.length) {
            summaryValues[0].textContent = formatPrice(total);
            if (summaryValues.length > 1) summaryValues[1].textContent = formatPrice(0);
            if (summaryValues.length > 2) summaryValues[2].textContent = formatPrice(total);
        }
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
            return [];
        }
    };

    window.storeCart = { refresh };
    document.addEventListener('DOMContentLoaded', () => {
        document.getElementById('cart-drawer-button')?.addEventListener('click', async () => {
            await refresh();
            openDrawer();
        });
        refresh();
    });
})();
