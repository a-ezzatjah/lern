(() => {
    const tree = document.getElementById('seo-tree');
    if (!tree) return;
    const searchForm = document.getElementById('seo-search-form');
    const searchInput = document.getElementById('seo-search');
    const searchStatus = document.getElementById('seo-search-status');
    let searchTimer, searchRequest, searchVersion = 0;
    const drafts = new Map();
    const editableFields = ['MetaTitle', 'MetaDescription', 'IndexPage', 'FollowPage', 'IncludeInSitemap', 'Description'];
    function fields(form) {
        return editableFields.map(name => form.querySelector(`[name="${name}"]:not([type=hidden])`)).filter(Boolean);
    }
    function readDraft(form) {
        return fields(form).map(field => field.type === 'checkbox' ? field.checked : field.value);
    }
    function writeDraft(form, values) {
        fields(form).forEach((field, i) => {
            if (field.type === 'checkbox') field.checked = values[i];
            else field.value = values[i];
            if (field.matches('[data-rich-editor]')) field.dispatchEvent(new Event('rich-editor:populate'));
        });
    }
    function restoreDrafts() {
        tree.querySelectorAll('[data-seo-form]').forEach(form => {
            const draft = drafts.get(form.elements.Path.value);
            if (!draft || form.dataset.draftRestored) return;
            writeDraft(form, draft);
            form.dataset.draftRestored = 'true';
            form.querySelector('[data-seo-save-status]').textContent = 'تغییرات ذخیره نشده';
        });
    }
    tree.addEventListener('input', event => {
        const form = event.target.closest('[data-seo-form]');
        if (!form || !editableFields.includes(event.target.name)) return;
        drafts.set(form.elements.Path.value, readDraft(form));
        form.dataset.draftRestored = 'true';
        const status = form.querySelector('[data-seo-save-status]');
        status.classList.remove('is-error');
        status.textContent = 'تغییرات ذخیره نشده';
    });

    document.getElementById('seo-collapse-all').addEventListener('click', () => {
        tree.querySelectorAll('details[open]').forEach(details => { details.open = false; });
        tree.querySelectorAll('[data-seo-more]').forEach(button => observer?.unobserve(button));
    });
    async function search() {
        clearTimeout(searchTimer);
        const version = ++searchVersion;
        searchRequest?.abort();
        searchRequest = new AbortController();
        const query = searchInput.value.trim();
        tree.setAttribute('aria-busy', 'true');
        searchStatus.textContent = 'در حال جست‌وجو…';
        try {
            const url = new URL(searchForm.action);
            if (query) url.searchParams.set('q', query);
            const response = await fetch(url, { signal: searchRequest.signal, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok || response.redirected) throw new Error('جست‌وجو انجام نشد. دوباره تلاش کنید یا وارد پنل شوید.');
            const html = await response.text();
            if (version !== searchVersion) return;
            observer?.disconnect();
            tree.innerHTML = html;
            restoreDrafts();
            searchStatus.textContent = query ? `نتایج جست‌وجوی «${query}»؛ برای دیدن موارد، شاخه‌ها را باز کنید.` : 'جست‌وجو شامل موارد داخل شاخه‌های بازنشده هم می‌شود.';
            window.history.replaceState(null, '', url.pathname + url.search);
        } catch (error) {
            if (version === searchVersion && error.name !== 'AbortError') searchStatus.textContent = error.message;
        } finally {
            if (version === searchVersion) tree.removeAttribute('aria-busy');
        }
    }
    searchInput.addEventListener('input', () => {
        // Invalidate a previous response immediately, including during the debounce.
        ++searchVersion;
        searchRequest?.abort();
        clearTimeout(searchTimer);
        searchTimer = setTimeout(search, 300);
    });
    searchForm.addEventListener('submit', event => { event.preventDefault(); search(); });
    document.getElementById('seo-search-clear').addEventListener('click', () => {
        searchInput.value = '';
        searchInput.focus();
        search();
    });
    const observer = 'IntersectionObserver' in window ? new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting && isExpanded(entry.target)) load(entry.target);
        });
    }, { threshold: 0.1 }) : null;

    function isExpanded(element) {
        for (let branch = element.closest('[data-seo-branch]'); branch; branch = branch.parentElement.closest('[data-seo-branch]')) {
            if (!branch.open) return false;
        }
        return true;
    }

    async function loadEditor(branch) {
        if (!branch.dataset.editorUrl || branch.dataset.editorLoaded || branch.dataset.editorLoading || !isExpanded(branch)) return;
        const editor = branch.querySelector(':scope > .seo-tree-content > [data-seo-editor]');
        const retry = branch.querySelector(':scope > .seo-tree-content > [data-seo-editor-retry]');
        branch.dataset.editorLoading = 'true';
        editor.textContent = 'در حال دریافت ویرایشگر…';
        editor.setAttribute('aria-busy', 'true');
        retry.hidden = true;
        try {
            const response = await fetch(branch.dataset.editorUrl, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok || response.redirected) throw new Error('دریافت ویرایشگر انجام نشد. دوباره تلاش کنید یا وارد پنل شوید.');
            const html = await response.text();
            if (!tree.contains(branch)) return;
            const template = document.createElement('template');
            template.innerHTML = html;
            if (!template.content.querySelector('[data-seo-form]')) throw new Error('پاسخ ویرایشگر معتبر نیست. دوباره تلاش کنید.');
            editor.replaceChildren(template.content);
            branch.dataset.editorLoaded = 'true';
            restoreDrafts();
            document.dispatchEvent(new Event('rich-editor:refresh'));
        } catch (error) {
            editor.textContent = error.message;
            retry.hidden = false;
        } finally {
            delete branch.dataset.editorLoading;
            editor.removeAttribute('aria-busy');
        }
    }

    async function load(button) {
        const branch = button.closest('[data-seo-branch]');
        if (button.disabled || button.hidden || !isExpanded(button)) return;
        const list = branch.querySelector(':scope > .seo-tree-content > [data-seo-list]');
        const progress = branch.querySelector(':scope > .seo-tree-content > [data-seo-pagination] > [data-seo-progress]');
        button.disabled = true;
        button.textContent = 'در حال بارگیری…';
        progress.textContent = '';
        try {
            const url = new URL(tree.dataset.childrenUrl || '/Admin/Seo/children', window.location.origin);
            url.searchParams.set('key', branch.dataset.key);
            url.searchParams.set('offset', button.dataset.offset || '0');
            if (branch.dataset.query) url.searchParams.set('q', branch.dataset.query);
            const response = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok || response.redirected) throw new Error('بارگیری انجام نشد. دوباره تلاش کنید یا وارد پنل شوید.');
            const template = document.createElement('template');
            template.innerHTML = await response.text();
            const batch = template.content.querySelector('[data-seo-batch]');
            if (!batch) throw new Error('دریافت فهرست انجام نشد. دوباره تلاش کنید.');
            if (!tree.contains(branch)) return;
            list.append(...batch.children);
            restoreDrafts();
            button.dataset.offset = batch.dataset.nextOffset;
            branch.dataset.loaded = 'true';
            const shown = Number(batch.dataset.nextOffset), total = Number(batch.dataset.total);
            progress.textContent = total ? `نمایش ${shown.toLocaleString('fa-IR')} از ${total.toLocaleString('fa-IR')} مورد` : 'صفحه‌ای در این بخش ثبت نشده است.';
            button.hidden = batch.dataset.hasMore !== 'true';
            button.textContent = 'بارگیری بیشتر — ۱۰ مورد بعدی';
            if (button.hidden) observer?.unobserve(button);
            else if (isExpanded(button)) {
                // Reobserve after every batch so a still-visible button can load
                // the next ten items even when its intersection never changes.
                observer?.unobserve(button);
                observer?.observe(button);
            }
        } catch (error) {
            progress.textContent = error.message;
            button.textContent = 'تلاش دوباره';
            observer?.unobserve(button);
        } finally { button.disabled = false; }
    }

    tree.addEventListener('toggle', event => {
        const branch = event.target;
        if (!branch.matches('[data-seo-branch]')) return;
        const button = branch.querySelector(':scope > .seo-tree-content > [data-seo-pagination] > [data-seo-more]');
        if (branch.open) {
            loadEditor(branch);
            if (!branch.dataset.loaded) load(button);
            else if (!button.hidden) observer?.observe(button);
        }
        // Nested details stay open when their parent closes. Keep their load-more
        // observers in sync with the visibility of the entire ancestor chain.
        branch.querySelectorAll('[data-seo-more]').forEach(more => {
            if (!more.hidden && isExpanded(more) && more.closest('[data-seo-branch]').dataset.loaded)
                observer?.observe(more);
            else observer?.unobserve(more);
        });
    }, true);
    tree.addEventListener('click', event => {
        const button = event.target.closest('[data-seo-more]');
        if (button) load(button);
        if (event.target.closest('[data-seo-editor-retry]')) loadEditor(event.target.closest('[data-seo-branch]'));
    });
    tree.addEventListener('submit', async event => {
        const form = event.target.closest('[data-seo-form]');
        if (!form) return;
        event.preventDefault();
        const button = form.querySelector('button[type=submit]'), status = form.querySelector('[data-seo-save-status]');
        if (button.disabled) return;
        const path = form.elements.Path.value, submitted = readDraft(form);
        button.disabled = true;
        status.classList.remove('is-error');
        status.textContent = 'در حال ذخیره…';
        try {
            const response = await fetch(form.action, { method: 'POST', body: new FormData(form), headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok || response.redirected) throw new Error('ذخیره انجام نشد. دوباره تلاش کنید یا وارد پنل شوید.');
            const result = await response.json();
            if (!result.success) throw new Error('تنظیمات ذخیره نشد.');
            // An edit made while the request was pending is still an unsaved draft.
            const draft = drafts.get(path);
            const newerDraft = draft && JSON.stringify(draft) !== JSON.stringify(submitted);
            if (!newerDraft) drafts.delete(path);
            tree.querySelectorAll('[data-seo-form]').forEach(current => {
                if (current.elements.Path.value !== path) return;
                if (!newerDraft) writeDraft(current, [result.metaTitle ?? '', result.metaDescription ?? '', result.indexPage, result.followPage, result.includeInSitemap, result.description ?? '']);
                current.querySelector('[data-seo-save-status]').textContent = newerDraft ? `${result.message} تغییرات جدید هنوز ذخیره نشده‌اند.` : result.message;
                const page = current.closest('[data-seo-page]');
                const badge = page.querySelector('[data-seo-index]');
                badge.textContent = result.indexPage ? 'index' : 'noindex';
                badge.classList.toggle('is-index', result.indexPage);
                badge.classList.toggle('is-noindex', !result.indexPage);
                const sitemap = page.querySelector('[data-seo-sitemap]');
                sitemap.textContent = result.isInSitemap ? 'در نقشه XML' : 'خارج از نقشه XML';
                sitemap.classList.toggle('is-included', result.isInSitemap);
                sitemap.classList.toggle('is-excluded', !result.isInSitemap);
            });
        } catch (error) { status.textContent = error.message; status.classList.add('is-error'); }
        finally { button.disabled = false; }
    });
})();
