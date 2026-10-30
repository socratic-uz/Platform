import {
    state,
    MAP_CONTAINER_ID
} from './map-libre-state.js';

import {
    ensureSourcesAndLayers,
    syncAllSources,
    applyOverlayVisibility,
    fitToBounds
} from './map-libre-layers.js';

export async function applyMapTiltInternal(isEnabled, animate = true) {
    await ensureMap(state.activeStyleUrl);
    if (!state.map) return false;

    state.mapTiltEnabled = !!isEnabled;
    const nextPitch = state.mapTiltEnabled ? 60 : 0;
    state.map.easeTo({ pitch: nextPitch, duration: animate ? 260 : 0 });
    return state.mapTiltEnabled;
}

export async function setMapTiltEnabled(isEnabled) {
    return applyMapTiltInternal(!!isEnabled, false);
}

export async function toggleMapTilt() {
    return applyMapTiltInternal(!state.mapTiltEnabled, true);
}



export function getFallbackStyle() {
    return {
        version: 8,
        sources: {
            osm: {
                type: "raster",
                tiles: ["https://tile.openstreetmap.org/{z}/{x}/{y}.png"],
                tileSize: 256,
                attribution: "&copy; OpenStreetMap contributors"
            }
        },
        layers: [
            {
                id: "osm",
                type: "raster",
                source: "osm"
            }
        ]
    };
}

let libraryLoadingPromise = null;

export function loadMapLibre() {
    if (window.maplibregl) return Promise.resolve();
    if (libraryLoadingPromise) return libraryLoadingPromise;

    libraryLoadingPromise = new Promise((resolve, reject) => {
        if (!document.querySelector('link[href*="maplibre-gl.css"]')) {
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = 'https://unpkg.com/maplibre-gl@5.6.2/dist/maplibre-gl.css';
            document.head.appendChild(link);
        }

        const script = document.querySelector('script[src*="maplibre-gl.js"]');
        if (script) {
            if (window.maplibregl) {
                resolve();
            } else {
                script.addEventListener('load', resolve);
                script.addEventListener('error', reject);
            }
            return;
        }

        const newScript = document.createElement('script');
        newScript.src = 'https://unpkg.com/maplibre-gl@5.6.2/dist/maplibre-gl.js';
        newScript.async = true;
        newScript.onload = () => resolve();
        newScript.onerror = (err) => reject(err);
        document.body.appendChild(newScript);
    });

    return libraryLoadingPromise;
}

export async function ensureMap(styleUrl) {
    const container = document.getElementById(MAP_CONTAINER_ID);
    if (!container) {
        return null;
    }

    if (state.map && state.mapReady) {
        if (state.map.getContainer() === container && container.isConnected) {
            return state.map;
        } else {
            console.log("Map container changed or unmounted. Resetting map instance...");
            try {
                state.map.remove();
            } catch (e) {}
            state.map = null;
            state.mapReady = false;
            state.mapReadyPromise = null;
        }
    }

    if (state.mapReadyPromise) {
        try {
            await state.mapReadyPromise;
            if (state.map && state.map.getContainer() === container && container.isConnected) {
                return state.map;
            }
        } catch (e) {
            return null;
        }
    }

    if (!window.maplibregl?.Map) {
        try {
            await loadMapboxDraw(); // ensure draw is loaded if needed, or maplibre
            await loadMapLibre();
        } catch (loadError) {
            console.error("Failed to load MapLibre GL runtime dynamically:", loadError);
            return null;
        }
    }

    const containerAfterLoad = document.getElementById(MAP_CONTAINER_ID);
    if (!containerAfterLoad) {
        return null;
    }

    state.mapReadyPromise = new Promise((resolve, reject) => {
        const style = styleUrl ? styleUrl : getFallbackStyle();
        state.activeStyleUrl = typeof style === "string" ? style : "";
        console.log("Initializing MapLibre with style:", typeof style === 'string' ? style : 'inline object');

        let initResolved = false;
        const resolveReady = () => {
            if (initResolved) return;
            if (!state.map) return;

            initResolved = true;
            state.mapReady = true;

            const applyStyleAddons = () => {
                try {
                    if (state.map && typeof state.map.setPitch === 'function') {
                        state.map.setPitch(state.mapTiltEnabled ? 60 : 0);
                    }
                    if (state.map && typeof state.map.setProjection === 'function') {
                        state.map.setProjection({ type: 'globe' });
                    }
                    if (state.map && typeof state.map.setSky === 'function') {
                        state.map.setSky({
                            "sky-color": "#0b0b14",
                            "horizon-color": "#1a1a3e",
                            "fog-color": "#0b0b14",
                            "sky-horizon-blend": 0.5,
                            "horizon-fog-blend": 0.8,
                            "fog-ground-blend": 0.9,
                            "atmosphere-blend": [
                                "interpolate",
                                ["linear"],
                                ["zoom"],
                                0, 1.0,
                                6, 1.0,
                                10, 0.0
                            ]
                        });
                    }
                } catch (e) {
                    console.warn("Globe/Sky setup deferred:", e?.message || e);
                }
            };

            if (typeof state.map.isStyleLoaded === 'function' && state.map.isStyleLoaded()) {
                applyStyleAddons();
            } else {
                try {
                    state.map.once("idle", applyStyleAddons);
                } catch (e) {
                    applyStyleAddons();
                }
            }

            ensureSourcesAndLayers();
            resolve();
        };

        try {
            state.map = new window.maplibregl.Map({
                container: MAP_CONTAINER_ID,
                style,
                center: [69.2401, 41.2995],
                zoom: 11,
                antialias: true,
                pitchWithRotate: true,
                dragRotate: true,
                touchPitch: true,
                attributionControl: false
            });

            state.map.on("load", () => {
                console.log("MapLibre 'load' event fired");
                resolveReady();
            });

            state.map.on("style.load", () => {
                console.log("MapLibre 'style.load' event fired");
                resolveReady();
            });

            state.map.on("styledata", () => {
                if (!state.mapReady && typeof state.map?.isStyleLoaded === "function" && state.map.isStyleLoaded()) {
                    resolveReady();
                }
            });

            state.map.on("error", (error) => {
                console.error("MapLibre error event:", error?.message || error);
            });

            setTimeout(() => {
                if (!initResolved && state.map) {
                    console.warn("MapLibre load timeout fallback triggered");
                    resolveReady();
                }
            }, 2000);

        } catch (initError) {
            console.error("MapLibre Map constructor error:", initError);
            reject(initError);
            return;
        }
    }).catch((error) => {
        state.mapReadyPromise = null;
        state.mapReady = false;
        console.error("Map initialization promise failed:", error);
        throw error;
    });

    try {
        await state.mapReadyPromise;
        console.log("Map successfully initialized, mapReady =", state.mapReady);
    } catch (error) {
        console.error("Failed to initialize map:", error);
    }

    return state.map;
}

export async function initializeMap(styleUrl) {
    const normalizedStyleUrl = String(styleUrl || "").trim();
    state.activeStyleUrl = normalizedStyleUrl;

    try {
        await ensureMap(normalizedStyleUrl || undefined);
        const isReady = state.map && state.mapReady;
        console.log("Map initialization result:", { isReady, mapReady: state.mapReady, hasMap: !!state.map });
        return isReady;
    } catch (error) {
        console.error("Failed to initialize map:", error);
        return false;
    }
}

export async function setBaseMapStyle(styleUrl) {
    const normalizedStyleUrl = String(styleUrl || "").trim();

    try {
        await ensureMap(normalizedStyleUrl || undefined);
        if (!state.map) {
            console.error("Map instance not initialized");
            return false;
        }

        if (state.activeStyleUrl === normalizedStyleUrl) {
            return true;
        }

        const styleToApply = normalizedStyleUrl ? normalizedStyleUrl : getFallbackStyle();
        state.activeStyleUrl = normalizedStyleUrl;

        if (state.mapReady) {
            try {
                state.map.setStyle(styleToApply);
                state.map.once("style.load", () => {
                    state.mapReady = true;
                    ensureSourcesAndLayers();
                    syncAllSources();
                    applyOverlayVisibility();
                });
                console.log("Map style applied successfully", normalizedStyleUrl || "fallback OSM");
            } catch (styleError) {
                console.error("Failed to apply style:", styleError);
                if (normalizedStyleUrl) {
                    try {
                        state.map.setStyle(getFallbackStyle());
                        state.activeStyleUrl = "";
                        state.map.once("style.load", () => {
                            state.mapReady = true;
                            ensureSourcesAndLayers();
                            syncAllSources();
                            applyOverlayVisibility();
                        });
                        console.log("Fallback style applied after error");
                    } catch (fallbackError) {
                        console.error("Fallback style also failed:", fallbackError);
                        return false;
                    }
                }
            }
        }

        return true;
    } catch (error) {
        console.error("Error in setBaseMapStyle:", error);
        return false;
    }
}

let drawLoadingPromise = null;
export function loadMapboxDraw() {
    if (window.MapboxDraw) return Promise.resolve();
    if (drawLoadingPromise) return drawLoadingPromise;

    drawLoadingPromise = new Promise((resolve, reject) => {
        if (!document.querySelector('link[href*="mapbox-gl-draw.css"]')) {
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = 'https://api.mapbox.com/mapbox-gl-js/plugins/mapbox-gl-draw/v1.4.3/mapbox-gl-draw.css';
            document.head.appendChild(link);
        }

        const script = document.createElement('script');
        script.src = 'https://api.mapbox.com/mapbox-gl-js/plugins/mapbox-gl-draw/v1.4.3/mapbox-gl-draw.js';
        script.async = true;
        script.onload = () => resolve();
        script.onerror = (err) => reject(err);
        document.body.appendChild(script);
    });

    return drawLoadingPromise;
}

function geojsonFeaturesToWkt(features) {
    if (!features || features.length === 0) return "";
    
    const polygons = [];
    for (const feature of features) {
        if (!feature || !feature.geometry) continue;
        const type = feature.geometry.type.toUpperCase();
        const coords = feature.geometry.coordinates;

        if (type === "POLYGON" && coords && coords.length > 0) {
            const rings = coords.map(ring => {
                if (!ring || ring.length === 0) return "";
                const pointsStr = ring.map(pt => {
                    if (!pt || pt.length < 2 || isNaN(pt[0]) || isNaN(pt[1])) return "";
                    return `${pt[0]} ${pt[1]}`;
                }).filter(s => s !== "").join(", ");
                return pointsStr ? "(" + pointsStr + ")" : "";
            }).filter(r => r !== "");
            if (rings.length > 0) {
                polygons.push(`(${rings.join(", ")})`);
            }
        }
    }

    if (polygons.length === 0) return "";
    if (polygons.length === 1) return `POLYGON${polygons[0]}`;
    return `MULTIPOLYGON(${polygons.join(", ")})`;
}

function wktToGeoJsonFeatures(wkt) {
    if (!wkt) return [];

    if (wkt.toUpperCase().includes("MULTIPOLYGON")) {
        const polyMatches = wkt.matchAll(/\(\s*\(\s*([^)]+)\s*\)\s*\)/gi);
        const features = [];
        for (const match of polyMatches) {
            const coordsStr = match[1];
            const coordinates = [
                coordsStr.split(',').map(ptStr => {
                    const parts = ptStr.trim().split(/\s+/);
                    const lng = parseFloat(parts[0]);
                    const lat = parseFloat(parts[1]);
                    if (isNaN(lng) || isNaN(lat)) return null;
                    return [lng, lat];
                }).filter(pt => pt !== null)
            ];
            if (coordinates[0] && coordinates[0].length > 0) {
                features.push({
                    type: "Feature",
                    geometry: { type: "Polygon", coordinates },
                    properties: {}
                });
            }
        }
        return features;
    } else {
        const match = wkt.match(/POLYGON\s*\(\s*\(?([^()]+)\)?\s*\)/i);
        if (!match) return [];

        const coordsStr = match[1];
        const coordinates = [
            coordsStr.split(',').map(ptStr => {
                const parts = ptStr.trim().split(/\s+/);
                const lng = parseFloat(parts[0]);
                const lat = parseFloat(parts[1]);
                if (isNaN(lng) || isNaN(lat)) return null;
                return [lng, lat];
            }).filter(pt => pt !== null)
        ];

        if (!coordinates[0] || coordinates[0].length === 0) return [];

        return [{
            type: "Feature",
            geometry: {
                type: "Polygon",
                coordinates: coordinates
            },
            properties: {}
        }];
    }
}

export async function startPolygonDraw(wkt) {
    state.isPolygonDrawActive = true;
    await loadMapboxDraw();

    await ensureMap(state.activeStyleUrl);
    if (!state.isPolygonDrawActive || !state.map) return;

    if (state.draw) {
        try {
            state.map.removeControl(state.draw);
        } catch (e) { }
    }

    const defaultMode = (wkt && wkt.trim().length > 0) ? 'simple_select' : 'draw_polygon';

    state.draw = new window.MapboxDraw({
        displayControlsDefault: false,
        controls: {
            polygon: true,
            trash: true
        },
        defaultMode: defaultMode
    });

    state.map.addControl(state.draw);

    if (wkt) {
        const features = wktToGeoJsonFeatures(wkt);
        const allLngs = [];
        const allLats = [];
        for (const feat of features) {
            state.draw.add(feat);
            if (feat.geometry && feat.geometry.coordinates[0]) {
                for (const pt of feat.geometry.coordinates[0]) {
                    allLngs.push(pt[0]);
                    allLats.push(pt[1]);
                }
            }
        }
        if (allLngs.length > 0 && allLats.length > 0) {
            const minLng = Math.min(...allLngs);
            const maxLng = Math.max(...allLngs);
            const minLat = Math.min(...allLats);
            const maxLat = Math.max(...allLats);
            fitToBounds(minLng, minLat, maxLng, maxLat, 80, 15);
        }
    }

    const updateDrawnPolygon = () => {
        if (!state.draw) return;
        const data = state.draw.getAll();
        if (data && data.features && data.features.length > 0) {
            const wktResult = geojsonFeaturesToWkt(data.features);
            if (state.activeDotNetRef) {
                state.activeDotNetRef.invokeMethodAsync("OnPolygonChanged", wktResult);
            }
        } else {
            if (state.activeDotNetRef) {
                state.activeDotNetRef.invokeMethodAsync("OnPolygonChanged", "");
            }
        }
    };

    if (state._drawCreateHandler) {
        state.map.off('draw.create', state._drawCreateHandler);
        state.map.off('draw.update', state._drawUpdateHandler);
        state.map.off('draw.delete', state._drawDeleteHandler);
        state.map.off('draw.selectionchange', state._drawCreateHandler);
        state.map.off('draw.modechange', state._drawCreateHandler);
    }

    state._drawCreateHandler = updateDrawnPolygon;
    state._drawUpdateHandler = updateDrawnPolygon;
    state._drawDeleteHandler = updateDrawnPolygon;

    state.map.on('draw.create', state._drawCreateHandler);
    state.map.on('draw.update', state._drawUpdateHandler);
    state.map.on('draw.delete', state._drawDeleteHandler);
    state.map.on('draw.selectionchange', state._drawCreateHandler);
    state.map.on('draw.modechange', state._drawCreateHandler);
}

export async function stopPolygonDraw() {
    state.isPolygonDrawActive = false;
    if (state.map && state.draw) {
        try {
            state.map.removeControl(state.draw);
        } catch (e) { }
        state.draw = null;
    }
    if (state.map && state._drawCreateHandler) {
        state.map.off('draw.create', state._drawCreateHandler);
        state.map.off('draw.update', state._drawUpdateHandler);
        state.map.off('draw.delete', state._drawDeleteHandler);
        state.map.off('draw.selectionchange', state._drawCreateHandler);
        state.map.off('draw.modechange', state._drawCreateHandler);
        state._drawCreateHandler = null;
        state._drawUpdateHandler = null;
        state._drawDeleteHandler = null;
    }
}

export async function destroyMap() {
    if (state.map) {
        try {
            state.map.remove();
        } catch (e) {
            console.error("Error removing map instance:", e);
        }
        state.map = null;
    }
    state.mapReady = false;
    state.mapReadyPromise = null;
    state.draw = null;
    state.isPolygonDrawActive = false;
    state._drawCreateHandler = null;
    state._drawUpdateHandler = null;
    state._drawDeleteHandler = null;
}

export function resizeMap() {
    if (state.map) {
        try {
            state.map.resize();
        } catch (e) {}
    }
}





