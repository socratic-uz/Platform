export const remoteSupport = {
    init: function (dotNetHelper) {
        console.log("[JS] remoteSupport.init called!");
        const element = document.getElementById("webrtc-video-preview");
        if (!element) {
            console.log("[JS] remoteSupport.init: element 'webrtc-video-preview' not found in DOM!");
            return;
        }
        if (element.dataset.rsInit === "true") {
            console.log("[JS] remoteSupport.init: element already initialized");
            return;
        }
        element.dataset.rsInit = "true";
        console.log("[JS] remoteSupport.init: binding click event listener to element");

        element.addEventListener('click', function (e) {
            console.log("[JS] Click registered on video element");
            var rect = element.getBoundingClientRect();
            var x = e.clientX - rect.left;
            var y = e.clientY - rect.top;
            
            var pctX = (x / rect.width) * 100.0;
            var pctY = (y / rect.height) * 100.0;
            
            dotNetHelper.invokeMethodAsync('OnImageClick', pctX, pctY);
        });
    }
};

export const remoteSupportWebRtc = {
    pc: null,
    localStream: null,
    dotNetHelper: null,
    pointerTimeout: null,
    iceQueue: [],

    log: function (msg) {
        console.log(msg);
        if (this.dotNetHelper) {
            this.dotNetHelper.invokeMethodAsync('LogFromJs', msg);
        }
    },

    // --- TWO-WAY AUDIO/VIDEO WEB-TO-WEB CALLS ---
    startCall: async function (dotNetHelper, isVideo) {
        this.dotNetHelper = dotNetHelper;
        this.iceQueue = [];
        this.log("[Call] Initiating outbound call, isVideo = " + isVideo);

        const config = { iceServers: [{ urls: "stun:stun.l.google.com:19302" }] };
        this.pc = new RTCPeerConnection(config);

        this.pc.onicecandidate = (event) => {
            if (event.candidate) {
                this.dotNetHelper.invokeMethodAsync('SendIceCandidateToPartner', JSON.stringify(event.candidate));
            }
        };

        this.pc.onconnectionstatechange = () => {
            this.log("[Call] Connection state: " + this.pc.connectionState);
            this.dotNetHelper.invokeMethodAsync('OnJsConnectionStateChanged', this.pc.connectionState);
        };

        this.pc.ontrack = (event) => {
            this.log("[Call] Remote track received: " + event.track.kind);
            const videoElement = document.getElementById("webrtc-video-preview");
            if (videoElement && event.streams && event.streams[0]) {
                videoElement.srcObject = event.streams[0];
                videoElement.play().catch(e => this.log("[Call] Play failed: " + e.message));
            }
        };

        try {
            this.localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: isVideo });
            this.log("[Call] Local media stream acquired");
            const localVideo = document.getElementById("webrtc-local-video");
            if (localVideo && isVideo) {
                localVideo.srcObject = this.localStream;
                localVideo.play().catch(e => this.log("[Call] Local play failed: " + e.message));
            }
        } catch (e) {
            this.log("[Call] Media acquisition failed: " + e.message);
            this.dotNetHelper.invokeMethodAsync('OnCaptureFailed', e.message);
            return;
        }

        this.localStream.getTracks().forEach(track => {
            this.pc.addTrack(track, this.localStream);
        });

        const offer = await this.pc.createOffer();
        await this.pc.setLocalDescription(offer);
        this.log("[Call] SDP Offer created");
        this.dotNetHelper.invokeMethodAsync('SendOfferToPartner', offer.sdp);
    },

    joinCall: async function (dotNetHelper, isVideo) {
        this.dotNetHelper = dotNetHelper;
        this.iceQueue = [];
        this.log("[Call] Joining inbound call, isVideo = " + isVideo);

        const config = { iceServers: [{ urls: "stun:stun.l.google.com:19302" }] };
        this.pc = new RTCPeerConnection(config);

        this.pc.onicecandidate = (event) => {
            if (event.candidate) {
                this.dotNetHelper.invokeMethodAsync('SendIceCandidateToPartner', JSON.stringify(event.candidate));
            }
        };

        this.pc.onconnectionstatechange = () => {
            this.log("[Call] Connection state: " + this.pc.connectionState);
            this.dotNetHelper.invokeMethodAsync('OnJsConnectionStateChanged', this.pc.connectionState);
        };

        this.pc.ontrack = (event) => {
            this.log("[Call] Remote track received: " + event.track.kind);
            const videoElement = document.getElementById("webrtc-video-preview");
            if (videoElement && event.streams && event.streams[0]) {
                videoElement.srcObject = event.streams[0];
                videoElement.play().catch(e => this.log("[Call] Play failed: " + e.message));
            }
        };

        try {
            this.localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: isVideo });
            this.log("[Call] Local media stream acquired");
            const localVideo = document.getElementById("webrtc-local-video");
            if (localVideo && isVideo) {
                localVideo.srcObject = this.localStream;
                localVideo.play().catch(e => this.log("[Call] Local play failed: " + e.message));
            }
        } catch (e) {
            this.log("[Call] Media acquisition failed: " + e.message);
            this.dotNetHelper.invokeMethodAsync('OnCaptureFailed', e.message);
            return;
        }

        this.localStream.getTracks().forEach(track => {
            this.pc.addTrack(track, this.localStream);
        });
    },

    // --- CLIENT ROLE (Shares Screen) ---
    startHost: async function (dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        this.iceQueue = [];
        this.log("[Host] startHost initiated");
        
        try {
            this.localStream = await navigator.mediaDevices.getDisplayMedia({ video: true });
            this.log("[Host] Screen capture stream acquired");
        } catch (e) {
            this.log("[Host] Screen capture failed: " + e.message);
            this.dotNetHelper.invokeMethodAsync('OnCaptureFailed', e.message);
            return;
        }

        const config = { iceServers: [{ urls: "stun:stun.l.google.com:19302" }] };
        this.pc = new RTCPeerConnection(config);
        this.log("[Host] RTCPeerConnection created");

        this.localStream.getTracks().forEach(track => {
            this.pc.addTrack(track, this.localStream);
            this.log("[Host] Track added: " + track.kind);
        });

        this.pc.onicecandidate = (event) => {
            if (event.candidate) {
                this.log("[Host] Local ICE candidate generated");
                this.dotNetHelper.invokeMethodAsync('SendIceCandidateToPartner', JSON.stringify(event.candidate));
            }
        };

        this.pc.onconnectionstatechange = () => {
            this.log("[Host] Connection state: " + this.pc.connectionState);
            this.dotNetHelper.invokeMethodAsync('OnJsConnectionStateChanged', this.pc.connectionState);
        };

        const offer = await this.pc.createOffer();
        this.log("[Host] SDP Offer created");
        await this.pc.setLocalDescription(offer);
        this.log("[Host] Local description set");

        this.dotNetHelper.invokeMethodAsync('SendOfferToPartner', offer.sdp);
        this.createPointerElement();
    },

    // --- OPERATOR ROLE (Controls / Views Screen) ---
    startViewer: async function (dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        this.iceQueue = [];
        this.log("[Viewer] startViewer initiated");

        const config = { iceServers: [{ urls: "stun:stun.l.google.com:19302" }] };
        this.pc = new RTCPeerConnection(config);
        this.log("[Viewer] RTCPeerConnection created");

        this.pc.onicecandidate = (event) => {
            if (event.candidate) {
                this.log("[Viewer] Local ICE candidate generated");
                this.dotNetHelper.invokeMethodAsync('SendIceCandidateToPartner', JSON.stringify(event.candidate));
            }
        };

        this.pc.onconnectionstatechange = () => {
            this.log("[Viewer] Connection state: " + this.pc.connectionState);
            this.dotNetHelper.invokeMethodAsync('OnJsConnectionStateChanged', this.pc.connectionState);
        };

        this.pc.ontrack = (event) => {
            this.log("[Viewer] Track received: " + (event.track ? event.track.kind : "unknown"));
            const videoElement = document.getElementById("webrtc-video-preview");
            if (videoElement && event.streams && event.streams[0]) {
                videoElement.srcObject = event.streams[0];
                this.log("[Viewer] Bound stream to videoElement");
                videoElement.play().then(() => {
                    this.log("[Viewer] Video playback started successfully");
                }).catch(e => {
                    this.log("[Viewer] Video playback failed: " + e.message);
                });
            } else {
                this.log("[Viewer] videoElement not found or no stream tracks!");
            }
        };
    },

    // --- COMMON SIGNALING METHODS ---
    handleReceiveOffer: async function (sdp) {
        if (!this.pc) return;
        this.log("[Common] Received SDP Offer, applying remote description...");
        const desc = new RTCSessionDescription({ type: 'offer', sdp: sdp });
        await this.pc.setRemoteDescription(desc);
        this.log("[Common] Remote description applied");
        
        this.processIceQueue();

        const answer = await this.pc.createAnswer();
        this.log("[Common] SDP Answer created");
        await this.pc.setLocalDescription(answer);
        this.log("[Common] Local description applied");

        this.dotNetHelper.invokeMethodAsync('SendAnswerToPartner', answer.sdp);
    },

    handleReceiveAnswer: async function (sdp) {
        if (!this.pc) return;
        this.log("[Common] Received SDP Answer, applying remote description...");
        const desc = new RTCSessionDescription({ type: 'answer', sdp: sdp });
        await this.pc.setRemoteDescription(desc);
        this.log("[Common] Remote description applied");
        
        this.processIceQueue();
    },

    handleAddIceCandidate: async function (candidateJson) {
        if (!this.pc) return;
        const candidateInit = JSON.parse(candidateJson);
        
        this.log("[Common] Received partner ICE Candidate");
        if (this.pc.remoteDescription && this.pc.remoteDescription.type) {
            await this.pc.addIceCandidate(new RTCIceCandidate(candidateInit));
            this.log("[Common] ICE Candidate applied immediately");
        } else {
            this.iceQueue.push(candidateInit);
            this.log("[Common] ICE Candidate queued (remoteDescription missing)");
        }
    },

    processIceQueue: async function () {
        while (this.iceQueue.length > 0) {
            const candidateInit = this.iceQueue.shift();
            try {
                await this.pc.addIceCandidate(new RTCIceCandidate(candidateInit));
                this.log("[Common] Queued ICE Candidate applied");
            } catch (e) {
                this.log("[Common] Queued ICE Candidate error: " + e.message);
            }
        }
    },

    // --- VISUAL POINTER (For Client/Host) ---
    createPointerElement: function () {
        let pointer = document.getElementById("webrtc-pointer");
        if (!pointer) {
            pointer = document.createElement("div");
            pointer.id = "webrtc-pointer";
            pointer.style.cssText = "position: fixed; width: 30px; height: 30px; border-radius: 50%; border: 3px solid #ff4a4a; background: rgba(255, 74, 74, 0.4); pointer-events: none; transform: translate(-50%, -50%); transition: all 0.15s ease-out; display: none; z-index: 99999; box-shadow: 0 0 15px #ff4a4a; animation: webrtc-pulse 1.2s infinite ease-in-out;";
            
            const style = document.createElement('style');
            style.type = 'text/css';
            style.innerHTML = '@keyframes webrtc-pulse { 0% { transform: translate(-50%, -50%) scale(0.9); opacity: 0.9; } 50% { transform: translate(-50%, -50%) scale(1.2); opacity: 0.6; } 100% { transform: translate(-50%, -50%) scale(0.9); opacity: 0.9; } }';
            document.getElementsByTagName('head')[0].appendChild(style);
            document.body.appendChild(pointer);
        }
    },

    showPointer: function (pctX, pctY) {
        const pointer = document.getElementById("webrtc-pointer");
        if (!pointer) return;

        const x = (pctX / 100) * window.innerWidth;
        const y = (pctY / 100) * window.innerHeight;

        pointer.style.left = x + 'px';
        pointer.style.top = y + 'px';
        pointer.style.display = 'block';

        clearTimeout(this.pointerTimeout);
        this.pointerTimeout = setTimeout(() => {
            pointer.style.display = 'none';
        }, 3000);
    },

    close: function () {
        this.iceQueue = [];
        if (this.localStream) {
            this.localStream.getTracks().forEach(track => track.stop());
            this.localStream = null;
        }
        if (this.pc) {
            this.pc.close();
            this.pc = null;
        }
        const pointer = document.getElementById("webrtc-pointer");
        if (pointer) {
            pointer.style.display = 'none';
        }
    }
};

export function init(dotNetHelper) {
    return remoteSupport.init(dotNetHelper);
}

export function startCall(dotNetHelper, isVideo) {
    return remoteSupportWebRtc.startCall(dotNetHelper, isVideo);
}

export function joinCall(dotNetHelper, isVideo) {
    return remoteSupportWebRtc.joinCall(dotNetHelper, isVideo);
}

export function startHost(dotNetHelper) {
    return remoteSupportWebRtc.startHost(dotNetHelper);
}

export function startViewer(dotNetHelper) {
    return remoteSupportWebRtc.startViewer(dotNetHelper);
}

export function handleReceiveOffer(sdp) {
    return remoteSupportWebRtc.handleReceiveOffer(sdp);
}

export function handleReceiveAnswer(sdp) {
    return remoteSupportWebRtc.handleReceiveAnswer(sdp);
}

export function handleAddIceCandidate(candidateJson) {
    return remoteSupportWebRtc.handleAddIceCandidate(candidateJson);
}

export function showPointer(pctX, pctY) {
    return remoteSupportWebRtc.showPointer(pctX, pctY);
}

export function close() {
    return remoteSupportWebRtc.close();
}

if (typeof window !== "undefined") {
    window.remoteSupport = remoteSupport;
    window.remoteSupportWebRtc = remoteSupportWebRtc;
}

