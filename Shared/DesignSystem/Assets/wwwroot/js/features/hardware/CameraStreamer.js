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
                        if (this._callInvoke)
                            this._video.ontimeupdate = (ev) => this.onFrame(ev);
                        this._video.onloadedmetadata = (ev) => __awaiter(this, void 0, void 0, function* () {
                            try {
                                yield this._video.play();
                                this._streamActive = true;
                            } catch (e) {
                                console.warn("[CameraStreamer] Error playing video stream:", e);
                            }
                        });
                        if (this._video.readyState >= 1) {
                            this._video.play().catch(e => console.warn("[CameraStreamer] Error playing video stream:", e));
                            this._streamActive = true;
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
                                if (this._callInvoke)
                                    this._video.ontimeupdate = (ev) => this.onFrame(ev);
                                this._video.play().catch(e => console.warn("[CameraStreamer] Fallback play error:", e));
                                this._streamActive = true;
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
                // Используйте метод pause, поскольку в интерфейсе HTMLVideoElement нет метода stop
                (_a = this._video) === null || _a === void 0 ? void 0 : _a.pause();
                // Остановите все треки потока (без этого поток все равно будет обрабатываться, и браузер покажет, что камера все еще используется сайтом)
                (_b = this._stream) === null || _b === void 0 ? void 0 : _b.getTracks().forEach(t => t.stop());
                this._streamActive = false;
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
            }
            /**
             * Вызывает объект dotnet на предоставленном методе с предоставленными данными
             * @param data Строка, которую получает метод dotnet
             */
            invokeDotnetObject(data) {
                if (this._callInvoke)
                    this._dotnetObject.invokeMethodAsync(this._invokeIdentifier, data);
            }
            getCurrentCanvasFrame() {
                if (!this._video) return "";
                let canvas = document.createElement("canvas");
                let w = this._video.videoWidth || (this._width || 640);
                let h = this._video.videoHeight || (this._height || 360);
                if (w === 0 || h === 0) return "";
                canvas.width = w;
                canvas.height = h;
                // Нарисуйте текущее изображение потока на холсте
                let ctx = canvas.getContext("2d");
                if (!ctx) return "";
                ctx.drawImage(this._video, 0, 0, w, h);
                // Получить изображение как строку 64base
                return canvas.toDataURL("image/png");
            }
            /**
             * Обрабатывает событие ontimeupdate видео и вызывает объект dotnet с img
             */
            onFrame(ev) {
                if (this._callInvoke && this._streamActive)
                    this.invokeDotnetObject(this._lastFrame = this.getCurrentCanvasFrame());
            }
        }
        Scripts.CameraStreamerInterop = CameraStreamerInterop;
    })(Scripts = JSInterop.Scripts || (JSInterop.Scripts = {}));
})(JSInterop || (JSInterop = {}));
