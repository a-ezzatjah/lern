(() => {
    const initialize = textarea => {
        if (textarea.dataset.richInitialized) return;
        textarea.dataset.richInitialized = 'true';
        const wrapper = document.createElement('div');
        wrapper.className = 'rich-editor';
        wrapper.innerHTML = `<div class="rich-toolbar" role="toolbar" aria-label="قالب‌بندی متن">
            <select aria-label="نوع تیتر"><option value="p">متن معمولی</option>${[1,2,3,4,5,6].map(i => `<option value="h${i}">H${i} — تیتر ${i}</option>`).join('')}</select>
            <button type="button" data-command="bold" aria-label="بولد کردن متن"><strong>B</strong></button>
            <button type="button" data-command="italic" aria-label="ایتالیک"><em>I</em></button>
            <button type="button" data-command="insertUnorderedList">فهرست</button>
            <button type="button" data-link>درج / ویرایش لینک</button>
            <button type="button" data-command="unlink">حذف لینک</button>
            <button type="button" data-command="removeFormat">پاک کردن قالب</button>
        </div><div class="rich-editor-body rich-content" contenteditable="true" role="textbox" aria-multiline="true" dir="rtl"></div>
        <p class="rich-editor-help">متن را انتخاب کنید و قالب یا لینک بدهید. عنوان اصلی صفحه H1 است؛ برای بخش‌ها معمولاً H2 و H3 مناسب است.</p>`;
        textarea.after(wrapper);
        const editor = wrapper.querySelector('[contenteditable]');
        editor.setAttribute('aria-label', textarea.closest('label')?.childNodes[0]?.textContent.trim() || 'ویرایش متن');
        const required = textarea.required;
        textarea.required = false;
        textarea.hidden = true;
        const populate = () => {
            const value = textarea.value;
            if (/<\/?(?:p|div|h[1-6]|strong|b|a|br|ul|ol|li|em|i|blockquote)\b/i.test(value)) {
                const parsed = new DOMParser().parseFromString(value, 'text/html');
                const tags = new Set(['P','DIV','SPAN','BR','H1','H2','H3','H4','H5','H6','STRONG','B','EM','I','U','A','UL','OL','LI','BLOCKQUOTE']);
                parsed.body.querySelectorAll('*').forEach(element => {
                    if (!tags.has(element.tagName)) { element.remove(); return; }
                    [...element.attributes].forEach(attribute => {
                        if (element.tagName !== 'A' || !['href','title','rel','target'].includes(attribute.name)) element.removeAttribute(attribute.name);
                    });
                    if (element.tagName === 'A') {
                        const href = element.getAttribute('href') || '';
                        if (!/^(https?:\/\/|\/(?!\/)|#)/i.test(href) || /[\s\\]/.test(href)) element.removeAttribute('href');
                    }
                });
                editor.innerHTML = parsed.body.innerHTML;
            }
            else {
                editor.replaceChildren();
                value.replaceAll('\r\n', '\n').split('\n\n').forEach(text => {
                    const paragraph = document.createElement(text.startsWith('## ') ? 'h2' : 'p');
                    paragraph.textContent = text.startsWith('## ') ? text.slice(3) : text;
                    editor.append(paragraph);
                });
            }
        };
        populate();
        textarea.addEventListener('rich-editor:populate', populate);
        const sync = () => {
            textarea.value = editor.innerHTML;
            textarea.dispatchEvent(new Event('input', { bubbles: true }));
        };
        let range;
        const remember = () => {
            const selection = window.getSelection();
            if (selection.rangeCount && editor.contains(selection.getRangeAt(0).commonAncestorContainer)) range = selection.getRangeAt(0).cloneRange();
        };
        const restore = () => {
            editor.focus();
            if (range) { const selection = window.getSelection(); selection.removeAllRanges(); selection.addRange(range); }
        };
        const command = (name, value) => { restore(); document.execCommand('styleWithCSS', false, false); document.execCommand(name, false, value); remember(); sync(); };
        editor.addEventListener('input', sync);
        editor.addEventListener('keyup', remember);
        editor.addEventListener('mouseup', remember);
        editor.addEventListener('paste', event => {
            // Paste only text; HTML formatting is added by the toolbar.
            event.preventDefault();
            document.execCommand('insertText', false, event.clipboardData.getData('text/plain'));
            sync();
        });
        wrapper.querySelectorAll('button').forEach(button => button.addEventListener('mousedown', event => event.preventDefault()));
        wrapper.querySelectorAll('[data-command]').forEach(button => button.addEventListener('click', () => command(button.dataset.command)));
        wrapper.querySelector('select').addEventListener('change', event => command('formatBlock', event.target.value));
        const dialog = document.createElement('dialog');
        dialog.className = 'rich-link-dialog';
        dialog.innerHTML = `<form method="dialog" class="seo-settings-form">
            <h2>درج لینک روی متن انتخاب‌شده</h2>
            <label>نشانی لینک<input data-url type="text" dir="ltr" required placeholder="/product/1 یا https://example.com"></label>
            <label>متن لینک<input data-text required></label>
            <label>عنوان لینک<input data-title></label>
            <label><input data-blank type="checkbox"> باز شدن در صفحه جدید</label>
            <label>نوع رابطه<select data-rel><option value="">لینک معمولی (follow)</option><option value="nofollow">nofollow</option><option value="sponsored">تبلیغاتی (sponsored)</option><option value="ugc">محتوای کاربران (ugc)</option></select></label>
            <p data-error role="alert"></p>
            <div><button type="submit" class="admin-primary-button">ثبت لینک</button> <button type="button" data-cancel>انصراف</button></div>
        </form>`;
        // Keep the link dialog outside the editing form. On lazy tree pages its
        // container also removes the dialog when search replaces the branch.
        (textarea.closest('[data-seo-editor]') || document.body).append(dialog);
        let existingLink;
        wrapper.querySelector('[data-link]').addEventListener('click', () => {
            remember();
            const node = range?.startContainer;
            existingLink = (node?.nodeType === Node.ELEMENT_NODE ? node : node?.parentElement)?.closest('a');
            dialog.querySelector('[data-url]').value = existingLink?.getAttribute('href') || '';
            dialog.querySelector('[data-text]').value = existingLink?.textContent || range?.toString() || '';
            dialog.querySelector('[data-title]').value = existingLink?.title || '';
            dialog.querySelector('[data-blank]').checked = existingLink?.target === '_blank';
            dialog.querySelector('[data-rel]').value = ['nofollow','sponsored','ugc'].find(x => existingLink?.rel.split(' ').includes(x)) || '';
            dialog.querySelector('[data-error]').textContent = '';
            dialog.showModal();
        });
        dialog.querySelector('[data-cancel]').addEventListener('click', () => dialog.close());
        dialog.querySelector('form').addEventListener('submit', event => {
            event.preventDefault();
            const href = dialog.querySelector('[data-url]').value.trim();
            if (!(/^(https?:\/\/|\/(?!\/)|#)/i.test(href)) || /[\s\\]/.test(href)) {
                dialog.querySelector('[data-error]').textContent = 'لینک باید داخلی با / یا # شروع شود، یا نشانی کامل http / https باشد.';
                return;
            }
            restore();
            const link = existingLink || document.createElement('a');
            link.href = href;
            link.textContent = dialog.querySelector('[data-text]').value;
            link.title = dialog.querySelector('[data-title]').value;
            link.target = dialog.querySelector('[data-blank]').checked ? '_blank' : '';
            link.rel = [dialog.querySelector('[data-rel]').value, link.target ? 'noopener noreferrer' : ''].filter(Boolean).join(' ');
            if (!existingLink) {
                const selection = window.getSelection();
                if (selection.rangeCount && editor.contains(selection.getRangeAt(0).commonAncestorContainer)) {
                    const selected = selection.getRangeAt(0); selected.deleteContents(); selected.insertNode(link);
                } else editor.append(link);
            }
            sync(); dialog.close();
        });
        textarea.form?.addEventListener('submit', event => {
            sync();
            if (required && !editor.textContent.trim()) {
                event.preventDefault(); event.stopImmediatePropagation(); editor.focus();
                window.alert('متن را وارد کنید.');
            }
        }, true);
        textarea.form?.addEventListener('formdata', event => event.formData.set(textarea.name, editor.innerHTML));
        textarea.form?.addEventListener('reset', () => setTimeout(populate, 0));
    };
    const scan = () => document.querySelectorAll('textarea[data-rich-editor]').forEach(textarea => {
        if (textarea.dataset.richInitialized || textarea.dataset.richWaiting) return;
        const details = textarea.closest('details');
        if (details && !details.open) {
            textarea.dataset.richWaiting = 'true';
            const onOpen = () => {
                if (!details.open) return;
                details.removeEventListener('toggle', onOpen);
                initialize(textarea);
            };
            details.addEventListener('toggle', onOpen);
        } else initialize(textarea);
    });
    document.addEventListener('rich-editor:refresh', scan);
    scan();
})();
