(() => {
    const modal = document.getElementById('jalali-date-modal');
    const days = document.getElementById('jalali-days');
    const monthSelect = document.getElementById('jalali-month');
    const yearSelect = document.getElementById('jalali-year');
    const months = ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور', 'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'];
    const formatter = new Intl.DateTimeFormat('en-US-u-ca-persian-nu-latn', { year: 'numeric', month: 'numeric', day: 'numeric' });
    const parts = date => {
        const values = Object.fromEntries(formatter.formatToParts(date).filter(x => x.type !== 'literal').map(x => [x.type, Number(x.value)]));
        return { year: values.year, month: values.month, day: values.day };
    };
    const faNumber = value => Number(value).toLocaleString('fa-IR', { useGrouping: false });
    const fromIso = value => {
        if (!/^\d{4}-\d{2}-\d{2}$/.test(value || '')) return null;
        const [year, month, day] = value.split('-').map(Number);
        return new Date(year, month - 1, day, 12);
    };
    const toIso = date => `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
    const findGregorian = (year, month, day) => {
        const cursor = new Date(year + 621, 1, 15, 12);
        for (let i = 0; i < 430; i += 1) {
            const value = parts(cursor);
            if (value.year === year && value.month === month && value.day === day) return new Date(cursor);
            cursor.setDate(cursor.getDate() + 1);
        }
        return null;
    };
    const today = parts(new Date());
    let activeInput, activeButton, calendarYear = today.year, calendarMonth = today.month;
    months.forEach((month, index) => monthSelect.add(new Option(month, String(index + 1))));
    for (let year = today.year + 1; year >= today.year - 120; year -= 1) yearSelect.add(new Option(faNumber(year), String(year)));
    function render() {
        monthSelect.value = String(calendarMonth);
        yearSelect.value = String(calendarYear);
        days.replaceChildren();
        const first = findGregorian(calendarYear, calendarMonth, 1);
        const next = findGregorian(calendarMonth === 12 ? calendarYear + 1 : calendarYear, calendarMonth === 12 ? 1 : calendarMonth + 1, 1);
        if (!first || !next) return;
        for (let i = 0; i < (first.getDay() + 1) % 7; i += 1) days.append(document.createElement('span'));
        const selectedDate = fromIso(activeInput.value);
        const selected = selectedDate && parts(selectedDate);
        for (let day = 1; day <= Math.round((next - first) / 86400000); day += 1) {
            const date = new Date(first);
            date.setDate(first.getDate() + day - 1);
            const button = document.createElement('button');
            button.type = 'button'; button.className = 'jalali-day'; button.textContent = faNumber(day);
            if (selected && selected.year === calendarYear && selected.month === calendarMonth && selected.day === day) button.classList.add('is-selected');
            if (today.year === calendarYear && today.month === calendarMonth && today.day === day) button.classList.add('is-today');
            button.addEventListener('click', () => {
                activeInput.value = toIso(date);
                activeButton.textContent = `${faNumber(calendarYear)}/${faNumber(calendarMonth).padStart(2, '۰')}/${faNumber(day).padStart(2, '۰')}`;
                modal.hidden = true;
            });
            days.append(button);
        }
    }
    document.querySelectorAll('.report-date-button').forEach(button => button.addEventListener('click', () => {
        activeButton = button;
        activeInput = document.getElementById(button.dataset.field);
        const selected = fromIso(activeInput.value);
        const value = selected ? parts(selected) : today;
        calendarYear = value.year; calendarMonth = value.month;
        document.getElementById('jalali-calendar-heading').textContent = button.dataset.field === 'report-from' ? 'انتخاب تاریخ شروع' : 'انتخاب تاریخ پایان';
        render(); modal.hidden = false;
    }));
    document.getElementById('jalali-clear').addEventListener('click', () => { activeInput.value = ''; activeButton.textContent = 'انتخاب تاریخ'; modal.hidden = true; });
    document.getElementById('jalali-cancel').addEventListener('click', () => modal.hidden = true);
    document.getElementById('jalali-prev').addEventListener('click', () => { if (--calendarMonth < 1) { calendarMonth = 12; calendarYear--; } render(); });
    document.getElementById('jalali-next').addEventListener('click', () => { if (++calendarMonth > 12) { calendarMonth = 1; calendarYear++; } render(); });
    monthSelect.addEventListener('change', () => { calendarMonth = Number(monthSelect.value); render(); });
    yearSelect.addEventListener('change', () => { calendarYear = Number(yearSelect.value); render(); });
    modal.addEventListener('click', event => { if (event.target === modal) modal.hidden = true; });
    document.addEventListener('keydown', event => { if (event.key === 'Escape') modal.hidden = true; });
})();
