// Socratic OAuth & OIDC Popup Manager (Google Account Chooser Popup & Telegram OIDC Widget)
window.oauthPopupHelper = {
    openGooglePopup: function (clientId, redirectUri, state) {
        try {
            const scope = encodeURIComponent("openid email profile");
            const encRedirect = encodeURIComponent(redirectUri);
            const prompt = "select_account";
            const url = `https://accounts.google.com/o/oauth2/v2/auth?client_id=${clientId}&redirect_uri=${encRedirect}&response_type=code&scope=${scope}&state=${state}&prompt=${prompt}`;

            const width = 500;
            const height = 650;
            const left = window.screenX + (window.outerWidth - width) / 2;
            const top = window.screenY + (window.outerHeight - height) / 2;

            const features = `width=${width},height=${height},top=${top},left=${left},status=no,resizable=yes,toolbar=no,menubar=no,location=no`;
            const popup = window.open(url, "GoogleOAuthPopup", features);

            if (popup && popup.focus) {
                popup.focus();
            }
            return true;
        } catch (err) {
            console.error("[OAuthPopup] Error opening Google popup:", err);
            return false;
        }
    },

    openTelegramPopup: function (clientId, callbackUrl, state) {
        try {
            if (window.Telegram && window.Telegram.Login && window.Telegram.Login.auth) {
                window.Telegram.Login.auth({
                    client_id: clientId,
                    scope: ['openid', 'profile', 'phone'],
                    lang: (document.documentElement.lang || 'ru').substring(0, 2)
                }, function (data) {
                    if (!data) {
                        console.warn("[TelegramPopup] User closed popup or cancelled Telegram auth.");
                        return;
                    }
                    if (data.error) {
                        console.error("[TelegramPopup] Telegram auth error:", data.error);
                        var errUrl = callbackUrl + (callbackUrl.indexOf('?') >= 0 ? '&' : '?') + 'error=' + encodeURIComponent(data.error);
                        if (state) errUrl += '&state=' + encodeURIComponent(state);
                        window.location.href = errUrl;
                        return;
                    }
                    if (data.id_token) {
                        var query = 'id_token=' + encodeURIComponent(data.id_token);
                        if (data.user) {
                            if (data.user.id) query += '&id=' + encodeURIComponent(data.user.id);
                            if (data.user.preferred_username) query += '&username=' + encodeURIComponent(data.user.preferred_username);
                            if (data.user.first_name || data.user.name) query += '&first_name=' + encodeURIComponent(data.user.first_name || data.user.name);
                            if (data.user.last_name) query += '&last_name=' + encodeURIComponent(data.user.last_name);
                            if (data.user.picture) query += '&photo_url=' + encodeURIComponent(data.user.picture);
                            if (data.user.phone_number) query += '&phone_number=' + encodeURIComponent(data.user.phone_number);
                        }
                        if (state) query += '&state=' + encodeURIComponent(state);
                        window.location.href = callbackUrl + (callbackUrl.indexOf('?') >= 0 ? '&' : '?') + query;
                        return;
                    }
                    // Fallback for any legacy or custom response format
                    var query = Object.keys(data).map(function (k) {
                        return encodeURIComponent(k) + '=' + encodeURIComponent(data[k]);
                    }).join('&');

                    var fullUrl = callbackUrl + (callbackUrl.indexOf('?') >= 0 ? '&' : '?') + query + (state ? '&state=' + encodeURIComponent(state) : '');
                    window.location.href = fullUrl;
                });
            } else {
                // Fallback: Standard OpenID Connect Authorization Code URL
                var oidcUrl = 'https://oauth.telegram.org/auth?client_id=' + encodeURIComponent(clientId)
                    + '&redirect_uri=' + encodeURIComponent(callbackUrl)
                    + '&response_type=code'
                    + '&scope=' + encodeURIComponent('openid profile phone')
                    + (state ? '&state=' + encodeURIComponent(state) : '');
                window.location.href = oidcUrl;
            }
        } catch (err) {
            console.error("[TelegramPopup] Error opening Telegram popup:", err);
            var oidcUrl = 'https://oauth.telegram.org/auth?client_id=' + encodeURIComponent(clientId)
                + '&redirect_uri=' + encodeURIComponent(callbackUrl)
                + '&response_type=code'
                + '&scope=' + encodeURIComponent('openid profile phone')
                + (state ? '&state=' + encodeURIComponent(state) : '');
            window.location.href = oidcUrl;
        }
    },

    notifyAndClose: function (returnUrl) {
        try {
            if (window.opener && !window.opener.closed) {
                try {
                    window.opener.location.href = returnUrl || "/";
                } catch (e) {
                    window.opener.location.reload();
                }
                window.close();
                return;
            }
        } catch (e) {
            console.warn("[OAuthPopup] Could not access window.opener:", e);
        }
        window.location.href = returnUrl || "/";
    }
};
