(() => {
  const toggle = document.querySelector('.admin-mobile-toggle');
  const sidebar = document.querySelector('.admin-sidebar');
  const overlay = document.querySelector('.admin-sidebar-overlay');
  if (!toggle || !sidebar || !overlay) return;

  const setOpen = (open) => {
    document.body.classList.toggle('admin-sidebar-open', open);
    toggle.setAttribute('aria-expanded', String(open));
    toggle.setAttribute('aria-label', open ? 'بستن منو' : 'باز کردن منو');
    if (open) sidebar.querySelector('a')?.focus();
    else if (sidebar.contains(document.activeElement)) toggle.focus();
  };

  toggle.addEventListener('click', () => setOpen(!document.body.classList.contains('admin-sidebar-open')));
  overlay.addEventListener('click', () => setOpen(false));
  sidebar.addEventListener('click', (event) => {
    if (event.target.closest('a') && matchMedia('(max-width: 900px)').matches) setOpen(false);
  });
  document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape' && document.body.classList.contains('admin-sidebar-open')) setOpen(false);
  });
  matchMedia('(min-width: 901px)').addEventListener('change', (event) => { if (event.matches) setOpen(false); });
})();
