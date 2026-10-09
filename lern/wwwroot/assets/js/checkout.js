(() => {
    const byId = id => document.getElementById(id);
    const money = value => Number(value || 0).toLocaleString('fa-IR') + ' تومان';
    const message = (text, error = false) => { byId('checkout-message').textContent = text; byId('checkout-message').classList.toggle('is-error', error); };
    const request = async (url, options = {}) => {
        const response = await fetch(url, { credentials: 'same-origin', cache: 'no-store', ...options });
        if (!response.ok) { const body = await response.text(); let error = body; try { error = JSON.parse(body).message || JSON.parse(body).Message || body; } catch {} throw new Error(error || 'ارتباط با فروشگاه انجام نشد.'); }
        return response.status === 204 ? null : response.json();
    };
    let items = [], addresses = [], selectedAddress = null, couponCode = '', couponDiscount = 0, paymentAvailable = false;
    let registeredOrder = null;
    const touched = new Set();
    ['checkout-first-name', 'checkout-last-name', 'checkout-phone'].forEach(id => byId(id).addEventListener('input', () => touched.add(id)));
    const tokenKey = 'lern.checkout.token';
    let checkoutToken = sessionStorage.getItem(tokenKey);
    if (!checkoutToken) { checkoutToken = crypto.randomUUID(); sessionStorage.setItem(tokenKey, checkoutToken); }
    const subtotal = () => items.reduce((sum, x) => sum + Number(x.price) * x.quantity, 0);
    const totals = () => {
        const original = items.reduce((sum, x) => sum + Number(x.originalPrice ?? x.price) * x.quantity, 0);
        byId('checkout-subtotal').textContent = money(original);
        byId('checkout-product-discount').textContent = money(original - subtotal());
        byId('checkout-coupon-discount').textContent = money(couponDiscount);
        byId('checkout-total').textContent = money(Math.max(0, subtotal() - couponDiscount));
    };
    const renderAddresses = () => {
        const box = byId('checkout-address'); box.replaceChildren();
        if (!addresses.length) { box.textContent = 'برای خرید مهمان یا با حساب کاربری، ابتدا آدرس تحویل را اضافه کنید.'; return; }
        selectedAddress = addresses.find(x => x.isDefault) || addresses[0];
        addresses.forEach(address => {
            const label = document.createElement('label'); label.className = 'checkout-address-choice';
            const radio = document.createElement('input'); radio.type = 'radio'; radio.name = 'checkout-address'; radio.value = address.id; radio.checked = address.id === selectedAddress.id;
            const text = document.createElement('div');
            const title = document.createElement('strong'); title.textContent = address.title;
            const details = document.createElement('span'); details.textContent = [address.province, address.city, address.details].join('، ');
            const receiver = document.createElement('span'); receiver.textContent = 'گیرنده: ' + address.receiverName + ' — کد پستی: ' + address.postalCode;
            text.append(title, details, receiver); label.append(radio, text); box.append(label);
            radio.addEventListener('change', () => { selectedAddress = address; showShipping(); });
        }); showShipping();
    };
    const showShipping = () => {
        const province = selectedAddress?.province.replace('استان', '').trim();
        byId('checkout-shipping-method').textContent = province === 'تهران' ? 'ارسال با پیک' : 'ارسال با باربری و روش‌های ارسال دیگر (انتخاب توسط مدیر)';
    };
    byId('checkout-coupon-code').addEventListener('input', () => { couponCode = ''; couponDiscount = 0; totals(); });
    byId('checkout-apply-coupon').addEventListener('click', async event => {
        const button = event.currentTarget, code = byId('checkout-coupon-code').value.trim();
        if (!code) { message('کد تخفیف را وارد کنید.', true); return; }
        button.disabled = true;
        try { const result = await request('/api/orders/coupon/preview', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ code }) });
            if (byId('checkout-coupon-code').value.trim().toUpperCase() !== result.code) return;
            couponCode = result.code; couponDiscount = Number(result.discount); totals(); message('کد تخفیف اعمال شد. کرایهٔ ارسال در محل پرداخت می‌شود.');
        } catch (error) { message(error.message, true); } finally { button.disabled = false; }
    });
    byId('checkout-submit').addEventListener('click', async event => {
        const button = event.currentTarget;
        if (!selectedAddress) { message('ابتدا آدرس تحویل را اضافه یا انتخاب کنید.', true); return; }
        const firstName = byId('checkout-first-name').value.trim(), lastName = byId('checkout-last-name').value.trim(), phone = byId('checkout-phone').value.trim();
        if (!firstName || !lastName || !phone) { message('نام، نام خانوادگی و شماره موبایل را کامل کنید.', true); return; }
        button.disabled = true;
        try {
            if (!registeredOrder) {
                registeredOrder = await request('/api/orders/checkout', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ shippingCost: 0, firstName, lastName, phone, addressId: selectedAddress.id, checkoutToken, couponCode: couponCode || null }) });
                sessionStorage.removeItem(tokenKey);
                const receipt = byId('checkout-receipt'); receipt.href = '/orders/' + registeredOrder.id; receipt.hidden = false;
                window.storeCart?.refresh();
            }
            message('سفارش شماره ' + registeredOrder.id + ' به مبلغ ' + money(registeredOrder.total) + ' ثبت شد. هزینهٔ ارسال در محل دریافت می‌شود؛ مبلغ کالاها هنوز پرداخت نشده است.');
            if (registeredOrder.paymentAvailable) {
                const payment = await request('/api/orders/' + registeredOrder.id + '/payment', { method: 'POST' });
                window.location.assign(payment.redirectUrl);
            } else { button.textContent = 'سفارش ثبت شد — درگاه هنوز فعال نیست'; }
        } catch (error) { message(error.message, true); button.disabled = false; button.textContent = registeredOrder ? 'تلاش دوباره برای پرداخت' : 'ثبت سفارش'; }
    });
    (async () => {
        try {
            items = await request('/api/cart');
            addresses = await request('/api/address/list') || []; renderAddresses();
            const status = await request('/api/payments/status'); paymentAvailable = status.available;
            byId('checkout-payment-notice').textContent = status.message;
            byId('checkout-submit').textContent = paymentAvailable ? 'ثبت سفارش و پرداخت مبلغ کالاها' : 'ثبت سفارش';
            const box = byId('checkout-items');
            items.forEach(item => { const row = document.createElement('article'); row.className = 'checkout-item'; const img = document.createElement('img'); img.src = item.imageUrl || '/assets/images/logo.png'; img.alt = item.productName;
                const details = document.createElement('div'), title = document.createElement('h3'), text = document.createElement('p'); title.textContent = item.productName; text.textContent = 'تعداد: ' + item.quantity.toLocaleString('fa-IR') + ' — ' + money(item.price * item.quantity); details.append(title, text); row.append(img, details); box.append(row); });
            totals(); byId('checkout-submit').disabled = !items.length;
            if (!items.length) message('سبد خرید خالی است.', true);
            try { const profile = await request('/api/account/me'); for (const [id, key] of [['checkout-first-name', 'firstName'], ['checkout-last-name', 'lastName'], ['checkout-phone', 'phoneNumber']]) if (!touched.has(id)) byId(id).value = profile[key] || ''; } catch { /* Guest checkout needs no account. */ }
        } catch (error) { message(error.message, true); }
    })();
})();
