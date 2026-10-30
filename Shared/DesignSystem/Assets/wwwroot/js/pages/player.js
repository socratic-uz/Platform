/**
 * Socratic Platform - Video Player Controller (Telegram WebApp & Standalone)
 */

export async function initPlayer(playerId = 'mediaPlayer', sourceId = 'videoSource', loaderId = 'loadingContainer', textId = 'loadingText') {
    try {
        if (window.Telegram?.WebApp) {
            window.Telegram.WebApp.ready();
            window.Telegram.WebApp.expand();
        }

        const urlParams = new URLSearchParams(window.location.search);
        let videoUrl = urlParams.get('video');
        let origUrl = urlParams.get('orig');

        if (!videoUrl && !origUrl) {
            let startParam = window.Telegram?.WebApp?.initDataUnsafe?.start_param || '';
            if (!startParam) {
                startParam = urlParams.get('tgWebAppStartParam') || urlParams.get('startapp') || '';
            }
            if (!startParam && window.location.hash) {
                const hashMatch = window.location.hash.match(/start_param=([A-Za-z0-9_-]+)/);
                if (hashMatch) startParam = decodeURIComponent(hashMatch[1]);
            }

            if (startParam && startParam.startsWith('p_')) {
                const shortcode = startParam.substring(2);
                origUrl = 'https://www.instagram.com/p/' + shortcode + '/';
            }
        }

        if (!videoUrl && origUrl) {
            try {
                let apiUrl = '/api/instagram/video?url=' + encodeURIComponent(origUrl);
                let res = await fetch(apiUrl).catch(() => null);
                if (!res || !res.ok) {
                    apiUrl = 'https://identity.socratic.uz/api/instagram/video?url=' + encodeURIComponent(origUrl);
                    res = await fetch(apiUrl, { mode: 'cors' }).catch(() => null);
                }
                if (res && res.ok) {
                    const data = await res.json();
                    if (data?.videoUrl) {
                        videoUrl = data.videoUrl.replace(/\\\//g, '/').replace(/\\u0026/g, '&');
                    }
                }
            } catch (err) {
                console.error('[Socratic Player] Video metadata fetch error:', err);
            }
        }

        const player = document.getElementById(playerId);
        const source = document.getElementById(sourceId);
        const loader = document.getElementById(loaderId);

        if (videoUrl && player && source) {
            source.src = videoUrl;
            player.load();
            if (loader) loader.style.display = 'none';
            player.style.display = 'block';
            player.play().catch(() => {});
        } else if (loader) {
            const text = document.getElementById(textId);
            if (text) {
                text.innerText = text.getAttribute('data-not-found') || 'Video link not found';
            }
        }
    } catch (e) {
        console.error('[Socratic Player] Initialization error:', e);
    }
}

