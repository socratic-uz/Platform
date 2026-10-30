// Socratic Telegram Login Helper (Official Telegram OIDC & Seamless Fallback)
window.telegramLoginHelper = {
    login: function (clientId, callbackUrl, state) {
        if (window.oauthPopupHelper && window.oauthPopupHelper.openTelegramPopup) {
            window.oauthPopupHelper.openTelegramPopup(clientId, callbackUrl, state);
        } else {
            var oidcUrl = 'https://oauth.telegram.org/auth?client_id=' + encodeURIComponent(clientId)
                + '&redirect_uri=' + encodeURIComponent(callbackUrl)
                + '&response_type=code'
                + '&scope=' + encodeURIComponent('openid profile phone')
                + (state ? '&state=' + encodeURIComponent(state) : '');
            window.location.href = oidcUrl;
        }
    }
};
