(() => {
  const panels = [
    { id: 'offcanvas-notification-left', kind: 1 },
    { id: 'offcanvas-user-message-left', kind: 2 }
  ];
  panels.forEach(({ id }) => {
    const main = document.querySelector(`#${id} main`);
    if (main) main.textContent = 'در حال بارگذاری...';
  });
  document.querySelectorAll('button[onclick*="offcanvas-notification-left"] span.absolute, button[onclick*="offcanvas-user-message-left"] span.absolute').forEach(dot => dot.hidden = true);
  fetch('/api/account/inbox', { credentials: 'same-origin' }).then(response => {
    if (!response.ok) throw new Error('دریافت پیام‌ها انجام نشد.');
    return response.json();
  }).then(items => {
    panels.forEach(({ id, kind }) => {
      const main = document.querySelector(`#${id} main`);
      if (!main) return;
      main.replaceChildren();
      const relevant = items.filter(item => item.kind === kind);
      const button = document.querySelector(`button[onclick*="${id}"]`);
      const dot = button?.querySelector('span.absolute');
      if (dot) dot.hidden = relevant.length === 0;
      if (!relevant.length) { main.textContent = kind === 1 ? 'اعلانی برای شما ثبت نشده است.' : 'پیامی برای شما ثبت نشده است.'; return; }
      relevant.forEach(item => {
        const card = document.createElement('article');
        card.className = 'rounded-xl border border-gray-200 bg-white p-4 shadow-sm dark:border-gray-700 dark:bg-card-dark';
        const title = document.createElement('strong'); title.className = 'block text-sm dark:text-white'; title.textContent = item.title;
        const body = document.createElement('p'); body.className = 'mt-2 whitespace-pre-wrap text-sm text-gray-600 dark:text-gray-300'; body.textContent = item.body;
        const date = document.createElement('small'); date.className = 'mt-3 block text-gray-500';
        date.textContent = new Intl.DateTimeFormat('fa-IR-u-ca-persian', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(item.createdAt + (item.createdAt.endsWith('Z') ? '' : 'Z')));
        card.append(title, body, date); main.append(card);
      });
    });
  }).catch(() => panels.forEach(({ id }) => {
    const main = document.querySelector(`#${id} main`);
    if (main) main.textContent = 'دریافت پیام‌ها انجام نشد. صفحه را دوباره بارگذاری کنید.';
  }));
})();
