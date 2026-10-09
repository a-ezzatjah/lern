(() => {
    const button = document.querySelector('[data-mobile-scroll-top]');
    if (!button) return;
    button.addEventListener('click', () => {
        const options = { top: 0, behavior: matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth' };
        const accountContent = document.querySelector('.account-scroll-content');
        if (accountContent) accountContent.scrollTo(options);
        window.scrollTo(options);
    });
})();

(() => {
    const drawer = document.getElementById('mobile-site-menu');
    const trigger = document.querySelector('[data-mobile-menu-open]');
    if (!drawer || !trigger) return;
    const links = drawer.querySelector('[data-mobile-menu-links]');
    const categories = drawer.querySelector('#mobile-site-menu-categories');
    const back = drawer.querySelector('[data-mobile-menu-back]');
    const title = drawer.querySelector('#mobile-site-menu-title');
    const categoryButton = drawer.querySelector('[data-mobile-menu-categories]');
    const showCategories = show => {
        links.hidden = show;
        categories.hidden = !show;
        back.hidden = !show;
        title.textContent = show ? 'دسته‌بندی‌ها' : 'منو';
        drawer.scrollTop = 0;
    };
    categories.querySelectorAll('.category-root').forEach(root => { root.open = false; });
    trigger.addEventListener('click', () => {
        showCategories(false);
        drawer.showModal();
        trigger.setAttribute('aria-expanded', 'true');
    });
    categoryButton.addEventListener('click', () => {
        showCategories(true);
        back.focus();
    });
    back.addEventListener('click', () => {
        showCategories(false);
        categoryButton.focus();
    });
    drawer.querySelector('[data-mobile-menu-close]').addEventListener('click', () => drawer.close());
    drawer.addEventListener('click', event => {
        const bounds = drawer.getBoundingClientRect();
        if (event.target === drawer && (event.clientX < bounds.left || event.clientX > bounds.right || event.clientY < bounds.top || event.clientY > bounds.bottom)) drawer.close();
    });
    drawer.addEventListener('close', () => {
        trigger.setAttribute('aria-expanded', 'false');
        trigger.focus();
    });
    window.addEventListener('resize', () => {
        if (window.matchMedia('(min-width: 64rem)').matches && drawer.open) drawer.close();
    });
})();
