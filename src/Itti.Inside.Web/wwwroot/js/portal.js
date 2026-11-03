// Small page behaviours for the portal. Loaded on every page, so each block checks for its elements.
(() => {
    'use strict';

    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    const showToggleState = (button, paused, noun) => {
        const use = button.querySelector('use');
        const href = use.getAttribute('href');

        use.setAttribute('href', href.replace(/#.*$/, paused ? '#play' : '#pause'));
        button.setAttribute('aria-label', `${paused ? 'Play' : 'Pause'} ${noun}`);
    };

    const setUpCarousel = () => {
        const element = document.querySelector('[data-portal-carousel]');
        if (!element || !window.bootstrap) {
            return;
        }

        const carousel = window.bootstrap.Carousel.getOrCreateInstance(element, {
            interval: 7000,
            pause: 'hover',
            ride: reducedMotion ? false : 'carousel',
        });

        const toggle = element.querySelector('[data-carousel-toggle]');
        let paused = reducedMotion;

        toggle.addEventListener('click', () => {
            paused = !paused;
            if (paused) {
                carousel.pause();
            } else {
                carousel.cycle();
            }
            showToggleState(toggle, paused, 'slideshow');
        });
    };

    const setUpTicker = () => {
        const track = document.querySelector('[data-ticker]');
        const toggle = document.querySelector('[data-ticker-toggle]');
        if (!track || !toggle) {
            return;
        }

        toggle.addEventListener('click', () => {
            const paused = track.classList.toggle('is-paused');
            showToggleState(toggle, paused, 'news');
        });
    };

    const setUpBackToTop = () => {
        const button = document.querySelector('[data-back-to-top]');
        if (!button) {
            return;
        }

        const update = () => {
            button.hidden = window.scrollY < 400;
        };

        window.addEventListener('scroll', update, { passive: true });
        update();

        button.addEventListener('click', () => {
            window.scrollTo({ top: 0, behavior: reducedMotion ? 'auto' : 'smooth' });
            document.getElementById('main')?.focus({ preventScroll: true });
        });
    };

    const setUpSignIn = () => {
        const form = document.querySelector('[data-sign-in-form]');
        if (!form) {
            return;
        }

        const toggle = form.querySelector('[data-password-toggle]');
        const password = form.querySelector('input[name="Password"]');

        toggle.addEventListener('click', () => {
            const reveal = password.type === 'password';
            const use = toggle.querySelector('use');

            password.type = reveal ? 'text' : 'password';
            use.setAttribute('href', use.getAttribute('href').replace(/#.*$/, reveal ? '#eye-off' : '#eye'));
            toggle.setAttribute('aria-pressed', String(reveal));
            toggle.setAttribute('aria-label', reveal ? 'Hide password' : 'Show password');
        });

        // Stops a double click from sending the sign-in twice.
        form.addEventListener('submit', () => {
            form.querySelector('[type="submit"]').disabled = true;
        });
    };

    const setUpConfirmations = () => {
        document.querySelectorAll('form[data-confirm]').forEach((form) => {
            form.addEventListener('submit', (event) => {
                if (!window.confirm(form.dataset.confirm)) {
                    event.preventDefault();
                }
            });
        });
    };

    // On a computer with a mouse, header menus open on hover. Touch screens and the phone menu keep tap to open.
    const setUpHoverMenus = () => {
        if (!window.bootstrap) {
            return;
        }

        const hoverMenus = window.matchMedia('(hover: hover) and (min-width: 992px)');

        document.querySelectorAll('.site-header .dropdown').forEach((item) => {
            const toggle = item.querySelector('[data-bs-toggle="dropdown"]');
            const dropdown = window.bootstrap.Dropdown.getOrCreateInstance(toggle);

            item.addEventListener('mouseenter', () => {
                if (hoverMenus.matches) {
                    dropdown.show();
                }
            });

            item.addEventListener('mouseleave', () => {
                if (hoverMenus.matches) {
                    dropdown.hide();
                }
            });

            // Otherwise a click would close the menu that the pointer has just opened.
            // Keyboard users still open it with Enter and close it with Escape.
            toggle.addEventListener('click', (event) => {
                if (hoverMenus.matches) {
                    event.preventDefault();
                    event.stopPropagation();
                    dropdown.show();
                }
            }, true);
        });
    };

    // Cards with data-pager show a few items at a time, with previous and next buttons.
    // Without scripts the controls stay hidden and every item shows.
    const setUpPagers = () => {
        document.querySelectorAll('[data-pager]').forEach((pager) => {
            const items = [...pager.querySelectorAll('[data-pager-item]')];
            const size = Number(pager.dataset.pageSize) || 1;
            const pageCount = Math.ceil(items.length / size);
            const list = pager.querySelector('[data-pager-list]');
            const controls = pager.querySelector('[data-pager-controls]');

            if (!list || !controls || pageCount < 2) {
                return;
            }

            const previous = controls.querySelector('[data-pager-prev]');
            const next = controls.querySelector('[data-pager-next]');
            const count = controls.querySelector('[data-pager-count]');
            let current = 0;

            const show = (page, direction) => {
                current = page;

                items.forEach((item, index) => {
                    const visible = Math.floor(index / size) === page;

                    item.hidden = !visible;
                    item.classList.remove('is-entering-next', 'is-entering-previous');

                    if (visible && direction && !reducedMotion) {
                        // Reading the width restarts the animation when the same item shows again.
                        void item.offsetWidth;
                        item.classList.add(`is-entering-${direction}`);
                    }
                });

                previous.disabled = page === 0;
                next.disabled = page === pageCount - 1;
                count.textContent = `${page + 1} / ${pageCount}`;
                count.setAttribute('aria-label', `Page ${page + 1} of ${pageCount}`);
            };

            // Holds the list at the height of its tallest page, so the cards below don't jump.
            const holdHeight = () => {
                const page = current;
                let tallest = 0;

                list.style.minHeight = '';

                for (let index = 0; index < pageCount; index += 1) {
                    show(index);
                    tallest = Math.max(tallest, list.offsetHeight);
                }

                show(page);
                list.style.minHeight = `${tallest}px`;
            };

            previous.addEventListener('click', () => show(current - 1, 'previous'));
            next.addEventListener('click', () => show(current + 1, 'next'));

            controls.hidden = false;
            holdHeight();

            let resizeTimer;
            window.addEventListener('resize', () => {
                window.clearTimeout(resizeTimer);
                resizeTimer = window.setTimeout(holdHeight, 150);
            });
        });
    };

    setUpCarousel();
    setUpTicker();
    setUpPagers();
    setUpBackToTop();
    setUpSignIn();
    setUpConfirmations();
    setUpHoverMenus();
})();
