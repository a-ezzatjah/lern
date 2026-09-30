(() => {
  const all = document.getElementById('settings-all');
  const specific = document.getElementById('settings-specific');
  const search = document.getElementById('settings-search');
  const results = document.getElementById('settings-results');
  const selected = document.getElementById('settings-selected');
  const userId = document.getElementById('settings-user-id');
  all.addEventListener('change', () => { specific.hidden = all.checked; userId.value = ''; selected.textContent = 'کاربری انتخاب نشده است.'; });
  let request;
  search.addEventListener('input', async () => {
    request?.abort(); userId.value = ''; selected.textContent = 'کاربری انتخاب نشده است.';
    const q = search.value.trim(); results.replaceChildren(); results.hidden = true;
    if (q.length < 2 && !/^\d$/.test(q)) return;
    request = new AbortController();
    try {
      const response = await fetch(`/Admin/Coupons/customers?q=${encodeURIComponent(q)}`, { signal: request.signal });
      if (!response.ok) return;
      const users = await response.json();
      if (search.value.trim() !== q) return;
      users.forEach(user => {
        const button = document.createElement('button'); button.type = 'button'; button.setAttribute('role', 'option');
        button.textContent = `${user.firstName} ${user.lastName} — #${user.id} — ${user.phoneNumber}`;
        button.addEventListener('click', () => { userId.value = user.id; selected.textContent = button.textContent; results.hidden = true; });
        results.append(button);
      });
      results.hidden = users.length === 0;
    } catch (error) { if (error.name !== 'AbortError') results.hidden = true; }
  });
})();
