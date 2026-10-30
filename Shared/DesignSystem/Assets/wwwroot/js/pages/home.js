let _dotNetHelper;

export function addHandlers(dotNetHelper) {
    if (dotNetHelper) {
        _dotNetHelper = dotNetHelper;
    }
    if (isScrollHandlerActive) return;

    isScrollHandlerActive = true;
    lastScrollY = window.scrollY;
    hasReachedBottom = false;

    window.onscroll = function () {
        const currentScrollY = window.scrollY;
        const windowHeight = window.innerHeight;
        const documentHeight = document.documentElement.scrollHeight;
        
        const isScrollingDown = currentScrollY > lastScrollY;
        const isAtBottom = (windowHeight + currentScrollY) >= (documentHeight);
        
        if (isScrollingDown && isAtBottom && !hasReachedBottom && _dotNetHelper) {
            hasReachedBottom = true;
            _dotNetHelper.invokeMethodAsync('LoadData').then(() => {
                setTimeout(() => {
                    hasReachedBottom = false;
                }, 1000);
            }).catch(() => {
                hasReachedBottom = false;
            });
        }

        lastScrollY = currentScrollY;
    };
}

export function removeHandlers() {
    window.onscroll = null;
    isScrollHandlerActive = false;
    lastScrollY = 0;
    hasReachedBottom = false;
}

let lastScrollY = 0;
let isScrollHandlerActive = false;
let hasReachedBottom = false;
