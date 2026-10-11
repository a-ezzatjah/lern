(() => {
    'use strict';
    const byId = id => document.getElementById(id);
    byId('theme-toggle')?.addEventListener('click', () => {
        document.documentElement.classList.toggle('dark');
        localStorage.setItem('theme', document.documentElement.classList.contains('dark') ? 'dark' : 'light');
    });
    document.querySelectorAll('[data-password-toggle]').forEach(button => button.addEventListener('click', () => {
        const input = byId(button.dataset.passwordToggle), show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        button.textContent = show ? 'پنهان' : 'نمایش';
        button.setAttribute('aria-pressed', String(show));
        button.setAttribute('aria-label', show ? 'پنهان کردن رمز عبور' : 'نمایش رمز عبور');
    }));
    const tabs = [...document.querySelectorAll('[role="tab"]')];
    const selectTab = tab => tabs.forEach(item => {
        const selected = item === tab;
        item.setAttribute('aria-selected', String(selected)); item.tabIndex = selected ? 0 : -1;
        byId(item.getAttribute('aria-controls')).hidden = !selected;
    });
    tabs.forEach((tab, index) => {
        tab.addEventListener('click', () => selectTab(tab));
        tab.addEventListener('keydown', event => {
            if (!['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) return;
            event.preventDefault();
            const next = event.key === 'Home' ? 0 : event.key === 'End' ? tabs.length - 1 : (index + (event.key === 'ArrowLeft' ? 1 : -1) + tabs.length) % tabs.length;
            selectTab(tabs[next]); tabs[next].focus();
        });
    });
    function showMessage(element, text, success = false) {
        element.textContent = text; element.hidden = false; element.dataset.success = String(success);
    }
    async function post(url, body) {
        const response = await fetch(url, { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
        const data = await response.json().catch(() => ({}));
        if (!response.ok) {
            const error = new Error(data.message || (response.status === 429 ? 'تعداد درخواست‌ها زیاد است؛ کمی بعد دوباره تلاش کنید.' : 'درخواست انجام نشد؛ دوباره تلاش کنید.'));
            error.retryAfter = Number(response.headers.get('Retry-After')) || 0;
            throw error;
        }
        return data;
    }
    const safeReturn = form => {
        const url = form.dataset.returnUrl || '/account';
        return url.startsWith('/') && !url.startsWith('//') && !url.includes('\\') ? url : '/account';
    };
    const passwordForm = byId('login-form') || byId('register-form');
    passwordForm?.addEventListener('submit', async event => {
        event.preventDefault(); const message = byId('form-message'), button = byId('submit-button'), original = button.textContent;
        message.hidden = true;
        const password = byId('password').value, register = passwordForm.id === 'register-form';
        if (register && password !== byId('confirm-password').value) { showMessage(message, 'تکرار رمز عبور یکسان نیست.'); return; }
        button.disabled = true; button.textContent = register ? 'در حال نام‌نویسی…' : 'در حال ورود…';
        try {
            const body = { phoneNumber: byId('phone').value, password, rememberMe: byId('remember-me')?.checked || false };
            if (register) Object.assign(body, { firstName: byId('first-name').value, lastName: byId('last-name').value, email: byId('email').value || null });
            await post(register ? '/api/account/register' : '/api/account/login', body);
            location.assign(safeReturn(passwordForm));
        } catch (error) { showMessage(message, error.message); }
        finally { button.disabled = false; button.textContent = original; }
    });
    const phoneForm = byId('phone-form');
    if (!phoneForm) return;
    const phone = byId('otp-phone'), send = byId('send-code'), fields = byId('code-fields'), message = byId('phone-message');
    let challengeId = null, countdownTimer = null, expiresAt = 0;
    const reset = phoneForm.dataset.purpose === 'reset';
    function countdown(seconds) {
        clearInterval(countdownTimer); expiresAt = Date.now() + seconds * 1000;
        const tick = () => {
            const remaining = Math.max(0, Math.ceil((expiresAt - Date.now()) / 1000));
            send.disabled = remaining > 0; send.textContent = remaining ? `ارسال مجدد پس از ${remaining} ثانیه` : 'ارسال مجدد کد';
            if (!remaining) clearInterval(countdownTimer);
        };
        tick(); countdownTimer = setInterval(tick, 1000);
    }
    phone.addEventListener('input', () => {
        challengeId = null; fields.hidden = true;
        fields.querySelectorAll('input').forEach(input => { input.required = false; input.value = ''; });
    });
    fetch('/api/account/phone/status', { credentials: 'same-origin' }).then(r => { if (!r.ok) throw new Error(); return r.json(); }).then(data => {
        document.querySelector('[data-sms-status]').textContent = data.message;
    }).catch(() => { document.querySelector('[data-sms-status]').textContent = 'وضعیت سرویس پیامک قابل بررسی نیست؛ کمی بعد دوباره تلاش کنید.'; });
    send.addEventListener('click', async () => {
        if (!phone.reportValidity()) return;
        send.disabled = true; send.textContent = 'در حال ارسال…'; message.hidden = true;
        try {
            const data = await post('/api/account/phone/request', { phoneNumber: phone.value, purpose: phoneForm.dataset.purpose });
            challengeId = data.challengeId; fields.hidden = false;
            fields.querySelectorAll('input:not([type="checkbox"])').forEach(input => input.required = true);
            byId('otp-code').value = ''; byId('otp-code').focus();
            showMessage(message, data.message + ' اعتبار کد ۵ دقیقه است.', true); countdown(data.retryAfter || 60);
        } catch (error) {
            showMessage(message, error.message); send.disabled = false; send.textContent = 'دریافت کد تأیید';
            if (error.retryAfter) countdown(error.retryAfter);
        }
    });
    phoneForm.addEventListener('submit', async event => {
        event.preventDefault();
        if (!challengeId) { showMessage(message, 'ابتدا کد تأیید را دریافت کنید.'); return; }
        if (reset && byId('new-password').value !== byId('confirm-password').value) { showMessage(message, 'تکرار رمز عبور یکسان نیست.'); return; }
        const button = byId('verify-code'), original = button.textContent;
        button.disabled = true; button.textContent = 'در حال تأیید…'; message.hidden = true;
        try {
            const body = { phoneNumber: phone.value, challengeId, code: byId('otp-code').value, rememberMe: byId('otp-remember')?.checked || false };
            if (reset) Object.assign(body, { newPassword: byId('new-password').value, confirmPassword: byId('confirm-password').value });
            const data = await post(reset ? '/api/account/phone/reset-password' : '/api/account/phone/login', body);
            if (reset) {
                challengeId = null; fields.hidden = true;
                fields.querySelectorAll('input').forEach(input => { input.value = ''; input.required = false; });
                showMessage(message, data.message, true); byId('otp-phone').disabled = true; send.hidden = true;
                const loginLink = document.createElement('a'); loginLink.href = '/login'; loginLink.textContent = 'ورود با رمز جدید'; loginLink.className = 'auth-secondary'; phoneForm.append(loginLink);
            } else location.assign(safeReturn(phoneForm));
        } catch (error) { showMessage(message, error.message); }
        finally { button.disabled = false; button.textContent = original; }
    });
})();
