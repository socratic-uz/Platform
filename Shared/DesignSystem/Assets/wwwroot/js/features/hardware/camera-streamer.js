var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var JSInterop;
(function (JSInterop) {
    var Scripts;
    (function (Scripts) {
        /**
         * Этот класс предназначен для работы с Microsoft.UI.Infrastructure в C# Blazor и помогает транслировать веб-камеры в элемент видео
         */
        class CameraStreamerInterop {
            constructor() {
                /**
                 * Активен ли поток в данный момент
                 */
                this._streamActive = false;
            }
            /**
             * Возвращает новый экземпляр класса CameraStreamerInterop
             */
            static createInstance() {
                return new CameraStreamerInterop();
            }
            /**
             * Инициализирует поток камеры - без инициализации поток не будет работать
             * @param video Элемент ссылки видео.
             * @param api Ссылка на объект dotnet, который должен получать обратные вызовы
             * @param camera Строка устройства (id) камеры, которая должна использоваться для потока.
             */
            init(video, callOnFrameInvoke, api = null, onFrameInvokeName = null, width = 640, height = 360) {
                this._video = video;
                this._dotnetObject = api;
                this._invokeIdentifier = onFrameInvokeName;
                this._callInvoke = this._invokeIdentifier === null || this._dotnetObject === null ? false : callOnFrameInvoke;
                this._width = width || 640;
                this._height = height || 360;
                this._constraints = {
                    audio: false,
                    video: {
                        width: { ideal: this._width },
                        height: { ideal: this._height }
                    }
                };
            }
            /**
             * Запускает поток камеры. Должен быть указан идентификатор устройства камеры
             * @param cameraId Идентификатор устройства камеры.
             */
            start(cameraId) {
                // Сначала остановите поток, если он уже активен
                if (this._streamActive)
                    this.stop();

                var videoConstraints = {
                    width: { ideal: this._width || 640 },
                    height: { ideal: this._height || 360 }
                };

                if (cameraId && typeof cameraId === 'string' && cameraId.trim().length > 0) {
                    videoConstraints.deviceId = { exact: cameraId.trim() };
                }

                this._constraints = {
                    audio: false,
                    video: videoConstraints
                };

                if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                    console.error("[CameraStreamer] navigator.mediaDevices.getUserMedia is unavailable. HTTPS or localhost is required.");
                    return;
                }

                navigator.mediaDevices.getUserMedia(this._constraints).then(mediaStream => {
                    this._stream = mediaStream;
                    if (this._video) {
                        this._video.srcObject = this._stream;
                        this._video.onloadedmetadata = (ev) => __awaiter(this, void 0, void 0, function* () {
                            try {
                                yield this._video.play();
                                this._streamActive = true;
                                if (this._callInvoke) this.startFrameLoop();
                            } catch (e) {
                                console.warn("[CameraStreamer] Error playing video stream:", e);
                            }
                        });
                        if (this._video.readyState >= 1) {
                            this._video.play().catch(e => console.warn("[CameraStreamer] Error playing video stream:", e));
                            this._streamActive = true;
                            if (this._callInvoke) this.startFrameLoop();
                        }
                    }
                }).catch(err => {
                    console.warn("[CameraStreamer] Failed to open camera with specified constraints:", err);
                    if (videoConstraints.deviceId) {
                        delete videoConstraints.deviceId;
                        navigator.mediaDevices.getUserMedia({ audio: false, video: true }).then(mediaStream => {
                            this._stream = mediaStream;
                            if (this._video) {
                                this._video.srcObject = this._stream;
                                this._video.play().catch(e => console.warn("[CameraStreamer] Fallback play error:", e));
                                this._streamActive = true;
                                if (this._callInvoke) this.startFrameLoop();
                            }
                        }).catch(err2 => console.error("[CameraStreamer] Fallback getUserMedia also failed:", err2));
                    }
                });
            }
            /**
             * Останавливает поток камеры (последний кадр все равно будет показан в элементе видео)
             */
            stop() {
                var _a, _b;
                this.stopFrameLoop();
                this.stopWebRtc();
                (_a = this._video) === null || _a === void 0 ? void 0 : _a.pause();
                (_b = this._stream) === null || _b === void 0 ? void 0 : _b.getTracks().forEach(t => t.stop());
                this._streamActive = false;
                this._inFlight = false;
                if (this._video !== null)
                    this._video.ontimeupdate = null;
            }
            /**
             * Изменяет текущую камеру (если камера такая же, как в данный момент, ничего не произойдет)
             * @param newId
             */
            changeCamera(newId) {
                // Не запускайте поток снова, если камера все еще та же
                if (this._constraints && this._constraints.video && this._constraints.video.deviceId && this._constraints.video.deviceId.exact === newId)
                    return;
                // Просто повторный вызов метода start изменит камеру, которая используется
                this.start(newId);
            }
            /**
             * Проверяет, имеет ли сайт доступ к камере (камерам) и запрашивает его, если доступ в настоящее время запрещен
             * @returns может ли сайт получить доступ к камере
             */
            static getCameraAccess() {
                return __awaiter(this, void 0, void 0, function* () {
                    try {
                        yield navigator.mediaDevices.getUserMedia({ video: true });
                    }
                    catch (_a) {
                        return false;
                    }
                    return true;
                });
            }
            /**
             * Получает все медиа-устройства типа 'videoinput' и возвращает их в виде массива MediaDeviceInfo
             */
            static getCameraDeviceList() {
                return navigator.mediaDevices.enumerateDevices()
                    // Дождитесь, пока все устройства будут перечислены
                    .then(l => l
                        // Отфильтровывает все видеовходы (поток не поддерживает аудио)
                        .filter(d => d.kind === "videoinput"));
            }
            getCurrentFrame() {
                return (this._callInvoke && this._streamActive) ?
                    Promise.resolve(this._lastFrame) :
                    Promise.resolve(this.getCurrentCanvasFrame());
            }
            /**
             * Освобождает все ресурсы и останавливает поток. Объект должен быть повторно инициализирован, прежде чем его можно будет использовать снова
             */
            dispose() {
                this.stop();
                // Установите переменные в null
                this._video = null;
                this._dotnetObject = null;
                this._stream = null;
                this._constraints = null;
                this._lastFrame = null;
                this._canvas = null;
                this._ctx = null;
            }
            /**
             * Вызывает объект dotnet на предоставленном методе с предоставленными данными
             * @param data Строка, которую получает метод dotnet
             */
            invokeDotnetObject(data) {
                if (!this._callInvoke || !this._dotnetObject) return Promise.resolve();
                return this._dotnetObject.invokeMethodAsync(this._invokeIdentifier, data);
            }
            setBinaryEndpoint(endpoint) {
                this._binaryEndpoint = endpoint || null;
            }
            drawVideoToCanvas() {
                if (!this._video) return false;
                let w = this._video.videoWidth || (this._width || 640);
                let h = this._video.videoHeight || (this._height || 360);
                if (w === 0 || h === 0) return false;
                if (!this._canvas) {
                    this._canvas = document.createElement("canvas");
                    this._ctx = this._canvas.getContext("2d", { alpha: false });
                }
                if (this._canvas.width !== w) this._canvas.width = w;
                if (this._canvas.height !== h) this._canvas.height = h;
                if (!this._ctx) return false;
                this._ctx.drawImage(this._video, 0, 0, w, h);
                return true;
            }
            getCurrentCanvasFrame() {
                if (!this.drawVideoToCanvas() || !this._canvas) return "";
                return this._canvas.toDataURL("image/jpeg", 0.72);
            }
            ensureOverlayCanvas() {
                if (!this._video || !this._video.parentElement) return null;
                if (!this._overlayCanvas) {
                    const c = document.createElement("canvas");
                    c.style.position = "absolute";
                    c.style.inset = "0";
                    c.style.width = "100%";
                    c.style.height = "100%";
                    c.style.pointerEvents = "none";
                    this._video.parentElement.appendChild(c);
                    this._overlayCanvas = c;
                    this._overlayCtx = c.getContext("2d");
                }
                return this._overlayCanvas;
            }
            drawOverlayBoxes(data) {
                const c = this.ensureOverlayCanvas();
                if (!c || !this._overlayCtx) return;
                const w = data.w || this._video.videoWidth || 640;
                const h = data.h || this._video.videoHeight || 360;
                if (c.width !== w) c.width = w;
                if (c.height !== h) c.height = h;
                const ctx = this._overlayCtx;
                ctx.clearRect(0, 0, w, h);
                const boxes = data.boxes || [];
                if (!boxes.length) return;
                ctx.font = "600 12px Roboto, sans-serif";
                for (let i = 0; i < boxes.length; i++) {
                    const b = boxes[i];
                    const x = (b.x / 100) * w;
                    const y = (b.y / 100) * h;
                    const bw = (b.w / 100) * w;
                    const bh = (b.h / 100) * h;
                    ctx.strokeStyle = "rgba(130, 224, 170, 0.92)";
                    ctx.lineWidth = 2;
                    ctx.fillStyle = "rgba(130, 224, 170, 0.08)";
                    ctx.beginPath();
                    ctx.roundRect(x, y, bw, bh, 8);
                    ctx.fill();
                    ctx.stroke();

                    const label = `${b.l} (${Math.round((b.s || 0) * 100)} %)`;
                    const tw = ctx.measureText(label).width;
                    const px = x + 6;
                    const py = y + 6;
                    ctx.fillStyle = "rgba(24, 28, 32, 0.82)";
                    ctx.beginPath();
                    ctx.roundRect(px, py, tw + 16, 22, 999);
                    ctx.fill();
                    ctx.fillStyle = "#82e0aa";
                    ctx.fillText(label, px + 8, py + 15);
                }
            }
            startWebRtc(endpoint) {
                return __awaiter(this, void 0, void 0, function* () {
                    this.stopWebRtc();
                    if (!this._stream) return;
                    const track = this._stream.getVideoTracks()[0];
                    if (!track) return;
                    const token = (this._rtcToken = (this._rtcToken || 0) + 1);
                    const pc = new RTCPeerConnection({
                        iceServers: [
                            { urls: "stun:netbird.socratic.uz:3478" },
                            { urls: "stun:stun.l.google.com:19302" }
                        ]
                    });
                    this._pc = pc;
                    const dc = pc.createDataChannel("boxes");
                    this._dc = dc;
                    let frames = 0;
                    let fps = 0;
                    let lastAt = 0;
                    let lastReportAt = 0;
                    let gotFrame = false;
                    const triggerFallback = () => {
                        if (gotFrame || token !== this._rtcToken || !this._streamActive) return;
                        console.warn("[CameraStreamer] WebRTC ICE/DataChannel timeout; falling back to /api/detector/frame");
                        this.stopWebRtc();
                        this._binaryEndpoint = "/api/detector/frame";
                        this.startFrameLoop();
                    };
                    const fallbackTimer = setTimeout(triggerFallback, 3500);
                    this._rtcFallbackTimer = fallbackTimer;
                    pc.oniceconnectionstatechange = () => {
                        if (pc.iceConnectionState === "failed") triggerFallback();
                    };
                    dc.onmessage = (ev) => {
                        if (token !== this._rtcToken || !this._streamActive) return;
                        if (!gotFrame) {
                            gotFrame = true;
                            clearTimeout(fallbackTimer);
                        }
                        try {
                            const data = JSON.parse(ev.data);
                            frames++;
                            const now = performance.now();
                            if (lastAt > 0) {
                                const inst = 1000 / Math.max(1, now - lastAt);
                                fps = fps === 0 ? inst : (fps * 0.8 + inst * 0.2);
                            }
                            lastAt = now;
                            requestAnimationFrame(() => {
                                if (token === this._rtcToken && this._streamActive)
                                    this.drawOverlayBoxes(data);
                            });
                            if (now - lastReportAt >= 350) {
                                lastReportAt = now;
                                this.invokeDotnetObject(JSON.stringify({
                                    ms: data.ms,
                                    w: data.w,
                                    h: data.h,
                                    fps: Math.round(fps * 10) / 10,
                                    frames: frames,
                                    boxes: []
                                }));
                            }
                        } catch {}
                    };
                    pc.addTrack(track, this._stream);
                    const offer = yield pc.createOffer();
                    yield pc.setLocalDescription(offer);
                    if (pc.iceGatheringState !== "complete") {
                        yield new Promise((resolve) => {
                            const timer = setTimeout(resolve, 1200);
                            pc.onicegatheringstatechange = () => {
                                if (pc.iceGatheringState === "complete") {
                                    clearTimeout(timer);
                                    resolve();
                                }
                            };
                        });
                    }
                    if (token !== this._rtcToken || !this._streamActive) return;
                    const resp = yield fetch(endpoint, {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ sdp: pc.localDescription.sdp })
                    });
                    if (!resp.ok || token !== this._rtcToken || !this._streamActive) return;
                    const ans = yield resp.json();
                    this._rtcSessionId = ans.sessionId;
                    yield pc.setRemoteDescription({ type: "answer", sdp: ans.sdp });
                });
            }
            stopWebRtc() {
                this._rtcToken = (this._rtcToken || 0) + 1;
                if (this._rtcFallbackTimer) { clearTimeout(this._rtcFallbackTimer); this._rtcFallbackTimer = 0; }
                if (this._dc) { try { this._dc.close(); } catch {} this._dc = null; }
                if (this._pc) { try { this._pc.close(); } catch {} this._pc = null; }
                if (this._rtcSessionId) {
                    const sid = this._rtcSessionId;
                    this._rtcSessionId = null;
                    try { fetch(`/api/detector/webrtc/${sid}`, { method: "DELETE", keepalive: true }); } catch {}
                }
                if (this._overlayCanvas) {
                    if (this._overlayCtx)
                        this._overlayCtx.clearRect(0, 0, this._overlayCanvas.width, this._overlayCanvas.height);
                    this._overlayCanvas.remove();
                    this._overlayCanvas = null;
                    this._overlayCtx = null;
                }
            }
            startFrameLoop() {
                this.stopFrameLoop();
                if (this._binaryEndpoint && this._binaryEndpoint.endsWith("/webrtc/offer")) {
                    this.startWebRtc(this._binaryEndpoint).catch(e => console.warn("[CameraStreamer] WebRTC start error:", e));
                    return;
                }
                this._loopToken = (this._loopToken || 0) + 1;
                const token = this._loopToken;
                const tick = () => {
                    if (token !== this._loopToken || !this._streamActive || !this._video) return;
                    if (this._callInvoke && !this._inFlight && this._video.readyState >= 2) {
                        if (this._binaryEndpoint) {
                            if (this.drawVideoToCanvas() && this._canvas) {
                                this._inFlight = true;
                                this._canvas.toBlob(async (blob) => {
                                    try {
                                        if (!blob || token !== this._loopToken || !this._streamActive) return;
                                        const resp = await fetch(this._binaryEndpoint, { method: "POST", body: blob });
                                        if (resp.ok) {
                                            const json = await resp.text();
                                            await this.invokeDotnetObject(json);
                                        }
                                    } catch {}
                                    finally {
                                        this._inFlight = false;
                                    }
                                }, "image/jpeg", 0.72);
                            }
                        } else {
                            const frame = this.getCurrentCanvasFrame();
                            if (frame) {
                                this._lastFrame = frame;
                                this._inFlight = true;
                                Promise.resolve(this.invokeDotnetObject(frame))
                                    .catch(() => {})
                                    .finally(() => { this._inFlight = false; });
                            }
                        }
                    }
                    if (typeof this._video.requestVideoFrameCallback === "function") {
                        this._rvfcId = this._video.requestVideoFrameCallback(tick);
                    } else {
                        this._rafId = requestAnimationFrame(tick);
                    }
                };
                if (typeof this._video.requestVideoFrameCallback === "function") {
                    this._rvfcId = this._video.requestVideoFrameCallback(tick);
                } else {
                    this._rafId = requestAnimationFrame(tick);
                }
            }
            stopFrameLoop() {
                this._loopToken = (this._loopToken || 0) + 1;
                if (this._video && this._rvfcId && typeof this._video.cancelVideoFrameCallback === "function") {
                    try { this._video.cancelVideoFrameCallback(this._rvfcId); } catch {}
                    this._rvfcId = 0;
                }
                if (this._rafId) {
                    cancelAnimationFrame(this._rafId);
                    this._rafId = 0;
                }
            }
            onFrame(ev) {
                if (this._callInvoke && this._streamActive && !this._inFlight)
                    this.invokeDotnetObject(this._lastFrame = this.getCurrentCanvasFrame());
            }
        }
        Scripts.CameraStreamerInterop = CameraStreamerInterop;
    })(Scripts = JSInterop.Scripts || (JSInterop.Scripts = {}));
})(JSInterop || (JSInterop = {}));
