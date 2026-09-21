// Socratic FIDO2 / WebAuthn Passkey Helper (Zero Trust Hardware-backed Biometrics)
window.passkeyHelper = {
    isSupported: async function () {
        return window.PublicKeyCredential && 
               typeof window.PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable === 'function' &&
               await window.PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable();
    },

    register: async function (userId, userName, challengeBase64) {
        try {
            if (!await this.isSupported()) return null;

            const challenge = Uint8Array.from(atob(challengeBase64), c => c.charCodeAt(0));
            const userHandle = new TextEncoder().encode(userId);

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
                    authenticatorAttachment: "platform",
                    userVerification: "required",
                    residentKey: "preferred"
                },
                timeout: 60000,
                attestation: "none"
            };

            const credential = await navigator.credentials.create({ publicKey });
            if (!credential) return null;

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
            console.warn("[Passkey] Registration error or cancelled:", err);
            return null;
        }
    },

    authenticate: async function (challengeBase64) {
        try {
            if (!await this.isSupported()) return null;

            const challenge = Uint8Array.from(atob(challengeBase64), c => c.charCodeAt(0));

            const publicKey = {
                challenge: challenge,
                rpId: window.location.hostname,
                userVerification: "required",
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
            console.warn("[Passkey] Authentication error or cancelled:", err);
            return null;
        }
    }
};
