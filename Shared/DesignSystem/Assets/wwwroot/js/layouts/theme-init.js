/**
 * Socratic Platform - Early Theme Initialization
 * Executes synchronously in <head> to prevent FOUC (flash of unstyled content).
 */
(function() {
    try {
        const cookieTheme = document.cookie.split('; ').find(row => row.startsWith('Theme='));
        const theme = (cookieTheme ? decodeURIComponent(cookieTheme.split('=')[1]) : null) || 
                      localStorage.getItem('Theme') || 
                      (window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches ? 'Light' : 'Dark');
        const normalized = theme.toLowerCase();
        document.documentElement.setAttribute('data-theme', normalized);
        document.documentElement.style.colorScheme = normalized;
    } catch (_) {}
})();

