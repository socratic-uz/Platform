/**
 * Beautiful QR Code styling helper using qr-code-styling library.
 */

const QR_CODE_STYLING_URL = "https://unpkg.com/qr-code-styling@1.5.0/lib/qr-code-styling.js";

async function ensureQrCodeStylingLoaded() {
    if (typeof QRCodeStyling !== "undefined") {
        return;
    }
    // Check if script is already in the document
    let script = document.querySelector(`script[src="${QR_CODE_STYLING_URL}"]`);
    if (!script) {
        script = document.createElement("script");
        script.src = QR_CODE_STYLING_URL;
        document.head.appendChild(script);
    }
    return new Promise((resolve, reject) => {
        const onLoad = () => {
            script.removeEventListener("load", onLoad);
            resolve();
        };
        const onError = (e) => {
            script.removeEventListener("error", onError);
            reject(new Error("Failed to load qr-code-styling library"));
        };
        script.addEventListener("load", onLoad);
        script.addEventListener("error", onError);
        // Double check
        if (typeof QRCodeStyling !== "undefined") {
            resolve();
        }
    });
}

export async function generateStyledQrCode(data, logoUrl, options) {
    console.log("[QR JS Debug] generateStyledQrCode called. data:", data, "logoUrl:", logoUrl, "options:", options);
    await ensureQrCodeStylingLoaded();

    options = options || {};
    const size = options.size || options.Size || 320;
    const primaryColor = options.primaryColor || options.PrimaryColor || "#3f51b5";
    const backgroundColor = options.backgroundColor || options.BackgroundColor || "#ffffff";
    const cornerColor = options.cornerColor || options.CornerColor || "#1a237e";
    const cornerDotColor = options.cornerDotColor || options.CornerDotColor || "#3f51b5";
    const dotType = options.dotType || options.DotType || "rounded";
    const cornerType = options.cornerType || options.CornerType || "extra-rounded";
    const cornerDotType = options.cornerDotType || options.CornerDotType || "dot";
    const imageSizeFactor = typeof options.imageSizeFactor === "number" ? options.imageSizeFactor : (typeof options.ImageSizeFactor === "number" ? options.ImageSizeFactor : 0.4);
    const margin = typeof options.margin === "number" ? options.margin : (typeof options.Margin === "number" ? options.Margin : 6);

    const useBackgroundGradient = options.useBackgroundGradient || options.UseBackgroundGradient || false;
    const backgroundGradientType = options.backgroundGradientType || options.BackgroundGradientType || "linear";
    const backgroundGradientRotation = typeof options.backgroundGradientRotation === "number" ? options.backgroundGradientRotation : (typeof options.BackgroundGradientRotation === "number" ? options.BackgroundGradientRotation : 0);
    const backgroundGradientStartColor = options.backgroundGradientStartColor || options.BackgroundGradientStartColor || "#ffffff";
    const backgroundGradientEndColor = options.backgroundGradientEndColor || options.BackgroundGradientEndColor || "#eceff1";

    return new Promise((resolve) => {
        const container = document.createElement("div");

        // Convert image to base64 to embed it directly as a data URL inside the SVG.
        // This prevents print composition failures and external fetch stalls.
        const getBase64Image = (url) => {
            return new Promise((res) => {
                if (!url) {
                    res(null);
                    return;
                }
                const img = new Image();
                img.crossOrigin = "anonymous";
                img.onload = () => {
                    try {
                        const canvas = document.createElement("canvas");
                        canvas.width = img.naturalWidth;
                        canvas.height = img.naturalHeight;
                        const ctx = canvas.getContext("2d");
                        ctx.drawImage(img, 0, 0);
                        res(canvas.toDataURL("image/png"));
                    } catch (e) {
                        console.warn("[QR Helper] Canvas conversion failed, using direct URL:", e);
                        res(url); // Fallback to direct URL if canvas security checks fail
                    }
                };
                img.onerror = () => {
                    console.warn("[QR Helper] Failed to load logo URL:", url);
                    res(null);
                };

                // Safety timeout to avoid hanging if the image server is unresponsive
                const timer = setTimeout(() => {
                    img.src = "";
                    res(null);
                }, 1500);

                img.src = url;
            });
        };

        getBase64Image(logoUrl).then((embeddedLogoUrl) => {
            const qrCode = new QRCodeStyling({
                width: size,
                height: size,
                type: "svg",
                data: data,
                image: embeddedLogoUrl || "",
                dotsOptions: {
                    color: primaryColor,
                    type: dotType
                },
                backgroundOptions: {
                    color: useBackgroundGradient ? undefined : backgroundColor,
                    gradient: useBackgroundGradient ? {
                        type: backgroundGradientType,
                        rotation: backgroundGradientRotation * Math.PI / 180,
                        colorStops: [
                            { offset: 0, color: backgroundGradientStartColor },
                            { offset: 1, color: backgroundGradientEndColor }
                        ]
                    } : null
                },
                imageOptions: {
                    crossOrigin: "anonymous",
                    margin: margin,
                    imageSizeFactor: imageSizeFactor
                },
                cornersSquareOptions: {
                    type: cornerType,
                    color: cornerColor
                },
                cornersDotOptions: {
                    type: cornerDotType,
                    color: cornerDotColor
                }
            });

            qrCode.append(container);

            setTimeout(() => {
                const svg = container.querySelector("svg");
                if (svg) {
                    // Make SVG responsive via CSS but keep absolute attributes for print engine stability
                    if (!svg.getAttribute("viewBox")) {
                        svg.setAttribute("viewBox", `0 0 ${size} ${size}`);
                    }
                    svg.setAttribute("width", size);
                    svg.setAttribute("height", size);
                    svg.style.display = "block";

                    try {
                        // Serialize SVG and convert to base64 data URL.
                        // This renders the SVG inside an <img> tag, which isolates its styling
                        // and prevents Chromium print engine bugs (like filling the entire QR code with solid blue).
                        const svgString = new XMLSerializer().serializeToString(svg);
                        const svgBase64 = btoa(unescape(encodeURIComponent(svgString)));
                        const imgHtml = `<img src="data:image/svg+xml;base64,${svgBase64}" style="display: block; width: 100%; height: 100%;" />`;
                        resolve(imgHtml);
                    } catch (e) {
                        console.warn("[QR Helper] SVG base64 conversion failed, falling back to raw SVG:", e);
                        resolve(container.innerHTML);
                    }
                } else {
                    resolve(container.innerHTML);
                }
            }, 50);
        });
    });
}
