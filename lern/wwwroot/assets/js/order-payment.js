(() => {
    const button = document.getElementById('order-pay');
    if (!button) return;
    button.addEventListener('click', async () => {
        button.disabled = true;
        try {
            const response = await fetch('/api/orders/' + button.dataset.order + '/payment', { method: 'POST', credentials: 'same-origin' });
            const result = await response.json();
            if (!response.ok || !result.succeeded) throw new Error(result.message || 'انتقال به درگاه انجام نشد.');
            window.location.assign(result.redirectUrl);
        } catch (error) { document.getElementById('payment-message').textContent = error.message; button.disabled = false; }
    });
})();
