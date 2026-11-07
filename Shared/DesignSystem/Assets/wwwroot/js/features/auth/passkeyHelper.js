// Socratic FIDO2 / WebAuthn Passkey Helper (Zero Trust Hardware-backed Biometrics)
window.passkeyHelper = {
    isSupported: async function () {
        if (!window.PublicKeyCredential) {
            console.warn("[Passkey] PublicKeyCredential is not supported in this browser.");
            return false;
        }
        try {
            if (typeof window.PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable === 'function') {
                const available = await window.PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable();
                return available;
            }
            return true;
        } catch (e) {
            console.warn("[Passkey] Error checking platform authenticator:", e);
            return false;
        }
    },

    register: async function (userId, userName, challengeBase64) {
        try {
            console.log("[Passkey] Starting Passkey registration for User:", userId, "Name:", userName);
            if (!window.PublicKeyCredential) {
                alert("Ваш браузер или устройство не поддерживает Passkeys (WebAuthn).");
                return null;
            }

            const challenge = Uint8Array.from(atob(challengeBase64), c => c.charCodeAt(0));
            const userHandle = new TextEncoder().encode(userId || "socratic-user");

            const publicKey = {
                challenge: challenge,
                rp: {
                    name: "Socratic Identity Platform",
                    id: window.location.hostname
                },
                user: {
                    id: userHandle,
                    name: userName || "user",
                    displayName: userName || "Socratic User"
                },
                pubKeyCredParams: [
                    { alg: -7, type: "public-key" },  // ES256
                    { alg: -257, type: "public-key" } // RS256
                ],
                authenticatorSelection: {
                    userVerification: "preferred",
                    residentKey: "preferred"
                },
                timeout: 60000,
                attestation: "none"
            };

            console.log("[Passkey] Calling navigator.credentials.create with publicKey options...");
            const credential = await navigator.credentials.create({ publicKey });
            if (!credential) {
                console.warn("[Passkey] User cancelled credential creation or no credential returned.");
                return null;
            }

            console.log("[Passkey] Credential successfully created:", credential.id);

            const rawId = btoa(String.fromCharCode(...new Uint8Array(credential.rawId)));
            const clientDataJSON = btoa(String.fromCharCode(...new Uint8Array(credential.response.clientDataJSON)));
            const attestationObject = btoa(String.fromCharCode(...new Uint8Array(credential.response.attestationObject)));

            return JSON.stringify({
                id: credential.id,
                rawId: rawId,
                clientDataJSON: clientDataJSON,
                attestationObject: attestationObject
            });
        } catch (err) {
            console.error("[Passkey] Registration error:", err);
            alert("Ошибка регистрации Passkey: " + (err.message || err.name));
            return null;
        }
    },

    authenticate: async function (challengeBase64) {
        try {
            console.log("[Passkey] Starting Passkey authentication...");
            if (!window.PublicKeyCredential) return null;

            const challenge = Uint8Array.from(atob(challengeBase64), c => c.charCodeAt(0));

            const publicKey = {
                challenge: challenge,
                rpId: window.location.hostname,
                userVerification: "preferred",
                timeout: 60000
            };

            const assertion = await navigator.credentials.get({ publicKey });
            if (!assertion) return null;

            const rawId = btoa(String.fromCharCode(...new Uint8Array(assertion.rawId)));
            const clientDataJSON = btoa(String.fromCharCode(...new Uint8Array(assertion.response.clientDataJSON)));
            const authenticatorData = btoa(String.fromCharCode(...new Uint8Array(assertion.response.authenticatorData)));
            const signature = btoa(String.fromCharCode(...new Uint8Array(assertion.response.signature)));
            const userHandle = assertion.response.userHandle ? btoa(String.fromCharCode(...new Uint8Array(assertion.response.userHandle))) : "";

            return JSON.stringify({
                id: assertion.id,
                rawId: rawId,
                clientDataJSON: clientDataJSON,
                authenticatorData: authenticatorData,
                signature: signature,
                userHandle: userHandle
            });
        } catch (err) {
            console.error("[Passkey] Authentication error:", err);
            return null;
        }
    }
};
